using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


public class TerrainGen : MonoBehaviour
{
    [Header("Tile Dictionary")]
    public TileDict tileDict;


    [Header("Tree Gen")]
    public int genTreeChance = 10;
    public int oakTreeMaxHeight = 30;
    public int oakTreeMinHeight = 4;
    public int birchTreeChance = 5;
    public int birchTreeMaxHeight = 10;
    public int birchTreeMinHeight = 5;

    [Header("Addon Gen")]
    public int tallGrassChance = 10;

    [Header("Cave Gen")]
    //TerrainSculptInfluence is Surface value in video
    public float terrainSculptInfluence = 0.25f;
    public bool generateCaves = true;
    public float caveFrequency = 0.05f;

    [Header("Overworld Gen")]
    public int chunkSize = 16;
    public int worldSize = 200;
    public int dirtLayerHeight = 7;
    public float terrainFrequency = 0.05f;
    public float worldHeightMultiplier = 5f;
    public int heightAddition = 25;
    

    [Header("Seed Gen")] 
    public float seed;
    public Texture2D caveNoiseTexture;

    [Header("Ore Gen")]
    public OreClass[] ores;
    /*public float coalRarity;
    public float coalVeinSize;
    public float ironRarity;
    public float ironVeinSize;
    public float goldRarity;
    public float goldVeinSize;
    public float diamondRarity;
    public float diamondVeinSize;*/
    //Create a perlin noise map for each ore to determine where they will spawn in world. 
    /*public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;*/

    //Create Array to store each chunk in the world
    private GameObject[] worldChunks; 
    public List<Vector2> worldTiles = new List<Vector2>();

    private void OnValidate()
    {
        
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadMap = new Texture2D(worldSize, worldSize);
        ores[1].spreadMap = new Texture2D(worldSize, worldSize);
        ores[2].spreadMap = new Texture2D(worldSize, worldSize);
        ores[3].spreadMap = new Texture2D(worldSize, worldSize);
        

        //Generate Perlin Noise which is utilized to form cave and terrain structures.
        GenerateNoiseTexture(caveFrequency, terrainSculptInfluence, caveNoiseTexture);
        //Ore spreads (perlin noise maps)
        GenerateNoiseTexture(ores[0].rarity, ores[0].veinSize, ores[0].spreadMap);
        GenerateNoiseTexture(ores[1].rarity, ores[1].veinSize, ores[1].spreadMap);
        GenerateNoiseTexture(ores[2].rarity, ores[2].veinSize, ores[2].spreadMap);
        GenerateNoiseTexture(ores[3].rarity, ores[3].veinSize, ores[3].spreadMap);
    }

    private void Start()
    {
        //Generate random world seed. This is used to generate random world terrain. 
        seed = Random.Range(-10000, 10000);
        //Use condition to gurantee perlin noise maps are only generated one time for each world
        
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadMap = new Texture2D(worldSize, worldSize);
        ores[1].spreadMap = new Texture2D(worldSize, worldSize);
        ores[2].spreadMap = new Texture2D(worldSize, worldSize);
        ores[3].spreadMap = new Texture2D(worldSize, worldSize);
        

     
        //Generate Perlin Noise which is utilized to form cave and terrain structures.
        GenerateNoiseTexture(caveFrequency, terrainSculptInfluence, caveNoiseTexture);
        //Ore spreads (perlin noise maps)
        GenerateNoiseTexture(ores[0].rarity, ores[0].veinSize, ores[0].spreadMap);
        GenerateNoiseTexture(ores[1].rarity, ores[1].veinSize, ores[1].spreadMap);
        GenerateNoiseTexture(ores[2].rarity, ores[2].veinSize, ores[2].spreadMap);
        GenerateNoiseTexture(ores[3].rarity, ores[3].veinSize, ores[3].spreadMap);
        spawnChunks();
        GenerateWorld();
    }

    public void spawnChunks() {
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

    public void GenerateWorld()
    {
        //For loop representing x axis (width) of world
        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFrequency, seed * terrainFrequency) * worldHeightMultiplier + heightAddition;
            //For loop representing y axis (height) of world

            for (int y = 0; y < height; y++)
            {
                Sprite[] tileSprites;
                //Start generating stone after dirtLayerHeigh is passed. 
                if (y < height - dirtLayerHeight)
                {
                    tileSprites = tileDict.stone.tileSprites;
                    

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
                    tileSprites = tileDict.dirt.tileSprites;
                }
                else 
                {
                    //Spawn grass on top block of world
                    tileSprites = tileDict.grass.tileSprites;
                   
                   
        
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
                    int t = Random.Range(0, genTreeChance);
                    if (t == 1)
                    {
                        //Generate tree
                        if (worldTiles.Contains(new Vector2(x + 0.05f, y + 0.05f)))
                        {
                            if (Random.Range(0, birchTreeChance) == 1)
                            {
                                generateBirchTree(x, y + 1);
                            }
                            else
                            {
                                generateOakTree(x, y + 1);

                            }
                        }
                    }
                    else { 
                        int i = Random.Range(0, tallGrassChance);
                        if (i == 1) {
                            //Generate tall grass and mushrooms
                            if (worldTiles.Contains(new Vector2(x + 0.05f, y + 0.05f)))
                            {
                                placeBlock(tileDict.tallGrass.tileSprites, x, y + 1);
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

    void generateOakTree(int x, int y)
    {
        //Generate random number of Oak logs to build tree
        int treeHeight = Random.Range(oakTreeMinHeight, oakTreeMaxHeight);
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

    void generateBirchTree(int x, int y) 
    {
        //Generate random number of Birch logs to build tree
        int treeHeight = Random.Range(birchTreeMinHeight, birchTreeMaxHeight);
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
            newTile.name = tileSprites[0].name;
            //Ensure tallgrass objects dont have colliders, this allows player to walk through them
            if (newTile.name != "grass1") {
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
