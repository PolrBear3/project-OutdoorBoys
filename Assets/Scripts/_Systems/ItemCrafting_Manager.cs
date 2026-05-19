using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCrafting_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemSlot_Manager _slotManager;

    [Space(20)]
    [SerializeField] private ItemsSource_Manager _previewSourceManager;
    [SerializeField] private ItemsSource_Manager _itemsSourceManager;

    [Space(20)]
    [SerializeField] private ItemSlot_Manager _ingredientSlotsManager;

    [Space(10)]
    [SerializeField] private Image _itemInfoPanel;
    [SerializeField] private ItemSlot _itemImageSlot;

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
        _slotManager.OnSlotSelect -= Craft_Item;

        _slotManager.OnSlotSelect -= Update_CraftableItems;
        _slotManager.OnSlotSelect -= Update_CraftItemsPreview;

        _slotManager.OnSlotSelect -= Toggle_ItemInfoPanel;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.slotManager.OnSlotSelect -= Update_CraftableItems;
        inventory.OnItemAdded -= Update_CraftableItems;

        inventory.slotManager.OnSlotSelect -= Update_CraftItemsPreview;
        inventory.OnItemAdded -= Update_CraftItemsPreview;

        inventory.OnItemAdded -= Toggle_ItemInfoPanel;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnItemReturn -= Update_CraftableItems;
        itemCursor.OnItemReturn -= Update_CraftItemsPreview;

        itemCursor.OnItemReturn -= Toggle_ItemInfoPanel;

        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileSelect -= Update_CraftableItems;
        tilesController.OnTileSelect -= Update_CraftItemsPreview;

        tilesController.OnTileSelect -= Toggle_ItemInfoPanel;

        TileTracker playerTileTracker = manager.player.movement.tileTracker;

        playerTileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Update_CraftableItems);
        playerTileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Update_CraftItemsPreview);

        playerTileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Toggle_ItemInfoPanel);

        Time_Manager time = manager.time;

        time.UnRegister(ActionUpdateBus.StartUpdate, Update_CraftableItems);
        time.UnRegister(ActionUpdateBus.StartUpdate, Update_CraftItemsPreview);
        time.UnRegister(ActionUpdateBus.StartUpdate, Toggle_ItemInfoPanel);

        EventBus_Manager.UnRegister(EventBus.SubLoad, Update_CraftableItems);
        EventBus_Manager.UnRegister(EventBus.SubLoad, Update_CraftItemsPreview);
        EventBus_Manager.UnRegister(EventBus.SubLoad, Toggle_ItemInfoPanel);
    }


    // Component
    private void Set_Data()
    {
        _slotManager.OnSlotHover += Toggle_ItemInfoPanel;
        _slotManager.OnSlotSelect += Craft_Item;

        _slotManager.OnSlotSelect += Update_CraftableItems;
        _slotManager.OnSlotSelect += Update_CraftItemsPreview;

        _slotManager.OnSlotSelect += Toggle_ItemInfoPanel;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.OnItemAdded += Update_CraftableItems;
        inventory.OnItemAdded += Update_CraftItemsPreview;

        inventory.slotManager.OnSlotSelect += Update_CraftableItems;
        inventory.slotManager.OnSlotSelect += Update_CraftItemsPreview;

        inventory.OnItemAdded += Toggle_ItemInfoPanel;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnItemReturn += Update_CraftableItems;
        itemCursor.OnItemReturn += Update_CraftItemsPreview;

        itemCursor.OnItemReturn += Toggle_ItemInfoPanel;

        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileSelect += Update_CraftableItems;
        tilesController.OnTileSelect += Update_CraftItemsPreview;

        tilesController.OnTileSelect += Toggle_ItemInfoPanel;

        TileTracker playerTileTracker = manager.player.movement.tileTracker;

        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Update_CraftableItems);
        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Update_CraftItemsPreview);

        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Toggle_ItemInfoPanel);

        Time_Manager time = manager.time;

        time.Register(ActionUpdateBus.StartUpdate, Update_CraftableItems);
        time.Register(ActionUpdateBus.StartUpdate, Update_CraftItemsPreview);
        time.Register(ActionUpdateBus.StartUpdate, Toggle_ItemInfoPanel);

        EventBus_Manager.Register(EventBus.SubLoad, Update_CraftableItems);
        EventBus_Manager.Register(EventBus.SubLoad, Update_CraftItemsPreview);
        EventBus_Manager.Register(EventBus.SubLoad, Toggle_ItemInfoPanel);
    }


    // Craft Preview
    private void Update_CraftItemsPreview()
    {
        Item_ScrObj[] allItems = Data_Manager.instance.allItems;

        List<ItemData> previewSourceDatas = _previewSourceManager.ItemDatas(_previewSourceManager.itemsSources);
        List<ItemData> previewDatas = new();

        for (int i = 0; i < allItems.Length; i++)
        {
            Item_ScrObj craftItem = allItems[i];

            if (craftItem.Available_CraftCount(previewSourceDatas) <= 0) continue;
            previewDatas.Add(new(craftItem, 1));
        }
        previewDatas.Sort((x, y) => y.amount.CompareTo(x.amount));

        List<ItemSlot> currentSlots = _slotManager.slots;
        List<ItemData> craftableDatas = _slotManager.Slot_ItemDatas();

        int previewIndex = 0;

        for (int i = 0; i < currentSlots.Count; i++)
        {
            ItemSlot slot = currentSlots[i];
            if (slot.data != null) continue;

            while (previewIndex < previewDatas.Count)
            {
                Item_ScrObj previewItem = previewDatas[previewIndex].itemScrObj;
                previewIndex++;
                
                bool itemLoaded = false;

                for (int j = 0; j < craftableDatas.Count; j++)
                {
                    if (craftableDatas[j] == null) continue;
                    if (previewItem != craftableDatas[j].itemScrObj) continue;

                    itemLoaded = true;
                    break;

                }
                if (itemLoaded) continue;

                slot.Set_Data(new ItemData(previewItem, 1));
                slot.Update_ItemImage();
                slot.Toggle_Transparency(true);

                break;
            }
            if (previewIndex >= previewDatas.Count) return;
        }
    }
    private void Update_CraftItemsPreview(Tile _)
    {
        Update_CraftItemsPreview();
    }
    private void Update_CraftItemsPreview(ItemSlot _)
    {
        Update_CraftItemsPreview();
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

    private void Update_CraftableItems(Tile playerTile)
    {
        Item_ScrObj[] allItems = Data_Manager.instance.allItems;

        List<ItemData> currentItemDatas = Ingredient_ItemDatas();
        List<ItemData> craftAvailableItemDatas = new();

        for (int i = 0; i < allItems.Length; i++)
        {
            Item_ScrObj craftItem = allItems[i];

            ItemData[] placedCheckDatas = craftItem.craftRequiredPlacedItemDatas;
            bool checkItemsPlaced = true;

            for (int j = 0; j < placedCheckDatas.Length; j++)
            {
                ItemData checkData = placedCheckDatas[j];
                if (playerTile.Placed_ItemCount(checkData.itemScrObj) >= checkData.amount) continue;

                checkItemsPlaced = false;
                break;
            }
            if (checkItemsPlaced == false) continue;

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
            }
            else slot.Set_Data(craftAvailableItemDatas[i]);

            slot.Update_ItemImage();
            slot.Update_AmountText();
            slot.Toggle_Transparency(false);
        }
    }
    private void Update_CraftableItems()
    {
        Update_CraftableItems(InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile());
    }
    private void Update_CraftableItems(ItemSlot _)
    {
        Update_CraftableItems();
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
        if (craftItem.Available_CraftCount(Ingredient_ItemDatas()) <= 0) return;

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


    // Toggles
    private void Toggle_ItemInfoPanel(ItemSlot hoveringItemSlot)
    {
        bool toggle = hoveringItemSlot != null && hoveringItemSlot.data != null;

        _itemInfoPanel.gameObject.SetActive(toggle);
        _ingredientSlotsManager.Remove_AllSlots();

        if (toggle == false) return;

        Item_ScrObj hoveringItem = hoveringItemSlot.data?.itemScrObj;
        if (hoveringItem == null) return;

        _itemImageSlot.Set_Data(new ItemData(hoveringItem, 1));
        _itemImageSlot.Update_ItemImage();
        _itemImageSlot.Update_AmountText();

        _itemNameText.text = hoveringItem.itemName;
        _itemDescriptionText.text = hoveringItem.description;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemInfoPanel.rectTransform);

        List<ItemData> ingredientDatas = hoveringItem.Item_IngredientDatas();
        foreach (ItemData data in ingredientDatas)
        {
            _ingredientSlotsManager.Add_NewSlot(data);
        }

        List<ItemSlot> addedSlots = _ingredientSlotsManager.slots;
        foreach (ItemSlot slot in addedSlots)
        {
            slot.Update_ItemImage();
            slot.Update_AmountText();
        }
    }
    private void Toggle_ItemInfoPanel(Tile _)
    {
        Toggle_ItemInfoPanel(_slotManager.hoveringSlot);
    }
    private void Toggle_ItemInfoPanel()
    {
        Toggle_ItemInfoPanel(_slotManager.hoveringSlot);
    }
}