using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Manager : MonoBehaviour
{
    [SerializeField] private Item_ScrObj _inventoryBagpack;

    [Space(20)]
    [SerializeField] private Image _togglePanel;

    [SerializeField] private InventorySlot[] _slots;
    public InventorySlot[] slots => _slots;


    private InventorySlot _hoveringSlot;

    public Action<InventorySlot> OnSlotSelect;
    public Action<InventorySlot> OnSlotHoldSelect;


    [HideInInspector] public Item_ScrObj loadItem;
    [HideInInspector][Range(0, 100)] public int loadItemAmount;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.Register(EventBus.SubLoad, Toggle_Update);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.SubLoad, Toggle_Update);

        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.OnMovement -= Toggle_Update;
        manager.tilesController.OnTileSelectComplete -= Toggle_Update;

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick -= Transfer_Item;
        input.OnHoldLeftClick -= Transfer_AllItems;
    }


    // Data
    public void Set_Data()
    {
        Load_ItemData(new(loadItem, loadItemAmount));
        Load_Slots();

        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.OnMovement += Toggle_Update;
        manager.tilesController.OnTileSelectComplete += Toggle_Update;

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClick += Transfer_Item;
        input.OnHoldLeftClick += Transfer_AllItems;
    }

    private List<InventorySlot> EmptySlots()
    {
        List<InventorySlot> emptySlots = new();

        for (int i = 0; i < _slots.Length; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot == null)
            {
                Debug.Log("Inspector Slot Field Empty at Index : " + i);
                return null;
            }

            if (slot.data == null) emptySlots.Add(slot);
        }
        return emptySlots;
    }


    // Toggle
    private void Toggle_Update()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;

        _togglePanel.gameObject.SetActive(playerTile.Placed_ItemCount(_inventoryBagpack) > 0);
    }

    public bool Toggled()
    {
        return _togglePanel.gameObject.activeSelf;
    }


    // Load
    /// <returns>
    /// Remaining amount as ItemData after load
    /// </returns>
    public ItemData Load_ItemData(ItemData addData)
    {
        if (addData == null) return addData;
        Item_ScrObj dataItem = addData.itemScrObj;

        if (dataItem.itemType != ItemType.place)
        {
            List<InventorySlot> newSlots = EmptySlots();
            if (newSlots.Count <= 0) return addData;

            newSlots[0].Set_Data(addData);
            return null;
        }

        int maxAmount = dataItem.maxAmount;
        if (maxAmount <= 0) return addData;

        int loadCount = addData.amount;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                Debug.Log("Inspector Slot Field Empty at Index : " + i);
                return addData;
            }

            InventorySlot slot = _slots[i];
            ItemData slotData = slot.data;

            if (slotData?.itemScrObj != dataItem) continue;
            if (slotData.amount >= maxAmount) continue;

            int slotSpace = maxAmount - slotData.amount;
            int loadAmount = Mathf.Min(slotSpace, loadCount);

            slotData.Update_CurrentAmount(slotData.amount + loadAmount);
            loadCount -= loadAmount;

            if (loadCount <= 0) return null;
        }

        List<InventorySlot> emptySlots = EmptySlots();
        if (emptySlots.Count <= 0) return new(dataItem, loadCount);

        for (int i = 0; i < emptySlots.Count; i++)
        {
            int loadAmount = Mathf.Min(maxAmount, loadCount);

            emptySlots[i].Set_Data(new(dataItem, loadAmount));
            loadCount -= loadAmount;

            if (loadCount <= 0) return null;
        }
        return new(dataItem, loadCount);
    }

    public void Load_Slots()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot == null)
            {
                Debug.Log("Inspector Slot Field Empty at Index : " + i);
                return;
            }

            slot.Update_Visuals();
        }
    }


    // Slot Select
    public void Track_HoveringSlot(InventorySlot hoveringSlot)
    {
        _hoveringSlot = hoveringSlot;
    }


    // Item Control
    private void Swap_Items()
    {
        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        if (_hoveringSlot.data == null && itemCursor.itemData == null) return;

        ItemData swapSlotData = _hoveringSlot?.data;

        _hoveringSlot.Set_Data(itemCursor.itemData);
        _hoveringSlot.Update_Visuals();

        itemCursor.Set_Data(swapSlotData);
        itemCursor.Update_Visuals();
    }

    private void Transfer_Item()
    {
        InventorySlot hoveringSlot = _hoveringSlot;
        if (hoveringSlot == null) return;

        ItemData slotItemData = hoveringSlot.data;
        Item_ScrObj pickupItem = slotItemData?.itemScrObj;

        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        ItemData cursorItemData = itemCursor.itemData;

        bool nonPlaceItem = slotItemData != null && pickupItem.itemType != ItemType.place;

        if (nonPlaceItem || cursorItemData != null && pickupItem != cursorItemData.itemScrObj) // different items
        {
            Swap_Items();
            return;
        }
        if (cursorItemData != null && cursorItemData.amount >= pickupItem.maxAmount) // amount maxed
        {
            Swap_Items();
            return;
        }
        if (slotItemData == null) return;

        slotItemData.Update_CurrentAmount(slotItemData.amount - 1);

        hoveringSlot.Set_Data(slotItemData.amount > 0 ? slotItemData : null);
        hoveringSlot.Update_Visuals();

        int currentCursorAmount = cursorItemData != null ? cursorItemData.amount : 0;

        itemCursor.Update_Data(new(pickupItem, currentCursorAmount + 1));
        itemCursor.Update_Visuals();
    }
    private void Transfer_AllItems()
    {
        InventorySlot hoveringSlot = _hoveringSlot;
        if (hoveringSlot == null) return;

        ItemData slotItemData = hoveringSlot.data;
        Item_ScrObj pickupItem = slotItemData?.itemScrObj;

        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        ItemData cursorItemData = itemCursor.itemData;

        bool nonPlaceItem = slotItemData != null && pickupItem.itemType != ItemType.place;

        if (nonPlaceItem || cursorItemData != null && pickupItem != cursorItemData.itemScrObj) // different items
        {
            Swap_Items();
            return;
        }
        if (cursorItemData != null && cursorItemData.amount >= pickupItem.maxAmount) // amount maxed
        {
            Swap_Items();
            return;
        }

        int currentCursorAmount = cursorItemData != null ? cursorItemData.amount : 0;
        int transferAmount = Mathf.Min(pickupItem.maxAmount - currentCursorAmount, slotItemData.amount);

        slotItemData.Update_CurrentAmount(slotItemData.amount - transferAmount);

        hoveringSlot.Set_Data(slotItemData.amount > 0 ? slotItemData : null);
        hoveringSlot.Update_Visuals();

        itemCursor.Update_Data(new(pickupItem, currentCursorAmount + transferAmount));
        itemCursor.Update_Visuals();
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(Inventory_Manager))]
public class Inventory_Manager_Editor : Editor
{
    private SerializedProperty loadItemProp;
    private SerializedProperty loadItemAmountProp;

    private void OnEnable()
    {
        loadItemProp = serializedObject.FindProperty("loadItem");
        loadItemAmountProp = serializedObject.FindProperty("loadItemAmount");
    }

    public override void OnInspectorGUI()
    {
        Inventory_Manager manager = (Inventory_Manager)target;

        serializedObject.Update();
        DrawDefaultInspector();

        GUILayout.Space(40);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(loadItemProp, GUIContent.none);
        Item_ScrObj loadItem = (Item_ScrObj)loadItemProp.objectReferenceValue;

        EditorGUILayout.PropertyField(loadItemAmountProp, GUIContent.none);
        int loadAmount = loadItemAmountProp.intValue;

        if (GUILayout.Button("Load Item"))
        {
            if (loadItem == null) return;

            ItemData data = manager.Load_ItemData(new(loadItem, loadAmount));
            manager.Load_Slots();

            if (data == null) return;
            Debug.Log("Leftover Amount: " + data.amount);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif