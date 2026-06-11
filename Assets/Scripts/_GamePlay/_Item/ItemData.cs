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
    [SerializeField] private ItemData _updateItemData;


    private bool TargetTile_ItemsPlaced(Tile targetTile)
    {
        if (targetTile == null) return false;
        if (_placedItemDatas.Length <= 0) return true;

        for (int i = 0; i < _placedItemDatas.Length; i++)
        {
            ItemData checkData = _placedItemDatas[i];
            int placedCount = targetTile.Placed_ItemCount(checkData.itemScrObj);

            if (placedCount < Mathf.Max(1, checkData.amount)) return false;
        }
        return true;
    }
    private bool CustomTiles_ItemsPlaced(List<Tile> customTiles)
    {
        if (_customTilesItemDatas.Length <= 0) return true;
        if (customTiles == null) return false;

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

    public bool UpdateTile_Match(Tile targetTile)
    {
        if (targetTile == null) return false;
        if (_tile != null && _tile != targetTile.data.tileScrObj) return false;

        return true;
    }
    public bool AllTiles_ItemsPlaced(Tile targetTile, List<Tile> customCheckTiles)
    {
        if (TargetTile_ItemsPlaced(targetTile) == false) return false;
        if (CustomTiles_ItemsPlaced(customCheckTiles) == false) return false;

        return true;
    }

    public ItemData Update_ItemData()
    {
        if (_updateItemData.itemScrObj == null) return null;

        return new(_updateItemData.itemScrObj, Mathf.Max(1, _updateItemData.amount));
    }
    public ItemData TargetTilePlaced_UpdateItemData(Tile targetTile, List<Tile> customCheckTiles)
    {
        if (UpdateTile_Match(targetTile) == false) return null;
        if (AllTiles_ItemsPlaced(targetTile, customCheckTiles) == false) return null;

        return Update_ItemData();
    }
}