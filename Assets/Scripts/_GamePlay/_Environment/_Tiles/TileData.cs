using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { softGround, harshGround }

[System.Serializable]
public class TileData
{
    private TileScrObj _tileScrObj;
    public TileScrObj tileScrObj => _tileScrObj;

    private List<ItemData> _placedItemDatas = new();
    public List<ItemData> placedItemDatas => _placedItemDatas;


    // Constructors
    public TileData(TileScrObj setTile)
    {
        _tileScrObj = setTile;
    }
}