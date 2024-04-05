using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileDict", menuName = "Tile Dictionary")]

public class TileDict : ScriptableObject
{
    [Header("Tile Dictionary")]
    public TileClass grass;
    public TileClass tallGrass;
    public TileClass stone;
    public TileClass dirt;
    public TileClass oakLog;
    public TileClass oakLeaf;
    public TileClass birchLog;
    public TileClass birchLeaf;
    public TileClass coal;
    public TileClass iron;
    public TileClass gold;
    public TileClass diamond;
    
}
