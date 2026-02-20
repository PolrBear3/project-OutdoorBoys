using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ItemCursor : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;

    
    private ItemData _currentItemData;
    public ItemData currentItemData => _currentItemData;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTileSelect -= Place_CurrentItem;
        tilesController.OnTileSelect -= Use_CurrentItem;
    }


    // Data
    private void Set_Data()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTileSelect += Place_CurrentItem;
        tilesController.OnTileSelect += Use_CurrentItem;

        Set_CurrentItem(new(Data_Manager.instance.ItemScrObj("Tarp"), 5));
    }

    public void Set_CurrentItem(ItemData setItemData)
    {
        _currentItemData = setItemData;

        // range
        int updateRange = setItemData != null ? setItemData.itemScrObj.triggerRange : 0;
        _cursor.Update_TilePointerRange(updateRange);

        // sprite
        Sprite itemSprite = setItemData != null ? setItemData.itemScrObj.inventorySprite : null;
        _cursor.Update_PointerSprite(itemSprite);

        // amount text
        if (setItemData == null)
        {
            _cursor.amountText.gameObject.SetActive(false);
            return;
        }
        _cursor.Update_AmountText(setItemData.count);
    }


    // Item Trigger
    private void Pickup_CurrentItem(Tile selectTile)
    {        
        List<PlaceableItem> placedItems = selectTile.placedItems;
        if (placedItems.Count <= 0) return;

        PlaceableItem pickupItem = placedItems[0];

        Set_CurrentItem(pickupItem.data);
        selectTile.Remove_PlacedItem(pickupItem);
    }

    private void Place_CurrentItem(Tile selectTile)
    {
        if (_currentItemData == null)
        {
            Pickup_CurrentItem(selectTile);
            return;
        }

        Item_ScrObj currentItem = _currentItemData.itemScrObj;
        List<PlaceableItem> placedItems = selectTile.placedItems;

        for (int i = 0; i < placedItems.Count; i++)
        {
            ItemData placedItemData = placedItems[i].data;

            if (currentItem != placedItemData.itemScrObj) continue;
            if (placedItemData.count >= currentItem.maxAmount) continue;

            _currentItemData.Update_CurrentCount(_currentItemData.count - 1);
            _cursor.Update_AmountText(_currentItemData.count);

            placedItemData.Update_CurrentCount(placedItemData.count + 1);
            return;
        }

        if (selectTile.Placed_StackableItems() > 1) return;
        if (currentItem.stackable == false && selectTile.NonStackableItem_Placed()) return;

        GameObject spawnedItem = Instantiate(currentItem.placeablePrefab);
        Transform itemTransform = spawnedItem.transform;

        PlaceableItem placedItem = spawnedItem.GetComponent<PlaceableItem>();
        placedItem.Set_Data(new(currentItem, 1));
        
        placedItem.Track_CurrentTile(selectTile);
        selectTile.Track_PlacingItem(placedItem);
        
        itemTransform.SetParent(selectTile.placeableItemsPrefabs);
        itemTransform.localPosition = Vector2.zero + currentItem.offsetPosition;

        _currentItemData.Update_CurrentCount(_currentItemData.count - 1);

        if (_currentItemData.count > 0)
        {
            _cursor.Update_AmountText(_currentItemData.count);
            return;
        }
        Set_CurrentItem(null);
    }
    

    private void Use_CurrentItem(Tile selectTile)
    {
        if (_currentItemData == null) return;
        Item_ScrObj currentItem = _currentItemData.itemScrObj;

        if (currentItem.placeablePrefab != null) return;

        // get useable item inherent class from Data_Manager
    }
}
