using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
    public int tallGrassChance = 10;
    public int tallGrassClusterChance = 4;

    [Header("Seed Gen")] 
    public float seed;
    public Texture2D caveNoiseTexture;

    [Header("Ore Gen")]
    public float coalRarity;
    public float coalVeinSize;
    public float ironRarity;
    public float ironVeinSize;
    public float goldRarity;
    public float goldVeinSize;
    public float diamondRarity;
    public float diamondVeinSize;
    //Create a perlin noise map for each ore to determine where they will spawn in world. 
    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;

    //Create Array to store each chunk in the world
    private GameObject[] worldChunks; 
    public List<Vector2> worldTiles = new List<Vector2>();

    private void OnValidate()
    {
        //Use condition to gurantee perlin noise maps are only generated one time for each world
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }

        //Generate Perlin Noise which is utilized to form cave and terrain structures.
        GenerateNoiseTexture(caveFrequency, terrainSculptInfluence, caveNoiseTexture);
        //Ore spreads (perlin noise maps)
        GenerateNoiseTexture(coalRarity, coalVeinSize, coalSpread);
        GenerateNoiseTexture(ironRarity, ironVeinSize, ironSpread);
        GenerateNoiseTexture(goldRarity, goldVeinSize, goldSpread);
        GenerateNoiseTexture(diamondRarity, diamondVeinSize, diamondSpread);
    }

    private void Start()
    {
        //Generate random world seed. This is used to generate random world terrain. 
        seed = Random.Range(-10000, 10000);
        //Use condition to gurantee perlin noise maps are only generated one time for each world
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }

     
        //Generate Perlin Noise which is utilized to form cave and terrain structures.
        GenerateNoiseTexture(caveFrequency, terrainSculptInfluence, caveNoiseTexture);
        //Ore spreads (perlin noise maps)
        GenerateNoiseTexture(coalRarity, coalVeinSize, coalSpread);
        GenerateNoiseTexture(ironRarity, ironVeinSize, ironSpread);
        GenerateNoiseTexture(goldRarity, goldVeinSize, goldSpread);
        GenerateNoiseTexture(diamondRarity, diamondVeinSize, diamondSpread);
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
                Sprite tileSprite;
                //Start generating stone after dirtLayerHeigh is passed. 
                if (y < height - dirtLayerHeight)
                {
                    int tileChoice = Random.Range(1, 3);
                    if (coalSpread.GetPixel(x, y).r > 0.5f)
                        if(tileChoice == 1)
                            tileSprite = tileDict.coal.tileSprite;
                        else
                            tileSprite = tileDict.coalAlt.tileSprite;
                    else if (ironSpread.GetPixel(x, y).r > 0.5f)
                        if (tileChoice == 1)
                            tileSprite = tileDict.iron.tileSprite;
                        else
                            tileSprite = tileDict.ironAlt.tileSprite;
                    else if (goldSpread.GetPixel(x, y).r > 0.5f)
                        if (tileChoice == 1)
                            tileSprite = tileDict.gold.tileSprite;
                        else
                            tileSprite = tileDict.goldAlt.tileSprite;
                    else if (diamondSpread.GetPixel(x, y).r > 0.5f)
                        if (tileChoice == 1)
                            tileSprite = tileDict.diamond.tileSprite;
                        else
                            tileSprite = tileDict.diamondAlt.tileSprite;
                    else
                        tileSprite = tileDict.stone.tileSprite;

                }
                //Spawn dirt layer
                else if (y < height - 1)
                {
                    tileSprite = tileDict.dirt.tileSprite;
                }
                else 
                {
                    //Spawn grass on top block of world
                    tileSprite = tileDict.grass.tileSprite;
                   
        
                }

                //Check to see if caves should generate based on boolean val
                if (generateCaves)
                {

                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        placeBlock(tileSprite, x, y);
                    }
                }
                else 
                {
                    placeBlock(tileSprite, x, y);
                }
                //Roll  to see if tree generates 
                //Check to make sure tree doesnt spawn floating
                if (y >= height-1)
                {
                    int t = Random.Range(0, genTreeChance);
                    if (t == 1)
                    {
                        //Generate tree
                        if (worldTiles.Contains(new Vector2(x +0.05f, y+0.05f))) 
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
            placeBlock(tileDict.oakLog.tileSprite, x, y+i);
            
        }
        //Generate tree leaves based on height of tree
        placeBlock(tileDict.oakLeaf.tileSprite, x, y + treeHeight);
        placeBlock(tileDict.oakLeaf.tileSprite, x + 1, y + treeHeight);
        placeBlock(tileDict.oakLeaf.tileSprite, x - 1, y + treeHeight);
        placeBlock(tileDict.oakLeaf.tileSprite, x, y + treeHeight + 1);
        placeBlock(tileDict.oakLeaf.tileSprite, x + 1, y + treeHeight + 1);
        placeBlock(tileDict.oakLeaf.tileSprite, x - 1, y + treeHeight + 1);
        placeBlock(tileDict.oakLeaf.tileSprite, x, y + treeHeight + 2);

        //Add more leaves if tree is taller than set value 
        if (treeHeight > 8) {
            placeBlock(tileDict.oakLeaf.tileSprite, x, y + treeHeight + 3);
            placeBlock(tileDict.oakLeaf.tileSprite, x-1, y + treeHeight + 3);
            placeBlock(tileDict.oakLeaf.tileSprite, x+1, y + treeHeight + 3);
            placeBlock(tileDict.oakLeaf.tileSprite, x - 2, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprite, x + 2, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprite, x - 3, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprite, x + 3, y + treeHeight);
            placeBlock(tileDict.oakLeaf.tileSprite, x - 2, y + treeHeight+ 1);
            placeBlock(tileDict.oakLeaf.tileSprite, x + 2, y + treeHeight+ 1);
            placeBlock(tileDict.oakLeaf.tileSprite, x, y + treeHeight + 4);
            placeBlock(tileDict.oakLeaf.tileSprite, x, y + treeHeight + 5);
            placeBlock(tileDict.oakLeaf.tileSprite, x + 1, y + treeHeight +2);
            placeBlock(tileDict.oakLeaf.tileSprite, x - 1, y + treeHeight +2);
        }

    }

    void generateBirchTree(int x, int y) 
    {
        //Generate random number of Birch logs to build tree
        int treeHeight = Random.Range(birchTreeMinHeight, birchTreeMaxHeight);
        for (int i = 0; i < treeHeight; i++)
        {
            placeBlock(tileDict.birchLog.tileSprite, x, y + i);
        }
        //Generate tree leaves based on height of tree
        placeBlock(tileDict.birchLeaf.tileSprite, x, y + treeHeight);
        placeBlock(tileDict.birchLeaf.tileSprite, x + 1, y + treeHeight);
        placeBlock(tileDict.birchLeaf.tileSprite, x - 1, y + treeHeight);
        placeBlock(tileDict.birchLeaf.tileSprite, x, y + treeHeight + 1);
        placeBlock(tileDict.birchLeaf.tileSprite, x + 1, y + treeHeight + 1);
        placeBlock(tileDict.birchLeaf.tileSprite, x - 1, y + treeHeight + 1);
        placeBlock(tileDict.birchLeaf.tileSprite, x, y + treeHeight + 2);

        //Add more leaves if tree is taller than set value 
        if (treeHeight > 8)
        {
            placeBlock(tileDict.birchLeaf.tileSprite, x, y + treeHeight + 3);
            placeBlock(tileDict.birchLeaf.tileSprite, x - 1, y + treeHeight + 3);
            placeBlock(tileDict.birchLeaf.tileSprite, x + 1, y + treeHeight + 3);
            placeBlock(tileDict.birchLeaf.tileSprite, x - 2, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprite, x + 2, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprite, x - 3, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprite, x + 3, y + treeHeight);
            placeBlock(tileDict.birchLeaf.tileSprite, x - 2, y + treeHeight + 1);
            placeBlock(tileDict.birchLeaf.tileSprite, x + 2, y + treeHeight + 1);
            placeBlock(tileDict.birchLeaf.tileSprite, x, y + treeHeight + 4);
            placeBlock(tileDict.birchLeaf.tileSprite, x, y + treeHeight + 5);
            placeBlock(tileDict.birchLeaf.tileSprite, x + 1, y + treeHeight + 2);
            placeBlock(tileDict.birchLeaf.tileSprite, x - 1, y + treeHeight + 2);
        }

    }

    


    public void placeBlock(Sprite tileSprite, int x, int y) 
    {
        //Check to see if a new tile should generate
        if (x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            //Create gameobject to hold new tile 
            GameObject newTile = new GameObject();
            //Round to nearest multiple of Chunk Size. Prevents uneven chunk sizes 
            float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
            //Find out location of block in chunk 
            chunkCoord /= chunkSize;
            newTile.transform.parent = worldChunks[(int)chunkCoord].transform;
            //Automatically create and name gameTiles based on their makeup. 
            newTile.AddComponent<SpriteRenderer>();
            newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
            newTile.name = tileSprite.name;
            newTile.transform.position = new Vector2(x + 0.05f, y + 0.05f);
            //Add placed tile to worldTile List. This helps keep track of where blacks are in the world
            worldTiles.Add(newTile.transform.position);
        }
    }

        
    
}
