using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Manager : MonoBehaviour
{
    [SerializeField] private Item_ScrObj _inventoryBagpack;

    [Space(20)]
    [SerializeField] private ItemSlot_Manager _slotManager;
    public ItemSlot_Manager slotManager => _slotManager;

    [SerializeField] private Image _togglePanel;


    public Action OnItemAdded;


    [HideInInspector] public Item_ScrObj loadItem;
    [HideInInspector] public int loadItemAmount;


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
        manager.tilesController.OnTileSelect -= Toggle_Update;

        _slotManager.OnTargetSlotSelect -= Transfer_Item;
        _slotManager.OnTargetSlotHoldSelect -= Transfer_AllItems;
    }


    // Data
    public void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.OnMovement += Toggle_Update;
        manager.tilesController.OnTileSelect += Toggle_Update;

        _slotManager.OnTargetSlotSelect += Transfer_Item;
        _slotManager.OnTargetSlotHoldSelect += Transfer_AllItems;
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


    // Item Control
    /// <returns>
    /// Remaining amount as ItemData after load
    /// </returns>
    public ItemData Add_ItemData(ItemData addData)
    {
        if (addData == null) return addData;

        Item_ScrObj dataItem = addData.itemScrObj;
        List<ItemSlot> emptySlots = _slotManager.EmptySlots();

        if (dataItem.itemType != ItemType.place)
        {
            if (emptySlots.Count <= 0) return addData;

            emptySlots[0].Set_Data(addData);

            OnItemAdded?.Invoke();
            return null;
        }

        int maxAmount = dataItem.maxAmount;
        if (maxAmount <= 0) return addData;

        List<ItemSlot> slots = _slotManager.slots;
        int loadCount = addData.amount;

        for (int i = 0; i < slots.Count; i++)
        {
            ItemSlot slot = slots[i];
            if (slot == null) return addData;

            ItemData slotData = slot.data;

            if (slotData?.itemScrObj != dataItem) continue;
            if (slotData.amount >= maxAmount) continue;

            int slotSpace = maxAmount - slotData.amount;
            int loadAmount = Mathf.Min(slotSpace, loadCount);

            slotData.Update_CurrentAmount(slotData.amount + loadAmount);
            loadCount -= loadAmount;

            if (loadCount <= 0)
            {
                OnItemAdded?.Invoke();
                return null;
            }
        }

        if (emptySlots.Count <= 0) return new(dataItem, loadCount);

        for (int i = 0; i < emptySlots.Count; i++)
        {
            int loadAmount = Mathf.Min(maxAmount, loadCount);

            emptySlots[i].Set_Data(new ItemData(dataItem, loadAmount));
            loadCount -= loadAmount;

            if (loadCount <= 0)
            {
                OnItemAdded?.Invoke();
                return null;
            }
        }

        if (loadCount < addData.amount) OnItemAdded?.Invoke();
        return new(dataItem, loadCount);
    }


    private void Swap_Items(ItemSlot targetSlot)
    {
        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        ItemData swapSlotData = targetSlot?.data;

        if (swapSlotData == null && itemCursor.data == null) return;

        targetSlot.Set_Data(itemCursor.data);
        targetSlot.Update_Visuals();

        itemCursor.Set_Data(swapSlotData);
        itemCursor.Update_Visuals();
    }

    private void Transfer_Item(ItemSlot targetSlot)
    {
        ItemData slotItemData = targetSlot.data;
        Item_ScrObj pickupItem = slotItemData?.itemScrObj;

        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        ItemData cursorItemData = itemCursor.data;

        bool nonPlaceItem = slotItemData != null && pickupItem.itemType != ItemType.place;

        if (nonPlaceItem || cursorItemData != null && pickupItem != cursorItemData.itemScrObj) // different items
        {
            Swap_Items(targetSlot);
            return;
        }
        if (cursorItemData != null && cursorItemData.amount >= pickupItem.maxAmount) // amount maxed
        {
            Swap_Items(targetSlot);
            return;
        }
        if (slotItemData == null) return;

        slotItemData.Update_CurrentAmount(slotItemData.amount - 1);

        targetSlot.Set_Data(slotItemData.amount > 0 ? slotItemData : null);
        targetSlot.Update_Visuals();

        int currentCursorAmount = cursorItemData != null ? cursorItemData.amount : 0;

        itemCursor.Update_Data(new(pickupItem, currentCursorAmount + 1));
        itemCursor.Update_Visuals();
    }
    private void Transfer_AllItems(ItemSlot targetSlot)
    {
        ItemData slotItemData = targetSlot.data;
        Item_ScrObj pickupItem = slotItemData?.itemScrObj;

        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;
        ItemData cursorItemData = itemCursor.data;

        bool nonPlaceItem = slotItemData != null && pickupItem.itemType != ItemType.place;

        if (nonPlaceItem || cursorItemData != null && pickupItem != cursorItemData.itemScrObj) // different items
        {
            Swap_Items(targetSlot);
            return;
        }
        if (cursorItemData != null && cursorItemData.amount >= pickupItem.maxAmount) // amount maxed
        {
            Swap_Items(targetSlot);
            return;
        }
        if (slotItemData == null) return;

        int currentCursorAmount = cursorItemData != null ? cursorItemData.amount : 0;
        int transferAmount = Mathf.Min(pickupItem.maxAmount - currentCursorAmount, slotItemData.amount);

        slotItemData.Update_CurrentAmount(slotItemData.amount - transferAmount);

        targetSlot.Set_Data(slotItemData.amount > 0 ? slotItemData : null);
        targetSlot.Update_Visuals();

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

            Inventory_Manager inventory = InGame_Manager.instance.inventory;

            ItemData data = inventory.Add_ItemData(new(loadItem, loadAmount));
            inventory.slotManager.Update_Visuals();

            if (data == null) return;
            Debug.Log("Leftover Amount: " + data.amount);
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif