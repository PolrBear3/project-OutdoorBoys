using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    [SerializeField] private Item_ScrObj _itemScrObj;
    public Item_ScrObj itemScrObj => _itemScrObj;

    [SerializeField][Range(0, 100)] private int _amount;
    public int amount => _amount;


    // Constructor
    public ItemData(Item_ScrObj setItem, int setAmount)
    {
        _itemScrObj = setItem;
        _amount = setAmount;
    }


    // Data
    public void Update_CurrentAmount(int updateAmount)
    {
        _amount = Mathf.Max(0, updateAmount);
    }

    public int Item_Weight()
    {
        int singleWeight = _itemScrObj.itemWeight;

        if (_itemScrObj.itemType == ItemType.use) return singleWeight;
        return singleWeight * _amount;
    }
}

[System.Serializable]
public class HeatUpdate_ItemData
{
    [SerializeField] private Item_ScrObj _preHeatItem;
    public Item_ScrObj preHeatItem => _preHeatItem;

    [SerializeField] private Item_ScrObj _heatedItem;
    public Item_ScrObj heatedItem => _heatedItem;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _updatePointValue;
    public int updatePointValue => _updatePointValue;
}

[System.Serializable]
public class TileUpdate_ItemData
{
    [SerializeField] private TileScrObj _tile;
    [SerializeField] private ItemData[] _placedItemDatas;

    [Space(10)]
    [SerializeField] private Item_ScrObj _updateItem;


    public Item_ScrObj TilePlaced_UpdateItem(Tile targetTile)
    {
        if (targetTile == null) return null;
        if (_tile != null && _tile != targetTile.data.tileScrObj) return null;

        int checkCount = _placedItemDatas.Length;
        if (checkCount <= 0) return _updateItem;

        for (int i = 0; i < _placedItemDatas.Length; i++)
        {
            ItemData checkData = _placedItemDatas[i];
            int placedCount = targetTile.Placed_ItemCount(checkData.itemScrObj);

            if (placedCount < Mathf.Max(1, checkData.amount)) return null;
        }
        return _updateItem;
    }
}