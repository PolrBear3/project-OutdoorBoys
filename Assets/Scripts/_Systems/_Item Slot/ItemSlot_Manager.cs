using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField][Range(0, 10)] private int _maxSlotAddAmount;

    [Space(10)]
    [SerializeField] private List<ItemSlot> _slots;
    public List<ItemSlot> slots => _slots;


    private ItemSlot _hoveringSlot;
    public ItemSlot hoveringSlot => _hoveringSlot;

    public Action<ItemSlot> OnSlotHover;
    public Action<ItemSlot> OnSlotSelect;
    public Action<ItemSlot> OnSlotHoldSelect;
    public Action<ItemSlot> OnSlotRightSelect;


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
        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += Select_HoveringSlot;
        input.OnHoldLeftClick += HoldSelect_HoveringSlot;
        input.OnRightClick += RightSelect_HoveringSlot;

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemSlot slot = _slots[i];

            slot.Set_Data(this);
            slot.Update_ItemImage();
            slot.Update_AmountIndications();
        }
    }


    public void Refresh_Datas()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            ItemSlot slot = _slots[i];
            slot.Set_Data(slot.data);
        }
    }

    public void Clear_Datas()
    {
        foreach (ItemSlot slot in _slots)
        {
            slot.Clear_Data();
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


    // New Slots
    public ItemSlot Add_NewSlot(ItemData addItemData)
    {
        if (_slots.Count >= _maxSlotAddAmount) return null;

        GameObject addedSlot = Instantiate(_slotPrefab, transform);
        if (addedSlot.TryGetComponent(out ItemSlot newSlot) == false) return null;

        _slots.Add(newSlot);
        newSlot.Set_Data(addItemData);

        return newSlot;
    }

    public void Remove_AllSlots()
    {
        foreach (ItemSlot currentSlot in _slots)
        {
            Destroy(currentSlot.gameObject);
        }
        _slots.Clear();
    }


    // Slot Hover
    public void Update_HoveringSlot(ItemSlot hoveringSlot)
    {
        _hoveringSlot = hoveringSlot;
    }

    private bool SlotSelect_Available(ItemSlot slot)
    {
        if (slot == null) return false;

        // if (InGame_Manager.instance.movements.AllMovements_Complete() == false) return false;

        return true;
    }


    private void Select_HoveringSlot()
    {
        if (SlotSelect_Available(_hoveringSlot) == false) return;
        
        OnSlotSelect?.Invoke(_hoveringSlot);
    }

    private void HoldSelect_HoveringSlot()
    {
        if (SlotSelect_Available(_hoveringSlot) == false) return;
        OnSlotHoldSelect?.Invoke(_hoveringSlot);
    }

    private void RightSelect_HoveringSlot()
    {
        if (SlotSelect_Available(_hoveringSlot) == false) return;
        OnSlotRightSelect?.Invoke(_hoveringSlot);
    }


    // Visuals
    public void Update_Visuals()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            slots[i].Update_ItemImage();
            slots[i].Update_AmountIndications();
        }
    }
}
