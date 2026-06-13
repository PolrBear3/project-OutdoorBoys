using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum TileType { softGround, harshGround }
public enum TileState { frozen, wet, warm, hot }

[System.Serializable]
public class TileState_Data
{
    [SerializeField] private TileState _tileState;
    public TileState tileState => _tileState;

    [SerializeField] private int _updateCount;
    public int updateCount => _updateCount;

    public TileState_Data(TileState state, int setCount)
    {
        _tileState = state;
        _updateCount = setCount;
    }

    public void Update_Count(int updateCount)
    {
        _updateCount = updateCount;
    }
}

[System.Serializable]
public class TileState_VisualData
{
    [SerializeField] private TileState _tileState;
    public TileState tileState => _tileState;

    [SerializeField] private Sprite _indicationSprite;
    public Sprite indicationSprite => _indicationSprite;
}

[System.Serializable]
public class TileData
{
    private TileScrObj _tileScrObj;
    public TileScrObj tileScrObj => _tileScrObj;

    /// <summary>
    /// State + Time Count
    /// </summary>
    private List<TileState_Data> _stateDatas = new();
    public List<TileState_Data> stateDatas => _stateDatas;

    private List<ItemData> _placedItemDatas = new();
    public List<ItemData> placedItemDatas => _placedItemDatas;

    private List<ItemData> _preservedItemDatas = new();
    public List<ItemData> preservedItemDatas => _preservedItemDatas;


    // Constructors
    public TileData(TileScrObj setTile)
    {
        _tileScrObj = setTile;
    }


    // State Data
    public void Add_StateData(TileState_Data addData)
    {
        TileState addState = addData.tileState;

        for (int i = 0; i < _stateDatas.Count; i++)
        {
            TileState_Data data = _stateDatas[i];

            if (addState != data.tileState) continue;
            data.Update_Count(addData.updateCount);

            return;
        }
        _stateDatas.Add(new(addState, addData.updateCount));
    }
    public void Remove_StateData(TileState removeState)
    {
        for (int i = 0; i < _stateDatas.Count; i++)
        {
            if (removeState != _stateDatas[i].tileState) continue;

            _stateDatas.RemoveAt(i);
            return;
        }
    }

    private void Remove_EmptyTimeStates()
    {
        for (int i = _stateDatas.Count - 1; i >= 0 ; i--)
        {
            TileState_Data data = _stateDatas[i];

            if (_tileScrObj.Is_StaticState(data.tileState)) continue;
            if (data.updateCount > 0) continue;

            _stateDatas.RemoveAt(i);
        }
    }
    public void Decrease_StateDatas()
    {
        for (int i = _stateDatas.Count - 1; i >= 0; i--)
        {
            TileState_Data data = _stateDatas[i];

            if (_tileScrObj.Is_StaticState(data.tileState)) continue;
            data.Update_Count(data.updateCount - 1);
        }
        Remove_EmptyTimeStates();
    }


    // Item
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