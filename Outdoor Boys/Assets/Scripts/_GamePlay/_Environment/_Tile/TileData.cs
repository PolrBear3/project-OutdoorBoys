using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { softGround, harshGround }

[System.Serializable]
public class TileData
{
    private TileScrObj _tileScrObj;
    public TileScrObj tileScrObj => _tileScrObj;


    // Constructors
    public TileData(TileScrObj setTile)
    {
        _tileScrObj = setTile;
    }
}