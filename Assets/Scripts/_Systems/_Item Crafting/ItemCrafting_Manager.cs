using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCrafting_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemSlot_Manager _slotManager;
    [SerializeField] private Image _togglePanel;

    [Space(20)]
    [SerializeField] private ItemSlot_Manager _ingredientSlotsManager;
    [SerializeField] private Image _itemInfoPanel;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemDescriptionText;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        _slotManager.OnSlotHover -= Toggle_ItemInfoPanel;
        _slotManager.OnSlotHover -= Update_HoveringItemInfo;

        _slotManager.OnTargetSlotSelect -= Craft_Item;
        _slotManager.OnSlotSelect -= Update_CraftItems;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.OnItemAdded -= Update_CraftItems;
        inventory.slotManager.OnSlotSelect -= Update_CraftItems;

        manager.tilesController.OnTileSelect -= Update_CraftItems;
        manager.player.movement.OnMovement -= Update_CraftItems;
    }


    // Component
    private void Set_Data()
    {
        _slotManager.OnSlotHover += Toggle_ItemInfoPanel;
        _slotManager.OnSlotHover += Update_HoveringItemInfo;
        
        _slotManager.OnTargetSlotSelect += Craft_Item;
        _slotManager.OnSlotSelect += Update_CraftItems;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.OnItemAdded += Update_CraftItems;
        inventory.slotManager.OnSlotSelect += Update_CraftItems;

        manager.tilesController.OnTileSelect += Update_CraftItems;
        manager.player.movement.OnMovement += Update_CraftItems;

        Toggle_ItemInfoPanel(null);
    }


    // Item Info
    private void Toggle_ItemInfoPanel(ItemSlot hoveringItemSlot)
    {
        _itemInfoPanel.gameObject.SetActive(hoveringItemSlot != null && hoveringItemSlot.data != null);
    }

    private void Update_HoveringItemInfo(ItemSlot hoveringItemSlot)
    {
        if (hoveringItemSlot == null) return;

        Item_ScrObj hoveringItem = hoveringItemSlot.data?.itemScrObj;
        if (hoveringItem == null) return;

        _itemNameText.text = hoveringItem.itemName;
        _itemDescriptionText.text = hoveringItem.description;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemInfoPanel.rectTransform);

        List<ItemData> ingredientDatas = hoveringItem.Item_IngredientDatas();
        List<ItemSlot> slots = _ingredientSlotsManager.slots;

        for (int i = 0; i < slots.Count; i++)
        {
            ItemSlot slot = slots[i];
            
            bool hasIngredient = i < ingredientDatas.Count;
            slot.gameObject.SetActive(hasIngredient);

            if (hasIngredient == false) continue;
            slot.Set_Data(ingredientDatas[i]);
        }
        _ingredientSlotsManager.Update_Visuals();
    }


    // Craft
    private List<ItemData> AllCurrent_ItemDatas()
    {
        InGame_Manager manager = InGame_Manager.instance;

        ItemData carryItemData = manager.cursor.itemCursor.data;

        Inventory_Manager inventory = manager.inventory;
        List<ItemData> inventoryDatas = inventory.Toggled() ? inventory.slotManager.Slot_ItemDatas() : null;

        Tile playerTile = manager.player.movement.currentTile;
        List<ItemData> tilePlacedDatas = playerTile.Placed_ItemDatas();

        List<ItemData> allCurrentDatas = new();

        if (carryItemData != null) allCurrentDatas.Add(carryItemData);
        if (inventoryDatas != null) allCurrentDatas.AddRange(inventoryDatas);
        if (tilePlacedDatas != null) allCurrentDatas.AddRange(tilePlacedDatas);

        return allCurrentDatas;
    }


    public void Update_CraftItems()
    {
        Item_ScrObj[] allItems = Data_Manager.instance.allItems;

        List<ItemData> currentItemDatas = new(AllCurrent_ItemDatas());
        List<ItemData> craftAvailableItemDatas = new();

        for (int i = 0; i < allItems.Length; i++)
        {
            Item_ScrObj craftItem = allItems[i];
            int craftCount = craftItem.Available_CraftCount(currentItemDatas);

            if (craftCount <= 0) continue;
            craftAvailableItemDatas.Add(new(craftItem, craftCount));
        }
        craftAvailableItemDatas.Sort((x, y) => y.amount.CompareTo(x.amount));

        List<ItemSlot> craftSlots = _slotManager.slots;

        for (int i = 0; i < craftSlots.Count; i++)
        {
            ItemSlot slot = craftSlots[i];

            if (i >= craftAvailableItemDatas.Count)
            {
                slot.Clear_Data();
                continue;
            }
            slot.Set_Data(craftAvailableItemDatas[i]);
        }
        _slotManager.Update_Visuals();
    }

    private void Craft_Item(ItemSlot craftItemSlot)
    {
        ItemData slotItemData = craftItemSlot.data;
        if (slotItemData == null) return;

        List<ItemData> currentDatas = AllCurrent_ItemDatas();

        Item_ScrObj craftItem = slotItemData.itemScrObj;
        List<ItemData> craftIngredientDatas = new(craftItem.Item_IngredientDatas());

        for (int i = 0; i < craftIngredientDatas.Count; i++)
        {
            int targetCount = craftIngredientDatas[i].amount;

            for (int j = 0; j < currentDatas.Count; j++)
            {
                if (craftIngredientDatas[i].itemScrObj != currentDatas[j].itemScrObj) continue;

                int consumeAmount = Mathf.Min(currentDatas[j].amount, targetCount);

                currentDatas[j].Update_CurrentAmount(currentDatas[j].amount - consumeAmount);
                targetCount -= consumeAmount;

                if (targetCount <= 0) break;
            }
        }

        InGame_Manager manager = InGame_Manager.instance;
        ItemCursor itemCursor = manager.cursor.itemCursor;

        Inventory_Manager inventory = manager.inventory;
        ItemSlot_Manager inventorySlotManager = inventory.slotManager;

        // itemCursor update
        itemCursor.Set_Data(itemCursor.data);
        itemCursor.Update_Visuals();

        // placed items update
        manager.player.movement.currentTile.RemoveUpdate_PlacedItems();

        // inventory update
        inventorySlotManager.Refresh_Datas();

        // add crafted item
        int craftAmount = craftItem.itemType == ItemType.use ? craftItem.maxAmount : 1;
        Add_CraftedItem(new(craftItem, craftAmount));
    }

    private void Add_CraftedItem(ItemData craftedItemData)
    {
        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        if (inventory.Toggled() && inventory.Add_ItemData(craftedItemData) == null)
        {
            inventory.slotManager.Update_Visuals();
            return;
        }

        ItemCursor itemCursor = manager.cursor.itemCursor;
        ItemData itemCursorData = itemCursor.data;

        if (itemCursorData == null)
        {
            itemCursor.Set_Data(craftedItemData);
            itemCursor.Update_Visuals();
            return;
        }

        Item_ScrObj craftedItem = craftedItemData.itemScrObj;

        if (itemCursorData.itemScrObj != craftedItem) return;
        if (itemCursorData.amount >= craftedItem.maxAmount) return;

        itemCursorData.Update_CurrentAmount(itemCursorData.amount + craftedItemData.amount);
        itemCursor.Update_Visuals();
    }
}