using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseableItem : MonoBehaviour
{
    private ItemData _data;
    public ItemData data => _data;

    public Action<Tile> OnUse;
    public Action OnUseDestroy;


    // Data
    public void Set_Data(ItemData setData)
    {
        _data = setData;
    }


    // Main
    public void Update_UseAmount(int useDecreaseAmount)
    {
        _data.Update_CurrentAmount(_data.amount - useDecreaseAmount);

        if (_data.amount > 0) return;
        
        OnUseDestroy?.Invoke();
        InGame_Manager.instance.cursor.itemCursor.Set_Data(null);
    }
}