using System.Collections;
using UnityEngine;

[System.Serializable]
public class BiomeClass 
{
    public string biomeName;

    public Color biomeCol;

    public TileDict tileDict;

    [Header("Noise Settings")]
    public float terrainFrequency = 0.05f;
    public float caveFrequency = 0.05f;
    public Texture2D caveNoiseTexture;

    [Header("WorldGen Settings")]
    public bool generateCaves = true;
    public int dirtLayerHeight = 7;
    public float worldHeightMultiplier = 5f;
    public float terrainSculptInfluence = 0.25f;

    [Header("Tree Gen")]
    public int genTreeChance = 10;
    public int oakTreeMaxHeight = 30;
    public int oakTreeMinHeight = 4;
    public int birchTreeChance = 5;
    public int birchTreeMaxHeight = 10;
    public int birchTreeMinHeight = 5;

    [Header("Addon Gen")]
    public int tallGrassChance = 10;

    [Header("Ore Gen")]
    public OreClass[] ores;


}
