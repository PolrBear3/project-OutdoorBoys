using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot_Manager : MonoBehaviour
{
    [SerializeField] private List<ItemSlot> _slots;
    public List<ItemSlot> slots => _slots;

    private ItemSlot _hoveringSlot;
    public ItemSlot hoveringSlot => _hoveringSlot;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }
    
    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
    }


    // Data
    private void Set_Data()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            ItemSlot slot = _slots[i];

            slot.Set_Data(this);
            slot.Update_Visuals();
        }
    }


    public List<ItemSlot> EmptySlots()
    {
        List<ItemSlot> emptySlots = new();

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemSlot slot = _slots[i];
            if (slot.data != null) continue;
            
            emptySlots.Add(slot);
        }
        return emptySlots;
    }

    public int Total_ItemWeight()
    {
        int count = 0;

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemData slotData = _slots[i].data;
            if (slotData == null) continue;

            count += slotData.Item_Weight();
        }
        return count;
    }


    // Component
    public void Update_HoveringSlot(ItemSlot hoveringSlot)
    {
        _hoveringSlot = hoveringSlot;
    }

    public void Update_Visuals()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            slots[i].Update_Visuals();
        }
    }
}
