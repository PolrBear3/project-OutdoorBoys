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

    private List<ItemData> _preservedItemDatas = new();
    public List<ItemData> preservedItemDatas => _preservedItemDatas;


    // Constructors
    public TileData(TileScrObj setTile)
    {
        _tileScrObj = setTile;
    }


    // Data
    public void Preserve_ItemData(ItemData preserveItemData)
    {
        if (preserveItemData == null) return;
        
        Item_ScrObj preserveItem = preserveItemData.itemScrObj;
        
        if (preserveItem.itemType == ItemType.use)
        {
            _preservedItemDatas.Add(preserveItemData);
            return;
        }
        
        int preserveAmount = preserveItemData.amount;

        for (int i = _preservedItemDatas.Count - 1; i >= 0 ; i--)
        {
            ItemData preserveData = _preservedItemDatas[i];

            if (preserveItem != preserveData.itemScrObj) continue;

            int amountSpace = preserveItem.maxAmount - preserveData.amount;
            int addAmount = Mathf.Min(preserveAmount, amountSpace);

            preserveData.Update_CurrentAmount(preserveData.amount + addAmount);
            preserveAmount -= addAmount;

            if (preserveAmount <= 0) return;
        }

        while (preserveAmount > 0)
        {
            int addAmount = Mathf.Min(preserveAmount, preserveItem.maxAmount);

            _preservedItemDatas.Add(new ItemData(preserveItem, addAmount));
            preserveAmount -= addAmount;
        }
    }
}