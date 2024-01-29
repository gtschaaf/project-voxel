using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TerrainGen : MonoBehaviour
{
    [Header("Tile Sprites")]
    public Sprite grass;
    public Sprite stone;
    public Sprite dirt;
    public Sprite oakLog;
    public Sprite oakLeaf;
    public Sprite birchLog;
    public Sprite birchLeaf;
    public Sprite grassTop1;
    public Sprite grassTop2;
    public Sprite grassTop3;
    public Sprite grassTop4;


    [Header("Tree Gen")]
    public int genTreeChance = 10;
    public int oakTreeMaxHeight = 30;
    public int oakTreeMinHeight = 4;
    public int birchTreeChance = 5;
    public int birchTreeMaxHeight = 10;
    public int birchTreeMinHeight = 5; 

    [Header("Cave Gen")]
    public float terrainSculptInfluence = 0.25f;
    public bool generateCaves = true;
    public float caveFrequency = 0.05f;

    [Header("Overworld Gen")] 
    public int dirtLayerHeight = 7;
    public int worldSize = 500;
    public float terrainFrequency = 0.05f;
    public float worldHeightMultiplier = 5f;
    public int heightAddition = 25;
    public int tallGrassChance = 10;
    public int tallGrassClusterChance = 4;

    [Header("Seed Gen")] 
    public float seed;
    public Texture2D noiseTexture;

    public List<Vector2> worldTiles = new List<Vector2>();

    private void Start()
    {
        //Generate random world seed. This is used to generate random world terrain. 
        seed = Random.Range(-10000, 10000);
        //Generate Perlin Noise which is utilized to form cave and terrain structures.
        GenerateNoiseTexture();
        GenerateWorld();
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
                    tileSprite = stone;

                }
                //Spawn dirt layer
                else if (y < height - 1)
                {
                    tileSprite = dirt;
                }
                else 
                {
                    //Spawn grass on top block of world
                    tileSprite = grass;
                   
        
                }

                //Check to see if caves should generate based on boolean val
                if (generateCaves)
                {

                    if (noiseTexture.GetPixel(x, y).r > terrainSculptInfluence)
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


    public void GenerateNoiseTexture() 
    {
        noiseTexture = new Texture2D(worldSize, worldSize);
        for (int x = 0; x < noiseTexture.width; x++) 
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x+seed) * caveFrequency, (y+seed) * caveFrequency);
                //Insert color value into array based on randomly generated noise texture 
                //This color value is used to generate cave structures
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }
        noiseTexture.Apply();
    }

    void generateOakTree(float x, float y)
    {
        //Generate random number of Oak logs to build tree
        int treeHeight = Random.Range(oakTreeMinHeight, oakTreeMaxHeight);
        for (int i = 0; i < treeHeight; i++) 
        {
            placeBlock(oakLog, x, y+i);
            
        }
        //Generate tree leaves based on height of tree
        placeBlock(oakLeaf, x, y + treeHeight);
        placeBlock(oakLeaf, x + 1, y + treeHeight);
        placeBlock(oakLeaf, x - 1, y + treeHeight);
        placeBlock(oakLeaf, x, y + treeHeight + 1);
        placeBlock(oakLeaf, x + 1, y + treeHeight + 1);
        placeBlock(oakLeaf, x - 1, y + treeHeight + 1);
        placeBlock(oakLeaf, x, y + treeHeight + 2);

        //Add more leaves if tree is taller than set value 
        if (treeHeight > 8) {
            placeBlock(oakLeaf, x, y + treeHeight + 3);
            placeBlock(oakLeaf, x-1, y + treeHeight + 3);
            placeBlock(oakLeaf, x+1, y + treeHeight + 3);
            placeBlock(oakLeaf, x - 2, y + treeHeight);
            placeBlock(oakLeaf, x + 2, y + treeHeight);
            placeBlock(oakLeaf, x - 3, y + treeHeight);
            placeBlock(oakLeaf, x + 3, y + treeHeight);
            placeBlock(oakLeaf, x - 2, y + treeHeight+ 1);
            placeBlock(oakLeaf, x + 2, y + treeHeight+ 1);
            placeBlock(oakLeaf, x, y + treeHeight + 4);
            placeBlock(oakLeaf, x, y + treeHeight + 5);
            placeBlock(oakLeaf, x + 1, y + treeHeight +2);
            placeBlock(oakLeaf, x - 1, y + treeHeight +2);
        }

    }

    void generateBirchTree(float x, float y) 
    {
        //Generate random number of Birch logs to build tree
        int treeHeight = Random.Range(birchTreeMinHeight, birchTreeMaxHeight);
        for (int i = 0; i < treeHeight; i++)
        {
            placeBlock(birchLog, x, y + i);
        }
        //Generate tree leaves based on height of tree
        placeBlock(birchLeaf, x, y + treeHeight);
        placeBlock(birchLeaf, x + 1, y + treeHeight);
        placeBlock(birchLeaf, x - 1, y + treeHeight);
        placeBlock(birchLeaf, x, y + treeHeight + 1);
        placeBlock(birchLeaf, x + 1, y + treeHeight + 1);
        placeBlock(birchLeaf, x - 1, y + treeHeight + 1);
        placeBlock(birchLeaf, x, y + treeHeight + 2);

        //Add more leaves if tree is taller than set value 
        if (treeHeight > 8)
        {
            placeBlock(birchLeaf, x, y + treeHeight + 3);
            placeBlock(birchLeaf, x - 1, y + treeHeight + 3);
            placeBlock(birchLeaf, x + 1, y + treeHeight + 3);
            placeBlock(birchLeaf, x - 2, y + treeHeight);
            placeBlock(birchLeaf, x + 2, y + treeHeight);
            placeBlock(birchLeaf, x - 3, y + treeHeight);
            placeBlock(birchLeaf, x + 3, y + treeHeight);
            placeBlock(birchLeaf, x - 2, y + treeHeight + 1);
            placeBlock(birchLeaf, x + 2, y + treeHeight + 1);
            placeBlock(birchLeaf, x, y + treeHeight + 4);
            placeBlock(birchLeaf, x, y + treeHeight + 5);
            placeBlock(birchLeaf, x + 1, y + treeHeight + 2);
            placeBlock(birchLeaf, x - 1, y + treeHeight + 2);
        }

    }

    


    public void placeBlock(Sprite tileSprite, float x, float y) 
    {
        //Check to see if a new tile should generate. A higher terrainSculptInfluence will generate more caves. 
        GameObject newTile = new GameObject();
        newTile.transform.parent = this.transform;
        //Automatically create and name gameTiles based on their makeup. 
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name;
        newTile.transform.position = new Vector2(x + 0.05f, y + 0.05f);

        //Add placed tile to worldTile List. This helps keep track of where blacks are in the world
        worldTiles.Add(newTile.transform.position);
    }
}
