using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class ItemCursor : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    
    private ItemData _data;
    public ItemData data => _data;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTileSelect -= Place_Item;
        tilesController.OnTileHoldSelect -= Place_AllItem;
        tilesController.OnTileSelect -= Use_Item;
        tilesController.OnTileSelectComplete -= Update_Visuals;

        Input_Controller input = Input_Controller.instance;

        input.OnRightClick -= Return_PickupItem;
        input.OnRightClick -= Update_Visuals;
    }


    // Data
    private void Set_Data()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTileSelect += Place_Item;
        tilesController.OnTileHoldSelect += Place_AllItem;
        tilesController.OnTileSelect += Use_Item;
        tilesController.OnTileSelectComplete += Update_Visuals;

        Input_Controller input = Input_Controller.instance;

        input.OnRightClick += Return_PickupItem;
        input.OnRightClick += Update_Visuals;
    }

    
    public void Set_Data(ItemData setItemData)
    {
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

        Item_ScrObj pickupItem = _data != null ? _data.itemScrObj : placedItems[0].data.itemScrObj;
        int maxAmount = pickupItem.maxAmount;

        for (int i = placedItems.Count - 1; i >= 0 ; i--)
        {
            int currentAmount = _data != null ? _data.amount : 0;
            if (currentAmount >= maxAmount) return;

            PlaceableItem placedItem = placedItems[i];
            if (pickupItem != placedItem.data.itemScrObj) continue;

            ItemData placedItemData = placedItem.data;
            int pickupAmount = Mathf.Min(maxAmount - currentAmount, placedItemData.amount);

            Update_Data(new(pickupItem, currentAmount + pickupAmount));
            placedItemData.Update_CurrentAmount(placedItemData.amount - pickupAmount);

            if (placedItemData.amount > 0) continue;
            selectTile.Remove_PlacedItem(placedItem);
        }
    }

    private void Return_PickupItem()
    {
        if (_data == null) return;

        Inventory_Manager inventory = InGame_Manager.instance.inventory;
        if (inventory.Toggled() == false) return;

        Item_ScrObj returnItem = _data.itemScrObj;

        ItemData leftOverData = inventory.Add_ItemData(_data);
        if (leftOverData != null && returnItem.itemType != ItemType.place) return;

        inventory.slotManager.Update_Visuals();
        Set_Data(leftOverData);
    }


    private void Place_Item(Tile selectTile)
    {
        if (_data == null)
        {
            Pickup_Item(selectTile);
            return;
        }

        if (_data.itemScrObj.itemType != ItemType.place) return; 

        List<PlaceableItem> placedItems = selectTile.placedItems;
        Item_ScrObj currentItem = _data.itemScrObj;

        for (int i = 0; i < placedItems.Count; i++)
        {
            ItemData placedItemData = placedItems[i].data;

            if (currentItem != placedItemData.itemScrObj) continue;
            if (placedItemData.amount >= currentItem.maxAmount) continue;

            placedItemData.Update_CurrentAmount(placedItemData.amount + 1);
            
            Update_Data(new(currentItem, _data.amount - 1));
            if (_data.amount > 0) return;
            Set_Data(null);
            
            return;
        }

        if (selectTile.placedItems.Count >= 2) return;
        if (selectTile.Placed_ItemCount(currentItem) >= currentItem.maxAmount) return;
        
        if (selectTile.Placed_StackableItems() > 1) return;
        if (currentItem.stackable == false && selectTile.NonStackableItem_Placed()) return;

        GameObject spawnedItem = Instantiate(currentItem.itemPrefab, selectTile.placeableItemsPrefabs);
        spawnedItem.transform.localPosition = currentItem.offsetPosition;
        
        PlaceableItem placedItem = spawnedItem.GetComponent<PlaceableItem>();
        
        placedItem.Set_Data(new(currentItem, 1));
        placedItem.Track_CurrentTile(selectTile);
        placedItem.animPlayer.Set_DefaultPosition(currentItem.offsetPosition);

        selectTile.Track_PlacingItem(placedItem);

        Update_Data(new(currentItem, _data.amount - 1));
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

        int placeCount = _data.amount;

        while (placeCount > 0)
        {
            Place_Item(selectTile);

            if (_data == null) return;
            placeCount--;
        }
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
