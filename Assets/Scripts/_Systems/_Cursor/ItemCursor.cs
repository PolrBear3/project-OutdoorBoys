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

    public Action<ItemData> OnItemDataUpdate;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTileInteract -= Place_CurrentItem;
        tilesController.OnTileInteract -= Use_CurrentItem;
    }


    // Data
    private void Set_Data()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.OnTileInteract += Place_CurrentItem;
        tilesController.OnTileInteract += Use_CurrentItem;

        Set_CurrentItemData(new(Data_Manager.instance.ItemScrObj("Tarp"), 3));
        Update_TilePointerRange();
    }

    public void Set_CurrentItemData(ItemData setData)
    {
        _currentItemData = setData;

        OnItemDataUpdate?.Invoke(_currentItemData);
    }


    // Item Trigger
    private void Update_TilePointerRange()
    {
        int updateRange = _currentItemData != null ? _currentItemData.itemScrObj.triggerRange : 0;
        
        _cursor.Update_TilePointerRange(updateRange);
    }


    private void Place_CurrentItem(Tile interactTile)
    {
        if (_currentItemData == null)
        {
            Pickup_CurrentItem(interactTile);
            return;
        }

        Item_ScrObj currentItem = _currentItemData.itemScrObj;
        
        if (currentItem.placeablePrefab == null) return;
        if (interactTile.Placed_StackableItems() > 1) return;
        if (currentItem.stackable == false && interactTile.NonStackableItem_Placed()) return;

        GameObject spawnedItem = Instantiate(currentItem.placeablePrefab);
        Transform itemTransform = spawnedItem.transform;
        PlaceableItem placedItem = spawnedItem.GetComponent<PlaceableItem>();
        
        placedItem.Set_Data(_currentItemData);
        placedItem.Set_CurrentTile(interactTile);
        
        interactTile.Track_PlacingItem(placedItem);
        
        itemTransform.SetParent(interactTile.placeableItemsPrefabs);
        itemTransform.localPosition = Vector2.zero + currentItem.offsetPosition;

        Set_CurrentItemData(null);
        Update_TilePointerRange();
    }
    private void Pickup_CurrentItem(Tile interactTile)
    {
        if (_currentItemData != null) return;

        List<PlaceableItem> placedItemDatas = interactTile.placedItems;
        if (placedItemDatas.Count <= 0) return;

        Set_CurrentItemData(placedItemDatas[0].data);
        Update_TilePointerRange();

        interactTile.Remove_PlacedItem(placedItemDatas[0]);
    }

    private void Use_CurrentItem(Tile interactTile)
    {
        if (_currentItemData == null) return;
        Item_ScrObj currentItem = _currentItemData.itemScrObj;

        if (currentItem.placeablePrefab != null) return;

        // get useable item inherent class from Data_Manager
    }
}
