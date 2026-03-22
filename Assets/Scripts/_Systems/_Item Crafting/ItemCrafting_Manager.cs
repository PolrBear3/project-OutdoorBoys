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
    [SerializeField] private ItemsSource_Manager _itemsSourceManager;

    [Space(20)]
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

        _slotManager.OnTargetSlotSelect -= Craft_Item;
        _slotManager.OnSlotHover -= Toggle_ItemInfoPanel;

        _slotManager.OnSlotSelect -= Update_CraftableItems;
        _slotManager.OnTargetSlotSelect -= Toggle_ItemInfoPanel;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.slotManager.OnSlotSelect -= Update_CraftableItems;
        inventory.OnItemAdded -= Update_CraftableItems;
        inventory.OnItemAdded -= Toggle_ItemInfoPanel;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnItemReturn -= Update_CraftableItems;
        itemCursor.OnItemReturn -= Toggle_ItemInfoPanel;

        Time_Manager time = manager.time;

        time.UnRegister(TimeUpdateBus.StartUpdate, Update_CraftableItems);
        time.UnRegister(TimeUpdateBus.StartUpdate, Toggle_ItemInfoPanel);
    }


    // Component
    private void Set_Data()
    {
        _slotManager.OnTargetSlotSelect += Craft_Item;
        _slotManager.OnSlotHover += Toggle_ItemInfoPanel;

        _slotManager.OnSlotSelect += Update_CraftableItems;
        _slotManager.OnTargetSlotSelect += Toggle_ItemInfoPanel;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.slotManager.OnSlotSelect += Update_CraftableItems;
        inventory.OnItemAdded += Update_CraftableItems;
        inventory.OnItemAdded += Toggle_ItemInfoPanel;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnItemReturn += Update_CraftableItems;
        itemCursor.OnItemReturn += Toggle_ItemInfoPanel;

        Time_Manager time = manager.time;

        time.Register(TimeUpdateBus.StartUpdate, Update_CraftableItems);
        time.Register(TimeUpdateBus.StartUpdate, Toggle_ItemInfoPanel);

        Toggle_ItemInfoPanel();
    }


    // Craft
    private List<ItemData> Ingredient_ItemDatas()
    {
        List<IItemsSource> itemsSources = new(_itemsSourceManager.itemsSources);

        Inventory_Manager inventory = InGame_Manager.instance.inventory;
        bool includeInventory = inventory.Toggled();

        if (includeInventory) return _itemsSourceManager.ItemDatas();
        if (inventory is IItemsSource inventorySource) itemsSources.Remove(inventorySource);

        return _itemsSourceManager.ItemDatas(itemsSources);
    }

    public void Update_CraftableItems()
    {
        Item_ScrObj[] allItems = Data_Manager.instance.allItems;

        List<ItemData> currentItemDatas = Ingredient_ItemDatas();
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


    private List<IItemsSourceRemove> IngredientRemove_ItemsSource()
    {
        Inventory_Manager inventory = InGame_Manager.instance.inventory;
        bool includeInventory = inventory.Toggled();

        List<IItemsSourceRemove> itemsSources = new(_itemsSourceManager.itemsRemoveSources);
        if (includeInventory) return itemsSources;

        if (inventory is IItemsSourceRemove inventorySource) itemsSources.Remove(inventorySource);
        return itemsSources;
    }
    private List<IItemsSourceAdd> AddItems_Source()
    {
        Inventory_Manager inventory = InGame_Manager.instance.inventory;
        bool includeInventory = inventory.Toggled();

        List<IItemsSourceAdd> itemsSources = new(_itemsSourceManager.itemsAddSources);
        if (includeInventory) return itemsSources;

        if (inventory is IItemsSourceAdd inventorySource) itemsSources.Remove(inventorySource);
        return itemsSources;
    }

    private void Craft_Item(ItemSlot craftItemSlot)
    {
        ItemData slotItemData = craftItemSlot.data;
        if (slotItemData == null) return;

        Item_ScrObj craftItem = slotItemData.itemScrObj;
        int craftAmount = craftItem.itemType == ItemType.use ? craftItem.maxAmount : 1;

        List<ItemData> craftIngredientDatas = new(craftItem.Item_IngredientDatas());

        List<IItemsSourceRemove> craftRemoveSources = IngredientRemove_ItemsSource();
        List<IItemsSourceAdd> craftAddSources = AddItems_Source();

        // use ingredients
        foreach (ItemData ingredientData in craftIngredientDatas)
        {
            _itemsSourceManager.RemoveItem(craftRemoveSources, ingredientData.itemScrObj, ingredientData.amount);
        }

        // check add item space
        if (_itemsSourceManager.AddItem(craftAddSources, craftItem, craftAmount) <= 0) return;

        // return ingredients
        foreach (ItemData ingredientData in craftIngredientDatas)
        {
            _itemsSourceManager.AddItem(craftAddSources, ingredientData.itemScrObj, ingredientData.amount);
        }
    }


    // Item Info
    private void Toggle_ItemInfoPanel(ItemSlot hoveringItemSlot)
    {
        bool toggle = hoveringItemSlot != null && hoveringItemSlot.data != null;
        _itemInfoPanel.gameObject.SetActive(toggle);

        if (toggle == false) return;

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
    private void Toggle_ItemInfoPanel()
    {
        Toggle_ItemInfoPanel(_slotManager.hoveringSlot);
    }
}