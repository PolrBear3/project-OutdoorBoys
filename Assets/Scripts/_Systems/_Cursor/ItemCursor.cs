using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ItemCursor : MonoBehaviour, IItemsSource, IItemsSourceRemove, IItemsSourceAdd
{
    [Space(20)]
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    [Space(20)]
    [SerializeField] private Item_ScrObj _useableItemDrop;


    private ItemData _data;
    public ItemData data => _data;

    public Action OnItemReturn;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTargetTileSelect -= Use_Item;

        tilesController.OnTargetTileSelect -= Place_AllItem;
        tilesController.OnTileRightSelect -= Place_Item;

        tilesController.OnTileSelect -= Update_Visuals;

        Input_Controller input = Input_Controller.instance;

        input.OnRightClick -= Return_CurrentItem;
        input.OnHoldRightClick -= Place_UseItem;

        input.OnRightClick -= Update_Visuals;
        input.OnHoldRightClick -= Update_Visuals;
    }


    // IItemsSource
    public IEnumerable<ItemData> ItemDatas()
    {
        if (_data == null) yield break;
        yield return _data;
    }

    public int RemoveItem(Item_ScrObj updateItem, int removeAmount)
    {
        if (data?.itemScrObj != updateItem) return 0;

        int currentAmount = _data.amount;
        int removeCount = Mathf.Min(currentAmount, removeAmount);

        int setAmount = currentAmount - removeAmount;

        Set_Data(setAmount > 0 ? new(updateItem, setAmount) : null);
        Update_Visuals();

        return removeCount;
    }

    public int AddItem(Item_ScrObj addItem, int addAmount)
    {
        int maxAmount = addItem.maxAmount;
        int targetAddAmount = Mathf.Min(addAmount, maxAmount);

        if (_data == null)
        {
            Set_Data(new(addItem, targetAddAmount));
            Update_Visuals();

            return maxAmount;
        }
        if (addItem != _data.itemScrObj) return 0;

        int currentAmount = _data.amount;
        int updateAmount = currentAmount + Mathf.Min(maxAmount - currentAmount, targetAddAmount);

        Update_Data(new(addItem, updateAmount));
        Update_Visuals();

        return targetAddAmount;
    }


    // Data
    private void Set_Data()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTargetTileSelect += Use_Item;

        tilesController.OnTargetTileSelect += Place_AllItem;
        tilesController.OnTileRightSelect += Place_Item;

        tilesController.OnTileSelect += Update_Visuals;

        Input_Controller input = Input_Controller.instance;

        input.OnRightClick += Return_CurrentItem;
        input.OnHoldRightClick += Place_UseItem;

        input.OnRightClick += Update_Visuals;
        input.OnHoldRightClick += Update_Visuals;
    }


    public void Set_Data(ItemData setItemData)
    {
        if (_data == setItemData) return;
        _data = setItemData != null && setItemData.amount > 0 ? setItemData : null;

        int updateRange = _data != null ? _data.itemScrObj.triggerRange : 0;
        _cursor.Update_TilePointerRange(updateRange);

        Load_UseItem();
    }

    public void Update_Data(ItemData updateItemData)
    {
        if (updateItemData == null)
        {
            Set_Data(null);
            return;
        }

        if (_data == null || _data.itemScrObj != updateItemData.itemScrObj)
        {
            Set_Data(updateItemData);
            return;
        }

        _data.Update_CurrentAmount(updateItemData.amount);
    }

    public void Update_Visuals()
    {
        // sprite
        _cursor.Update_PointerSprite(_data?.itemScrObj.inventorySprite);
        InGame_Manager.instance.player.interaction.Update_IndicationIcon(_data?.itemScrObj.microSprite);

        // amount text
        if (_data == null || _data.amount <= 1)
        {
            _cursor.amountText.gameObject.SetActive(false);
            return;
        }
        _cursor.Update_AmountText(_data.amount);
    }


    // Item Control
    private void Pickup_Item(Tile selectTile)
    {
        List<PlaceableItem> placedItems = selectTile.placedItems;
        if (placedItems.Count <= 0) return;

        PlaceableItem firstPlacedItem = placedItems[0];
        ItemData firstPlacedItemData = firstPlacedItem.data;

        Item_ScrObj pickupItem = _data != null ? _data.itemScrObj : firstPlacedItemData.itemScrObj;

        if (pickupItem.itemType == ItemType.use)
        {
            Set_Data(firstPlacedItemData);
            
            selectTile.Remove_PlacedItemData(firstPlacedItem);
            Destroy(firstPlacedItem.gameObject);

            return;
        }

        int maxAmount = pickupItem.maxAmount;
        int currentAmount = _data != null ? _data.amount : 0;

        for (int i = placedItems.Count - 1; i >= 0; i--)
        {
            PlaceableItem placedItem = placedItems[i];

            if (pickupItem != placedItem.data.itemScrObj) continue;
            if (currentAmount >= maxAmount) return;

            ItemData placedItemData = placedItem.data;
            int pickupAmount = Mathf.Min(maxAmount - currentAmount, placedItemData.amount);

            Update_Data(new(pickupItem, currentAmount + pickupAmount));
            placedItemData.Update_CurrentAmount(placedItemData.amount - pickupAmount);

            if (placedItemData.amount > 0) continue;

            selectTile.Remove_PlacedItemData(placedItem);
            Destroy(placedItem.gameObject);
        }
    }

    private void Return_CurrentItem()
    {
        if (_data == null) return;

        InGame_Manager manager = InGame_Manager.instance;
        if (manager.tilesController.Tile_Selectable(manager.cursor.pointingTile)) return;

        Inventory_Manager inventory = manager.inventory;
        if (inventory.Toggled() == false)
        {
            Place_AllItem(manager.player.movement.currentTile);
            OnItemReturn?.Invoke();

            return;
        }
        if (inventory.slotManager.hoveringSlot != null) return;

        Item_ScrObj returnItem = _data.itemScrObj;
        ItemData leftOverData = inventory.Add_ItemData(_data);

        Set_Data(leftOverData);
        OnItemReturn?.Invoke();

        if (leftOverData != null && returnItem.itemType != ItemType.place) return;
        inventory.slotManager.Update_Visuals();
    }


    private void Place_Item(Tile selectTile)
    {
        if (_data == null)
        {
            Pickup_Item(selectTile);
            return;
        }

        if (selectTile.Set_PlacingItem(new(_data.itemScrObj, 1)) != null) return;
        _data.Update_CurrentAmount(_data.amount - 1);

        if (_data.amount > 0) return;
        Set_Data(null);
    }

    private void Place_AllItem(Tile selectTile)
    {
        if (_data == null)
        {
            Pickup_Item(selectTile);
            return;
        }
        
        Set_Data(selectTile.Set_PlacingItem(_data));
    }

    private void Place_UseItem()
    {
        Item_ScrObj currentUseItem = _data?.itemScrObj;
        if (currentUseItem == null || currentUseItem.itemType != ItemType.use) return;

        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
        if (playerTile.ItemPlaceCount_Available() == false) return;

        GameObject spawnedDrop = Instantiate(_useableItemDrop.itemPrefab, playerTile.placeableItemsPrefabs);
        PlaceableItem placedDrop = spawnedDrop.GetComponent<PlaceableItem>();

        placedDrop.Set_Data(new(currentUseItem, _data.amount));
        placedDrop.Track_CurrentTile(playerTile);
        placedDrop.animPlayer.spriteRenderer.sprite = currentUseItem.microSprite;

        Set_Data(null);

        playerTile.Track_PlacingItem(placedDrop);
    }


    private void Load_UseItem()
    {
        Player_Interaction player = InGame_Manager.instance.player.interaction;
        bool itemLoadReady = _data != null && _data.amount > 0 && _data.itemScrObj.itemType == ItemType.use;

        GameObject loadItem = itemLoadReady ? _data.itemScrObj.itemPrefab : null;
        player.Load_ItemPrefab(loadItem);

        if (loadItem == null || player.currentItemPrefab.TryGetComponent(out UseableItem useItem) == false) return;
        useItem.Set_Data(_data);
    }

    private void Use_Item(Tile selectTile)
    {
        if (_data == null) return;
        Item_ScrObj currentItem = _data.itemScrObj;

        if (currentItem.itemType != ItemType.use) return;

        GameObject currentUseItem = InGame_Manager.instance.player.interaction.currentItemPrefab;
        if (currentUseItem.TryGetComponent(out UseableItem useItem) == false) return;

        useItem.OnUse?.Invoke(selectTile);
    }
}
