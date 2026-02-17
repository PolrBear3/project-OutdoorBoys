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
        EventBus_Manager.Register(EventBus.StartLoad, Update_TilePointerRange);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Update_TilePointerRange);

        InGame_Manager.instance.tilesController.OnTileInteract -= Trigger_CurrentItem;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager.instance.tilesController.OnTileInteract += Trigger_CurrentItem;

        Set_CurrentItemData(new(Data_Manager.instance.ItemScrObj("Tarp"), 3));
    }

    public void Set_CurrentItemData(ItemData setData)
    {
        if (setData == null)
        {
            _currentItemData = null;
            return;
        }
        _currentItemData = setData;
    }


    // Item Trigger
    private void Update_TilePointerRange()
    {
        int updateRange = _currentItemData == null ? 0 : _currentItemData.itemScrObj.triggerRange;
        _cursor.Update_TilePointerRange(updateRange);
    }

    public void Trigger_CurrentItem(Tile interactTile)
    {
        if (_currentItemData == null) return;
        Item_ScrObj currentItem = _currentItemData.itemScrObj;

        if (currentItem.placeablePrefab == null)
        {
            // get useable item inherent class from Data_Manager
            
            return;
        }

        TileData tileData = interactTile.data;

        if (tileData.Placed_StackableItems() > 1) return;
        if (currentItem.stackable == false && tileData.NonStackableItem_Placed()) return;

        tileData.placedItemDatas.Add(_currentItemData);
        _currentItemData = null;

        GameObject spawnedItem = Instantiate(currentItem.placeablePrefab);
        Transform itemTransform = spawnedItem.transform;

        itemTransform.SetParent(interactTile.placeableItemsPrefabs);
        itemTransform.localPosition = Vector2.zero + currentItem.offsetPosition;

        if (spawnedItem.TryGetComponent(out PlaceableItem placedItem) == false) return;
        placedItem.Set_CurrentTile(interactTile);
    }
}
