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

    private List<ItemData> _droppedItemDatas = new();
    public List<ItemData> droppedItemDatas => _droppedItemDatas;


    // Constructors
    public TileData(TileScrObj setTile)
    {
        _tileScrObj = setTile;
    }


    // Item Datas
    public int Placed_StackableItems()
    {
        int count = 0;

        for (int i = 0; i < _placedItemDatas.Count; i++)
        {
            if (_placedItemDatas[i].itemScrObj.stackable == false) continue;
            count++;
        }

        return count;
    }

    public bool NonStackableItem_Placed()
    {
        for (int i = 0; i < _placedItemDatas.Count; i++)
        {
            if (_placedItemDatas[i].itemScrObj.stackable) continue;
            return true;
        }

        return false;
    }
}