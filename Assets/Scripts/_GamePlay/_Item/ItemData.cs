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
public class ConvertUpdate_ItemData
{
    [SerializeField] private Item_ScrObj _preUpdateItem;
    public Item_ScrObj preUpdateItem => _preUpdateItem;

    [SerializeField] private ItemData[] _convertUpdateItemDatas;

    public bool Is_ConvertedItem(Item_ScrObj targetItem)
    {
        for (int i = 0; i < _convertUpdateItemDatas.Length; i++)
        {
            if (_convertUpdateItemDatas[i].itemScrObj == targetItem) return true;
        }
        return false;
    }

    public ItemData Converted_ItemData()
    {
        if (_convertUpdateItemDatas.Length <= 0) return null;
        return _convertUpdateItemDatas[Random.Range(0, _convertUpdateItemDatas.Length)];
    }
}

[System.Serializable]
public class TileUpdate_ItemData
{
    [SerializeField] private TileScrObj _tile;
    public TileScrObj tile => _tile;

    [SerializeField] private ItemData[] _placedItemDatas;
    [SerializeField] private ItemData[] _customTilesItemDatas;

    [Space(10)]
    [SerializeField] private Item_ScrObj _updateItem;
    public Item_ScrObj updateItem => _updateItem;


    public bool CustomTiles_ItemsPlaced(List<Tile> customTiles)
    {
        if (_customTilesItemDatas.Length <= 0) return true;

        for (int i = 0; i < _customTilesItemDatas.Length; i++)
        {
            ItemData checkData = _customTilesItemDatas[i];
            int checkCount = Mathf.Max(1, checkData.amount); ;

            for (int j = 0; j < customTiles.Count; j++)
            {
                checkCount -= customTiles[j].Placed_ItemCount(checkData.itemScrObj);
                if (checkCount <= 0) break;
            }
            if (checkCount > 0) return false;
        }
        return true;
    }

    private bool TargetTile_ItemsPlaced(Tile targetTile)
    {
        if (_placedItemDatas.Length <= 0) return true;

        for (int i = 0; i < _placedItemDatas.Length; i++)
        {
            ItemData checkData = _placedItemDatas[i];
            int placedCount = targetTile.Placed_ItemCount(checkData.itemScrObj);

            if (placedCount < Mathf.Max(1, checkData.amount)) return false;
        }
        return true;
    }
    public Item_ScrObj TargetTilePlaced_UpdateItem(Tile targetTile)
    {
        if (targetTile == null) return null;
        if (_tile != null && _tile != targetTile.data.tileScrObj) return null;

        if (TargetTile_ItemsPlaced(targetTile) == false) return null;
        return _updateItem;
    }
}