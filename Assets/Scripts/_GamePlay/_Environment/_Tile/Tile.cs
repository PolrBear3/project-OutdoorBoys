using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventPointer _pointer;
    public EventPointer pointer => _pointer;

    [SerializeField] private Transform _setPosition;
    public Transform setPosition => _setPosition;

    [Space(20)]
    [SerializeField] private SpriteRenderer _tileSpriteRenderer;
    [SerializeField] private SpriteRenderer _rangeIndicator;
    [SerializeField] private SpriteRenderer _pointerRenderer;

    [Space(20)]
    [SerializeField] private Transform _placeableItemsPrefabs;

    [SerializeField] private Transform _droppedItemsPrefabs;
    public Transform droppedItemsPrefabs => _droppedItemsPrefabs;

    [SerializeField] private Transform _otherPrefabs;
    public Transform otherPrefabs => _otherPrefabs;

    [Space(20)]
    [SerializeField][Range(0, 1)] private float _transparencyValue;
    [SerializeField][Range(0, 10)] private float _transparencyTransitionTime;


    private TileData _data;
    public TileData data => _data;

    private List<PlaceableItem> _placedItems = new();
    public List<PlaceableItem> placedItems => _placedItems;

    private bool _pointerToggled;
    public bool pointerToggled => _pointerToggled;


    // MonoBehaviour
    private void OnDestroy()
    {
        _pointer.OnEnter -= Toggle_Pointer;
        _pointer.OnExit -= Toggle_Pointer;
    }


    // Data
    public TileData Set_Data(TileScrObj setTile)
    {
        _pointer.OnEnter += Toggle_Pointer;
        _pointer.OnExit += Toggle_Pointer;

        if (setTile == null) return null;

        _data = new(setTile);
        return _data;
    }

    public void Update_SetSprite(bool isBaseTile)
    {
        if (_data == null) return;

        TileScrObj tileScrObj = _data.tileScrObj;
        if (tileScrObj == null) return;

        Sprite[] sprites = tileScrObj.GroupedSprites();
        if (sprites.Length <= 1) return;

        _tileSpriteRenderer.sprite = isBaseTile ? sprites[1] : sprites[0];
    }


    public int DistanceTo_TargetTile(Tile targetTile)
    {
        if (targetTile == null || targetTile == this) return 0;

        return Utility.Chebyshev_Distance(transform.position, targetTile.transform.position);
    }


    // Toggles
    public void Toggle_Transparency(bool toggle)
    {
        LeanTween.cancel(_tileSpriteRenderer.gameObject);

        float value = toggle ? _transparencyValue : 1f;
        LeanTween.alpha(_tileSpriteRenderer.gameObject, value, _transparencyTransitionTime);
    }

    public void Toggle_Pointer()
    {
        _pointerToggled = InGame_Manager.instance.cursor.PointingTile_InRange(this) && _pointer.pointerDetected;
        _pointerRenderer.gameObject.SetActive(_pointerToggled);

        Tile cursorPointTile = _pointerToggled ? this : null;
        InGame_Manager.instance.cursor.Track_PointingTile(cursorPointTile);
    }


    // Current Placed Items
    public void Track_PlacingItem(PlaceableItem placingItem)
    {
        _placedItems.Add(placingItem);
        _data.placedItemDatas.Add(placingItem.data);
    }
    
    /// <returns>
    /// Leftover data
    /// </returns>
    public ItemData Set_PlacingItem(ItemData setItemData)
    {
        if (setItemData == null) return setItemData;
        Item_ScrObj setItem = setItemData.itemScrObj;

        if (setItem.itemType != ItemType.place) return setItemData;
        PlaceableItem placedItem = PlacedItem(setItem);

        int maxAmount = setItem.maxAmount;
        int setItemAmount = setItemData.amount;

        if (placedItem == null)
        {
            // spawn new
            GameObject spawnedItem = Instantiate(setItem.itemPrefab, _placeableItemsPrefabs);
            spawnedItem.transform.localPosition = setItem.offsetPosition;

            placedItem = spawnedItem.GetComponent<PlaceableItem>();

            placedItem.Set_Data(new(setItem, Mathf.Min(setItemData.amount, maxAmount)));
            placedItem.Track_CurrentTile(this);

            Track_PlacingItem(placedItem);
            
            int overflowAmount = setItemAmount - maxAmount;

            if (overflowAmount <= 0) return null;
            return new(setItem, overflowAmount);
        }

        // update amount
        int placedItemAmount = placedItem.data.amount;
        if (placedItemAmount >= maxAmount) return setItemData;
        
        int leftSpaceAmount = maxAmount - placedItemAmount;
        int amountToAdd = Mathf.Min(setItemAmount, leftSpaceAmount);

        placedItem.data.Update_CurrentAmount(placedItemAmount + amountToAdd);

        int leftOverAmount = setItemAmount - amountToAdd;
        return leftOverAmount > 0 ? new(setItem, leftOverAmount) : null;
    }


    public void Remove_PlacedItemData(PlaceableItem PlacedItem)
    {
        _placedItems.Remove(PlacedItem);
        _data.placedItemDatas.Remove(PlacedItem.data);
    }
    public void Remove_EmptyPlacedItems()
    {
        for (int i = _placedItems.Count - 1; i >= 0; i--)
        {
            PlaceableItem placedItem = _placedItems[i];

            if (placedItem.data.amount > 0) continue;

            Remove_PlacedItemData(placedItem);
            Destroy(placedItem.gameObject);
        }
    }


    public int Placed_ItemCount(Item_ScrObj targetItem)
    {
        int count = 0;

        for (int i = 0; i < _placedItems.Count; i++)
        {
            ItemData placedItemData = _placedItems[i].data;

            if (targetItem != placedItemData.itemScrObj) continue;
            count += placedItemData.amount;
        }
        return count;
    }

    private int Placed_StackableItemCount()
    {
        int count = 0;

        for (int i = 0; i < _placedItems.Count; i++)
        {
            if (_placedItems[i].data.itemScrObj.stackable == false) continue;
            count++;
        }

        return count;
    }


    public List<ItemData> Placed_ItemDatas()
    {
        List<ItemData> placedDatas = new();

        for (int i = 0; i < _placedItems.Count; i++)
        {
            placedDatas.Add(_placedItems[i].data);
        }
        return placedDatas;
    }

    public PlaceableItem PlacedItem(Item_ScrObj targetItem)
    {
        for (int i = 0; i < _placedItems.Count; i++)
        {
            if (targetItem != _placedItems[i].data.itemScrObj) continue;
            return _placedItems[i];
        }
        return null;
    }


    private bool NonStackableItem_Placed()
    {
        for (int i = 0; i < _placedItems.Count; i++)
        {
            if (_placedItems[i].data.itemScrObj.stackable) continue;
            return true;
        }

        return false;
    }

    public bool ItemPlacing_Available(Item_ScrObj itemToPlace)
    {
        if (_placedItems.Count >= 2) return false;
        if (Placed_ItemCount(itemToPlace) >= itemToPlace.maxAmount) return false;

        if (Placed_StackableItemCount() > 1) return false;
        if (itemToPlace.stackable == false && NonStackableItem_Placed()) return false;

        return true;
    }
}