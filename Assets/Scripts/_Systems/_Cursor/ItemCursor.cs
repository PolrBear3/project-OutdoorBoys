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

    
    private ItemData _itemData;
    public ItemData itemData => _itemData;


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
        tilesController.OnTileSelect -= Use_Item;
        tilesController.OnTileHoldSelect -= Place_AllItem;
    }


    // Data
    private void Set_Data()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTileSelect += Place_Item;
        tilesController.OnTileSelect += Use_Item;
        tilesController.OnTileHoldSelect += Place_AllItem;
    }


    public void Set_Item(ItemData setItemData)
    {
        _itemData = setItemData;

        // range
        int updateRange = setItemData != null ? setItemData.itemScrObj.triggerRange : 0;
        _cursor.Update_TilePointerRange(updateRange);

        // sprite
        _cursor.Update_PointerSprite(setItemData?.itemScrObj.inventorySprite);
        InGame_Manager.instance.player.interaction.Update_IndicationIcon(setItemData?.itemScrObj.microSprite);

        // amount text
        if (setItemData == null)
        {
            _cursor.amountText.gameObject.SetActive(false);
            return;
        }
        _cursor.Update_AmountText(setItemData.amount);
    }

    public void Update_Item(ItemData updateItemData)
    {
        if (updateItemData == null)
        {
            Set_Item(null);
            return;
        }

        if (_itemData == null || _itemData.itemScrObj != updateItemData.itemScrObj)
        {
            Set_Item(updateItemData);
            return;
        }

        _itemData.Update_CurrentAmount(updateItemData.amount);
        _cursor.Update_AmountText(_itemData.amount);
    }


    // Item Trigger
    private void Pickup_Item(Tile selectTile)
    {        
        List<PlaceableItem> placedItems = selectTile.placedItems;
        if (placedItems.Count <= 0) return;

        Item_ScrObj pickupItem = _itemData != null ? _itemData.itemScrObj : placedItems[0].data.itemScrObj;
        int maxAmount = pickupItem.maxAmount;

        for (int i = placedItems.Count - 1; i >= 0 ; i--)
        {
            int currentAmount = _itemData != null ? _itemData.amount : 0;
            if (currentAmount >= maxAmount) return;

            PlaceableItem placedItem = placedItems[i];
            if (pickupItem != placedItem.data.itemScrObj) continue;

            ItemData placedItemData = placedItem.data;
            int availableAmount = Mathf.Min(maxAmount - currentAmount, placedItemData.amount);

            Update_Item(new(pickupItem, currentAmount + availableAmount));
            placedItemData.Update_CurrentAmount(placedItemData.amount - availableAmount);

            if (placedItemData.amount > 0) continue;
            selectTile.Remove_PlacedItem(placedItem);
        }
    }


    private void Place_Item(Tile selectTile)
    {
        if (_itemData == null)
        {
            Pickup_Item(selectTile);
            return;
        }

        List<PlaceableItem> placedItems = selectTile.placedItems;
        Item_ScrObj currentItem = _itemData.itemScrObj;

        for (int i = 0; i < placedItems.Count; i++)
        {
            ItemData placedItemData = placedItems[i].data;

            if (currentItem != placedItemData.itemScrObj) continue;
            if (placedItemData.amount >= currentItem.maxAmount) continue;

            placedItemData.Update_CurrentAmount(placedItemData.amount + 1);
            
            Update_Item(new(currentItem, _itemData.amount - 1));
            if (_itemData.amount > 0) return;
            Set_Item(null);
            
            return;
        }

        if (selectTile.Placed_StackableItems() > 1) return;
        if (currentItem.stackable == false && selectTile.NonStackableItem_Placed()) return;

        GameObject spawnedItem = Instantiate(currentItem.placeablePrefab, selectTile.placeableItemsPrefabs);
        spawnedItem.transform.localPosition = currentItem.offsetPosition;
        
        PlaceableItem placedItem = spawnedItem.GetComponent<PlaceableItem>();
        
        placedItem.Set_Data(new(currentItem, 1));
        placedItem.Track_CurrentTile(selectTile);
        selectTile.Track_PlacingItem(placedItem);

        Update_Item(new(currentItem, _itemData.amount - 1));
        if (_itemData.amount > 0) return;
        Set_Item(null);
    }
    
    private void Place_AllItem(Tile selectTile)
    {
        if (_itemData == null)
        {
            Pickup_Item(selectTile);
            return;
        }

        int placeCount = _itemData.amount;

        while (placeCount > 0)
        {
            Place_Item(selectTile);

            if (_itemData == null) return;
            placeCount--;
        }
    }


    private void Use_Item(Tile selectTile)
    {
        if (_itemData == null) return;
        Item_ScrObj currentItem = _itemData.itemScrObj;

        if (currentItem.placeablePrefab != null) return;

        // get useable item inherent class from Data_Manager
    }
}
