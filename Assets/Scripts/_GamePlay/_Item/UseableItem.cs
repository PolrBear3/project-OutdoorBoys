using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseableItem : MonoBehaviour
{
    private ItemData _data;
    public ItemData data => _data;

    public Func<Tile, bool> CanUse;
    public Action<Tile> OnUse;

    public event Action OnUseDestroy;


    // Data
    public void Set_Data(ItemData setData)
    {
        _data = setData;
    }


    // Main
    public void Update_UseAmount(int useDecreaseAmount)
    {
        InGame_Manager manager = InGame_Manager.instance;
        _data.Update_CurrentAmount(_data.amount - useDecreaseAmount);

        Player_Interaction interaction = manager.player.interaction;
        interaction.InteractUpdate_Stamina();

        if (_data.amount > 0) return;
        OnUseDestroy?.Invoke();

        if (manager.cursor.itemCursor.data.amount > 0) return;
        manager.cursor.itemCursor.Set_Data(null);
    }
}