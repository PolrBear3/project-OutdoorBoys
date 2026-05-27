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
    [SerializeField] private GameObject _slotsPageControlButtons;

    [Space(20)]
    [SerializeField] private ItemsSource_Manager _previewSourceManager;
    [SerializeField] private ItemsSource_Manager _itemsSourceManager;

    [Space(20)]
    [SerializeField] private ItemSlot_Manager _ingredientSlotsManager;

    [Space(10)]
    [SerializeField] private PanelToggle_AnimationController _itemInfoToggleController;
    [SerializeField] private ItemSlot _itemImageSlot;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemDescriptionText;


    private int _currentSlotsPage;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        _slotManager.OnSlotHover -= Toggle_ItemInfoPanel;
        _slotManager.OnSlotSelect -= Craft_Item;

        _slotManager.OnSlotSelect -= Refresh_Page;
        _slotManager.OnSlotSelect -= Toggle_ItemInfoPanel;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.OnItemAdded -= Refresh_Page;
        inventory.slotManager.OnSlotSelect -= Refresh_Page;
        inventory.OnItemAdded -= Toggle_ItemInfoPanel;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnItemReturn -= Refresh_Page;
        itemCursor.OnItemReturn -= Toggle_ItemInfoPanel;

        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileSelect -= Refresh_Page;
        tilesController.OnTileSelect -= Toggle_ItemInfoPanel;

        TileTracker playerTileTracker = manager.player.movement.tileTracker;

        playerTileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Refresh_Page);
        playerTileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Toggle_ItemInfoPanel);

        Time_Manager time = manager.time;

        time.UnRegister(ActionUpdateBus.StartUpdate, Refresh_Page);
        time.UnRegister(ActionUpdateBus.StartUpdate, Toggle_ItemInfoPanel);

        EventBus_Manager.UnRegister(EventBus.SubLoad, Refresh_Page);
        EventBus_Manager.UnRegister(EventBus.SubLoad, Toggle_ItemInfoPanel);
    }


    // Component
    private void Set_Data()
    {
        _slotManager.OnSlotHover += Toggle_ItemInfoPanel;
        _slotManager.OnSlotSelect += Craft_Item;

        _slotManager.OnSlotSelect += Refresh_Page;
        _slotManager.OnSlotSelect += Toggle_ItemInfoPanel;

        InGame_Manager manager = InGame_Manager.instance;
        Inventory_Manager inventory = manager.inventory;

        inventory.OnItemAdded += Refresh_Page;
        inventory.slotManager.OnSlotSelect += Refresh_Page;
        inventory.OnItemAdded += Toggle_ItemInfoPanel;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnItemReturn += Refresh_Page;
        itemCursor.OnItemReturn += Toggle_ItemInfoPanel;

        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileSelect += Refresh_Page;
        tilesController.OnTileSelect += Toggle_ItemInfoPanel;

        TileTracker playerTileTracker = manager.player.movement.tileTracker;

        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Refresh_Page);
        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Toggle_ItemInfoPanel);

        Time_Manager time = manager.time;

        time.Register(ActionUpdateBus.StartUpdate, Refresh_Page);
        time.Register(ActionUpdateBus.StartUpdate, Toggle_ItemInfoPanel);

        EventBus_Manager.Register(EventBus.SubLoad, Refresh_Page);
        EventBus_Manager.Register(EventBus.SubLoad, Toggle_ItemInfoPanel);
    }


    // Datas
    private List<ItemData> CraftPreview_ItemDatas()
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
        return previewDatas;
    }

    private List<ItemData> Ingredient_ItemDatas()
    {
        List<IItemsSource> itemsSources = new(_itemsSourceManager.itemsSources);

        Inventory_Manager inventory = InGame_Manager.instance.inventory;
        bool includeInventory = inventory.Toggled();

        if (includeInventory) return _itemsSourceManager.ItemDatas();
        if (inventory is IItemsSource inventorySource) itemsSources.Remove(inventorySource);

        return _itemsSourceManager.ItemDatas(itemsSources);
    }
    private List<ItemData> CraftAvailable_ItemDatas()
    {
        Item_ScrObj[] allItems = Data_Manager.instance.allItems;
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();

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
        return craftAvailableItemDatas;
    }

    private List<ItemData> Refreshed_ItemDatas()
    {
        List<ItemData> craftableDatas = CraftAvailable_ItemDatas();
        List<ItemData> previewDatas = CraftPreview_ItemDatas();

        for (int i = previewDatas.Count - 1; i >= 0; i--)
        {
            Item_ScrObj previewItem = previewDatas[i].itemScrObj;

            for (int j = 0; j < craftableDatas.Count; j++)
            {
                if (craftableDatas[j].itemScrObj != previewItem) continue;

                previewDatas.RemoveAt(i);
                break;
            }
        }

        List<ItemData> refreshedDatas = new();

        refreshedDatas.AddRange(craftableDatas);
        refreshedDatas.AddRange(previewDatas);

        return refreshedDatas;
    }


    // Page Update
    private void Refresh_Page()
    {
        List<ItemData> craftableDatas = CraftAvailable_ItemDatas();
        List<ItemData> refreshDatas = Refreshed_ItemDatas();

        int maxSlotsCount = _slotManager.slots.Count;
        _slotsPageControlButtons.SetActive(refreshDatas.Count > maxSlotsCount);

        int maxPage = refreshDatas.Count <= 0 ? 0 : (refreshDatas.Count - 1) / maxSlotsCount;
        int startIndex = Mathf.Clamp(_currentSlotsPage, 0, maxPage) * maxSlotsCount;

        for (int i = 0; i < maxSlotsCount; i++)
        {
            ItemSlot slot = _slotManager.slots[i];
            int dataIndex = startIndex + i;

            if (dataIndex >= refreshDatas.Count)
            {
                slot.Clear_Data();
                slot.Update_ItemImage();
                slot.Toggle_Transparency(false);
                
                continue;
            }
            slot.Set_Data(refreshDatas[dataIndex]);
            slot.Update_ItemImage();
            slot.Toggle_Transparency(dataIndex >= craftableDatas.Count);
        }
    }
    private void Refresh_Page(ItemSlot _)
    {
        Refresh_Page();
    }
    private void Refresh_Page(Tile _)
    {
        Refresh_Page();
    }

    public void Update_Page(bool nextPage)
    {
        List<ItemData> refreshDatas = Refreshed_ItemDatas();

        int maxSlotsCount = _slotManager.slots.Count;
        int maxPage = refreshDatas.Count <= 0 ? 0 : (refreshDatas.Count - 1) / maxSlotsCount;

        _currentSlotsPage = (_currentSlotsPage + (nextPage ? 1 : -1) + maxPage + 1) % (maxPage + 1);
        Refresh_Page();
    }


    // Craft
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

        _itemInfoToggleController.Toggle(toggle);
        _ingredientSlotsManager.Remove_AllSlots();

        if (toggle == false) return;

        Item_ScrObj hoveringItem = hoveringItemSlot.data?.itemScrObj;
        if (hoveringItem == null) return;

        _itemImageSlot.Set_Data(new ItemData(hoveringItem, 1));
        _itemImageSlot.Update_ItemImage();
        _itemImageSlot.Update_AmountText();

        _itemNameText.text = hoveringItem.itemName;
        _itemDescriptionText.text = hoveringItem.description;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_itemInfoToggleController.togglePanel);

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