using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class TerrainGen : MonoBehaviour
{
    public PlayerMovement player;
    public CamController cam;
    [Header("Tile Dictionary")]
    public TileDict tileDict;
    public float seed;
    public BiomeClass[] biomes;
    [Header("Cave Gen")]
    //TerrainSculptInfluence influences how extreme terrain is. 
    public float terrainSculptInfluence = 0.25f;
    //Boolean allows for switching on and off caves, mainly for testing
    public bool generateCaves = true;
    //public float caveFrequency = 0.05f;
    [Header("Overworld Gen")]
    public int chunkSize = 16;
    public int worldSize = 200;
    //Determines frequency of caves vs terrain
    public float terrainFrequency = 0.05f;
    public float worldHeightMultiplier = 5f;
    //Add height to world
    public int heightAddition = 25;
    //Biomes implemented same as caves using Perlin Noise. With each 'cave' being a biome
    [Header("Biomes")]
    public float biomeRarity;
    public Gradient biomeGradient;
    public Texture2D biomeMap;
    [Header("Seed Gen")] 
    //Visual Perlin Noise Map in Unity for caves
    public Texture2D caveNoiseTexture;
    [Header("Ore Gen")]
    public OreClass[] ores;
    //Create Array to store each chunk in the world
    public GameObject[] worldChunks; 
    //Arrat to keep track of tiles and their x,y pos 
    public List<Vector2> worldTiles = new List<Vector2>();
    //Holds actual game object for each tile in world
    public List<GameObject> worldTileObjects = new List<GameObject>();
    private BiomeClass curBiome;
    //All this function does is constantly refreshes unity editor. This is helpful for seeing perlin noise maps change when values are edited. 
    private void OnValidate()
    {
        DrawTextures();
    }
    private void Start()
    {
        //Generate random world seed. This is used to generate random world terrain. 
        seed = Random.Range(-10000, 10000);
        //Draw Perlin Noise Maps for biomes, caves, and ores 
        DrawTextures();
        //Divide world into chunks, mainly used for occlusion culling
        spawnChunks();
        //Generate actual terrain/caves
        GenerateWorld();
        //Spawn playerBody into world
        player.Spawn();
        //Spawn in camera and connect it to players position
        cam.Spawn(new Vector3(player.spawnPoint.x, player.spawnPoint.y, cam.transform.position.z));
        //Set bounds of camera to world bounds 
        cam.worldSize = worldSize;
        //This achieves occlusion culling, constantly updating to check which chunks player is in/near
        RefreshChunks();
    }

    private void Update()
    {
        RefreshChunks();
    }

    //This function achieves Occlusion Culling. This will keep only chunks near the player spawned in, improving performance.
    void RefreshChunks()
    {
        //Parse through each chunk
        for (int i = 0; i < worldChunks.Length; i++)
        {
            //Calculate distance from player to the chunk. If that distance is further than the cameras range, turn chunk off.
            if (Vector2.Distance(new Vector2((i * chunkSize)+(chunkSize/2), 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 3.5f)
            {
                //Turn off chunk
                worldChunks[i].SetActive(false);
            }
            else 
            {
                //Turn on chunk
                worldChunks[i].SetActive(true);
            }
        }

    }
    //This function draws the perlin map and displays it in Unity editor
    public void DrawTextures() 
    {
        //Create 2D texture to hold Perlin Noise Map
        biomeMap = new Texture2D(worldSize, worldSize);
        drawBiomeTexture();
        //For each biome
        for (int i = 0; i < biomes.Length; i++)
        {
            //Create cave noise map for each biome, allowing for biomes to have different cave settings
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            //For each ore in current biome
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                //Create ore spread Perlin Noise Maps for current biome
                biomes[i].ores[o].spreadMap = new Texture2D(worldSize, worldSize);
            }
            //Generate terrain perlin noise map based on current biome settings
            GenerateNoiseTexture(biomes[i].caveFrequency, biomes[i].terrainFrequency, biomes[i].caveNoiseTexture); 
            //Generate ore perlin noise maps for current biome
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                GenerateNoiseTexture(biomes[i].ores[o].rarity, biomes[i].ores[o].veinSize, biomes[i].ores[o].spreadMap);
            }                     
        }       
    }

    public void drawBiomeTexture() 
    {

        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                //(y+seed) * biome rarity: makes it so biomes can spawn at different y levels aswell. 
                float v = Mathf.PerlinNoise((x + seed) * biomeRarity, (y+seed) * biomeRarity);
                Color color = biomeGradient.Evaluate(v);
                //Insert color value into array based on randomly generated noise texture 
                //This color value is used to generate caves, ores, and biomes
                biomeMap.SetPixel(x, y, color);
            }

            biomeMap.Apply();
        }
    }

    public void spawnChunks() 
    {
        int chunkCt = worldSize / chunkSize;
        worldChunks = new GameObject[chunkCt];
        for (int i = 0; i < chunkCt; i++) {
            //Create chunk game object depending on world size and chunk size
            GameObject worldChunk = new GameObject();
            //Rename each game object chunk to make identifying a specific chunk easier 
            worldChunk.name = i.ToString();
            //Place chunks underneath world gen in hierarchy 
            worldChunk.transform.parent = this.transform;
            //Add each chunk to worldChunks array
            worldChunks[i] = worldChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y) 
    {
        //Change curBiome value here
        //Search through biomes 
        for (int i = 0; i < biomes.Length; i++) 
        {
            
            if (biomes[i].biomeCol == biomeMap.GetPixel(x,y)) 
            {
                return biomes[i];              
            }

        }
        return curBiome;
    }

    public void GenerateWorld()
    {
        Sprite[] tileSprites;
        //For loop representing x axis (width) of world
        for (int x = 0; x < worldSize; x++)
        {
            //Find current biome based on which biome map the current pixel is found in, achieved by GetCurrentBiome()
            curBiome = GetCurrentBiome(x, 0);
            //Height of world
            float height = Mathf.PerlinNoise((x + seed) * curBiome.terrainFrequency, seed * curBiome.terrainFrequency) * curBiome.worldHeightMultiplier + heightAddition;
            if (x == worldSize / 2) 
            {
                //Set player spawn point to middle of world
                player.spawnPoint = new Vector2(x, height + 25);
            }
            //For loop representing y axis (height) of world
            for (int y = 0; y < height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                //Start generating stone after dirtLayerHeight for current biome is passed. 
                if (y < height - curBiome.dirtLayerHeight)
                {
                    //Find stone texture for current biome
                    tileSprites = curBiome.tileDict.stone.tileSprites;
                    //Place ores based on their perlin noise maps and spawn parameters for height
                    if (ores[0].spreadMap.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                            tileSprites = tileDict.coal.tileSprites;                       
                    if (ores[1].spreadMap.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)                        
                            tileSprites = tileDict.iron.tileSprites;                      
                    if (ores[2].spreadMap.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)                       
                            tileSprites = tileDict.gold.tileSprites;                      
                    if (ores[3].spreadMap.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                            tileSprites = tileDict.diamond.tileSprites;                     
                }
                //Spawn dirt layer
                else if (y < height - 1)
                {
                    //Get dirt texture for current biome. This would be sand for desert and snow for Taiga. 
                    tileSprites = curBiome.tileDict.dirt.tileSprites;
                }
                else 
                {
                    //Spawn grass on top block of world
                    tileSprites = curBiome.tileDict.grass.tileSprites;                                         
                }
                //Check to see if caves should generate based on boolean val
                if (generateCaves)
                {
                    //Decide if current pixel should be a 'cave' based on randomly generated PN value. 
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        placeBlock(tileSprites, x, y);
                    }
                }
                else 
                //Else not a cave, place a normal tile
                {
                    placeBlock(tileSprites, x, y);
                }
                //Roll  to see if tree generates 
                //Check to make sure tree doesnt spawn floating
                if (y >= height-1)
                {
                    //Roll random number based on current biomes tree chance
                    int t = Random.Range(0, curBiome.genTreeChance);
                    if (t == 1)
                    {
                        //Generate tree. Check to see tree will generate on existing tile
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {                        
                            //Set rare chance for tree to spawn as a birch tree
                            if (Random.Range(0, curBiome.birchTreeChance) == 1)
                            {
                                generateBirchTree(Random.Range(curBiome.birchTreeMinHeight, curBiome.birchTreeMaxHeight), x, y + 1);
                            }
                            else
                            {
                                generateOakTree(Random.Range(curBiome.oakTreeMinHeight, curBiome.oakTreeMaxHeight), x, y + 1);

                            }
                        }
                    }
                    //If not tree generates, roll to see if tall grass generates. 
                    else { 
                        int i = Random.Range(0, curBiome.tallGrassChance);
                        if (i == 1) {
                            //Generate tall grass and mushrooms stones and cacti
                            if (worldTiles.Contains(new Vector2(x , y)))
                            {                              
                                if (curBiome.tileDict.tallGrass != null)
                                {
                                    //Place tall grass tile based on biome. Desert has dead grass. Taiga has no tall grass. 
                                    placeBlock(curBiome.tileDict.tallGrass.tileSprites, x, y + 1);
                                }
                            }
                        }
                        

                    }
                }
            }
        }
    }


    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture) 
    {
        //For each 'X' in Perlin Noise Map
        for (int x = 0; x < noiseTexture.width; x++) 
        {
            //For each 'Y' in Perlin Noise Map
            for (int y = 0; y < noiseTexture.height; y++)
            {
                //Set Perlin Noise value of current pixel at (X,Y) to a random value based on the world seed
                float v = Mathf.PerlinNoise((x+seed) * frequency, (y+seed) * frequency);
                //Insert color value into array based on randomly generated noise texture 
                //This color value is used to generate cave and ore spreads
                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }
        //Copy changes made in texture to the GPU 
        noiseTexture.Apply();
    }

    void generateOakTree(int treeHeight, int x, int y)
    {
        //Generate random number of Oak logs to build tree
        for (int i = 0; i < treeHeight; i++) 
        {
            placeBlock(tileDict.oakLog.tileSprites, x, y+i);
            
        }
        //Generate tree leaves based on height of tree
        placeBlock(tileDict.oakLeaf.tileSprites, x, y + treeHeight);
        placeBlock(tileDict.oakLeaf.tileSprites, x + 1, y + treeHeight);
        placeBlock(tileDict.oakLeaf.tileSprites, x - 1, y + treeHeight);
        placeBlock(tileDict.oakLeaf.tileSprites, x, y + treeHeight + 1);
        placeBlock(tileDict.oakLeaf.tileSprites, x + 1, y + treeHeight + 1);
        placeBlock(tileDict.oakLeaf.tileSprites, x - 1, y + treeHeight + 1);
        placeBlock(tileDict.oakLeaf.tileSprites, x, y + treeHeight + 2);

        //Add more leaves if tree is taller than set value 
        if (treeHeight > 8) {
            placeBlock(tileDict.oakLeaf.tileSprites, x, y + treeHeight + 3);
            placeBlock(tileDict.oakLeaf.tileSprites, x-1, y + treeHeight + 3);
            placeBlock(tileDict.oakLeaf.tileSprites, x+1, y + treeHeight + 3);
            placeBlock(tileDict.oakLeaf.tileSprites, x - 2, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprites, x + 2, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprites, x - 3, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprites, x + 3, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprites, x - 2, y + treeHeight+ 1);
            placeBlock(tileDict.oakLeaf.tileSprites, x + 2, y + treeHeight+ 1);
            placeBlock(tileDict.oakLeaf.tileSprites, x, y + treeHeight + 4);
            placeBlock(tileDict.oakLeaf.tileSprites, x, y + treeHeight + 5);
            placeBlock(tileDict.oakLeaf.tileSprites, x + 1, y + treeHeight +2);
            placeBlock(tileDict.oakLeaf.tileSprites, x - 1, y + treeHeight +2);
        }

    }

    void generateBirchTree(int treeHeight, int x, int y) 
    {
        //Generate random number of Birch logs to build tree
        for (int i = 0; i < treeHeight; i++)
        {
            placeBlock(tileDict.birchLog.tileSprites, x, y + i);
        }
        //Generate tree leaves based on height of tree
        placeBlock(tileDict.birchLeaf.tileSprites, x, y + treeHeight);
        placeBlock(tileDict.birchLeaf.tileSprites, x + 1, y + treeHeight);
        placeBlock(tileDict.birchLeaf.tileSprites, x - 1, y + treeHeight);
        placeBlock(tileDict.birchLeaf.tileSprites, x, y + treeHeight + 1);
        placeBlock(tileDict.birchLeaf.tileSprites, x + 1, y + treeHeight + 1);
        placeBlock(tileDict.birchLeaf.tileSprites, x - 1, y + treeHeight + 1);
        placeBlock(tileDict.birchLeaf.tileSprites, x, y + treeHeight + 2);

        //Add more leaves if tree is taller than set value 
        if (treeHeight > 8)
        {
            placeBlock(tileDict.birchLeaf.tileSprites, x, y + treeHeight + 3);
            placeBlock(tileDict.birchLeaf.tileSprites, x - 1, y + treeHeight + 3);
            placeBlock(tileDict.birchLeaf.tileSprites, x + 1, y + treeHeight + 3);
            placeBlock(tileDict.birchLeaf.tileSprites, x - 2, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprites, x + 2, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprites, x - 3, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprites, x + 3, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprites, x - 2, y + treeHeight + 1);
            placeBlock(tileDict.birchLeaf.tileSprites, x + 2, y + treeHeight + 1);
            placeBlock(tileDict.birchLeaf.tileSprites, x, y + treeHeight + 4);
            placeBlock(tileDict.birchLeaf.tileSprites, x, y + treeHeight + 5);
            placeBlock(tileDict.birchLeaf.tileSprites, x + 1, y + treeHeight + 2);
            placeBlock(tileDict.birchLeaf.tileSprites, x - 1, y + treeHeight + 2);
        }

    }

   

    public void breakBlock(int x, int y)
    {
        //Check if block exists in world before breaking it
        if (worldTiles.Contains(new Vector2Int(x, y)) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize && !IsOnMapEdge(x, y))
        {
            //Search array of world tile objects to find block at x and y axis being clicked on. 
            Destroy(worldTileObjects[worldTiles.IndexOf(new Vector2(x, y))]);
            //Remove tile from world tiles array. Without this, blocks can not be placed in spots where previously placed blocks have been destroyed.   
            worldTileObjects.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
            worldTiles.RemoveAt(worldTiles.IndexOf(new Vector2(x, y)));
        }
        // Function to check if a block is on the edge of the map
        bool IsOnMapEdge(int x, int y)
        {
            return x == 0 || x == worldSize - 1 || y == 0 || y == worldSize - 1;
        }
    }


    public void placeBlock(Sprite[] tileSprites, int x, int y) 
    {
        //Check to see if tile is already placed in location
        //This prevents player from placing blocks on existing blocks
        if (!worldTiles.Contains(new Vector2Int(x, y)) && x>= 0 && x<= worldSize && y >= 0 && y<= worldSize)
        {
            //Check to see if a new tile should generate
            if (x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
            {
                //Create gameobject to hold new tile 
                GameObject newTile = new GameObject();
                //Add ground tag to each placed tile 
                //Add 2d box collider to each placed tile
                //These 2 allow for playerBody to correctly detect when its on the ground


                //Round to nearest multiple of Chunk Size. Prevents uneven chunk sizes 
                float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
                //Find out location of block in chunk 
                chunkCoord /= chunkSize;
                newTile.transform.parent = worldChunks[(int)chunkCoord].transform;
                //Automatically create and name gameTiles based on their makeup. 
                newTile.AddComponent<SpriteRenderer>();

                int spriteIndex = Random.Range(0, tileSprites.Length);
                newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;

                newTile.name = tileSprites[0].name;
                
                //Ensure tallgrass and trees dont have colliders, this allows player to walk through them
                if (newTile.name != "grass1" && newTile.name != "trunk_side" && newTile.name != "trunk_white_side" && newTile.name 
                    != "leaves_transparent" && newTile.name != "leaves" && newTile.name != "grass_brown" && newTile.name != "grass_tan")
                {
                    newTile.tag = "Ground";
                    BoxCollider2D boxCollider = new BoxCollider2D();
                    newTile.AddComponent<BoxCollider2D>();
                }
                newTile.transform.position = new Vector2(x, y);
                //Add placed tile to worldTile List. This helps keep track of where blacks are in the world
                worldTiles.Add(newTile.transform.position);
                worldTileObjects.Add(newTile);

            }
        }
    }  
}
