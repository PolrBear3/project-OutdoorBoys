using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseableItem : MonoBehaviour
{
    private ItemData _data;
    public ItemData data => _data;

    public Action<Tile> OnUse;
    public Action<Tile> OnEmptyUse;


    // Data
    public void Set_Data(ItemData setData)
    {
        _data = setData;
    }


    // Main
    public void Use(Tile useTile)
    {
        if (_data.amount <= 0)
        {
            OnEmptyUse?.Invoke(useTile);
            return;
        }
        OnUse?.Invoke(useTile);
    }
}