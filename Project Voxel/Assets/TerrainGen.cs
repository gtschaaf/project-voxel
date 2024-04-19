using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    //Commented out because implemented per biome 
    /*[Header("Tree Gen")]
    public int genTreeChance = 10;
    public int oakTreeMaxHeight = 30;
    public int oakTreeMinHeight = 4;
    public int birchTreeChance = 5;
    public int birchTreeMaxHeight = 10;
    public int birchTreeMinHeight = 5;*/

    //Commented out because implemented per biome 
    /*[Header("Addon Gen")]
    public int tallGrassChance = 10;*/

    [Header("Cave Gen")]
    //TerrainSculptInfluence is Surface value in video
    public float terrainSculptInfluence = 0.25f;
    public bool generateCaves = true;
    //public float caveFrequency = 0.05f;

    [Header("Overworld Gen")]
    public int chunkSize = 16;
    public int worldSize = 200;
    //Commented out because implemented per biome 
    //public int dirtLayerHeight = 7;
    public float terrainFrequency = 0.05f;
    public float worldHeightMultiplier = 5f;
    public int heightAddition = 25;

    //Biomes implemented same as caves using Perlin Noise. With each 'cave' being a biome
    [Header("Biomes")]
    public float biomeRarity;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Seed Gen")] 
    public Texture2D caveNoiseTexture;

    [Header("Ore Gen")]
    public OreClass[] ores;

    //Create Array to store each chunk in the world
    private GameObject[] worldChunks; 
    public List<Vector2> worldTiles = new List<Vector2>();
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
        DrawTextures();
        spawnChunks();
        GenerateWorld();
        cam.Spawn(new Vector3(player.spawnPoint.x, player.spawnPoint.y, cam.transform.position.z));
        cam.worldSize = worldSize;
        player.Spawn();
    }


    //This function draws the perlin map and displays it in Unity editor
    public void DrawTextures() 
    {
        biomeMap = new Texture2D(worldSize, worldSize);
        drawBiomeTexture();
        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {

                biomes[i].ores[o].spreadMap = new Texture2D(worldSize, worldSize);
            }
            GenerateNoiseTexture(biomes[i].caveFrequency, biomes[i].terrainFrequency, biomes[i].caveNoiseTexture); 
            //Generate ore perlin noise maps
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
            curBiome = GetCurrentBiome(x, 0);
            float height = Mathf.PerlinNoise((x + seed) * curBiome.terrainFrequency, seed * curBiome.terrainFrequency) * curBiome.worldHeightMultiplier + heightAddition;

            if (x == worldSize / 2) 
            {
                player.spawnPoint = new Vector2(x, height + 1);
            }
            //For loop representing y axis (height) of world

            for (int y = 0; y < height; y++)
            {
                curBiome = GetCurrentBiome(x, y);

                //Start generating stone after dirtLayerHeigh is passed. 
                if (y < height - curBiome.dirtLayerHeight)
                {

                    tileSprites = curBiome.tileDict.stone.tileSprites;
                    

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

                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        placeBlock(tileSprites, x, y);
                    }
                }
                else 
                {
                    placeBlock(tileSprites, x, y);
                }
                //Roll  to see if tree generates 
                //Check to make sure tree doesnt spawn floating
                if (y >= height-1)
                {
                    int t = Random.Range(0, curBiome.genTreeChance);
                    if (t == 1)
                    {
                        //Generate tree
                        if (worldTiles.Contains(new Vector2(x + 0.05f, y + 0.05f)))
                        {
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
                    else { 
                        int i = Random.Range(0, curBiome.tallGrassChance);
                        if (i == 1) {
                            //Generate tall grass and mushrooms and stones
                            if (worldTiles.Contains(new Vector2(x + 0.05f, y + 0.05f)))
                            {
                                if (curBiome.tileDict.tallGrass != null)
                                {
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
        for (int x = 0; x < noiseTexture.width; x++) 
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x+seed) * frequency, (y+seed) * frequency);

                //Insert color value into array based on randomly generated noise texture 
                //This color value is used to generate cave and ore spreads
                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }
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
                if (newTile.name != "grass1" && newTile.name != "trunk_side" && newTile.name != "trunk_white_side" && newTile.name != "leaves_transparent" && newTile.name != "leaves" && newTile.name != "grass_brown" && newTile.name != "grass_tan")
                {
                    newTile.tag = "Ground";
                    BoxCollider2D boxCollider = new BoxCollider2D();
                    newTile.AddComponent<BoxCollider2D>();
                }
                newTile.transform.position = new Vector2(x + 0.05f, y + 0.05f);
                //Add placed tile to worldTile List. This helps keep track of where blacks are in the world
                worldTiles.Add(newTile.transform.position);
            }
        }
    }

        
    
}
