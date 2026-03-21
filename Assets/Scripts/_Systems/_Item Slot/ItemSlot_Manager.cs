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


    public Action<ItemSlot> OnSlotHover;

    public Action<ItemSlot> OnTargetSlotSelect;
    public Action<ItemSlot> OnTargetSlotHoldSelect;
    public Action<ItemSlot> OnRightSelect;

    public Action OnSlotSelect;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Datas);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Datas);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick -= Select_HoveringSlot;
        input.OnHoldLeftClick -= HoldSelect_HoveringSlot;
        input.OnRightClick -= RightSelect_HoveringSlot;
    }


    // Data
    private void Set_Datas()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            ItemSlot slot = _slots[i];

            slot.Set_Data(this);
            slot.Update_Visuals();
        }

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += Select_HoveringSlot;
        input.OnHoldLeftClick += HoldSelect_HoveringSlot;
        input.OnRightClick += RightSelect_HoveringSlot;
    }

    public void Refresh_Datas()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            ItemSlot slot = _slots[i];
            slot.Set_Data(slot.data);
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

    public List<ItemData> Slot_ItemDatas()
    {
        List<ItemData> itemDatas = new();

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemData slotData = _slots[i].data;
            if (slotData == null) continue;

            itemDatas.Add(slotData);
        }
        return itemDatas;
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

    private void Select_HoveringSlot()
    {
        if (_hoveringSlot == null) return;

        OnTargetSlotSelect?.Invoke(_hoveringSlot);
        OnSlotSelect?.Invoke();
    }
    private void HoldSelect_HoveringSlot()
    {
        if (_hoveringSlot == null) return;

        OnTargetSlotHoldSelect?.Invoke(_hoveringSlot);
        OnSlotSelect?.Invoke();
    }

    private void RightSelect_HoveringSlot()
    {
        if (_hoveringSlot == null) return;

        OnRightSelect?.Invoke(_hoveringSlot);
        OnSlotSelect?.Invoke();
    }


    public void Update_Visuals()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            slots[i].Update_Visuals();
        }
    }
}
