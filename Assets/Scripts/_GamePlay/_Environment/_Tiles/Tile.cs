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

    [Space(20)]
    [SerializeField][Range(0, 10)] private int _maxItemPlaceCount;


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

        Update_PlacedItemOffsets();
    }
    
    /// <returns>
    /// Leftover data
    /// </returns>
    public ItemData Set_PlacingItem(ItemData setItemData)
    {
        if (setItemData == null) return setItemData;
        Item_ScrObj setItem = setItemData.itemScrObj;

        if (setItem.itemType != ItemType.place) return setItemData;

        int maxAmount = setItem.maxAmount;
        int setItemAmount = setItemData.amount;

        List<ItemData> samePlacedItemDatas = Placed_ItemDatas(setItem);

        // update amount
        for (int i = 0; i < samePlacedItemDatas.Count; i++)
        {
            int placedItemAmount = samePlacedItemDatas[i].amount;
            if (placedItemAmount >= maxAmount) continue;

            int leftSpaceAmount = maxAmount - placedItemAmount;
            int amountToAdd = Mathf.Min(setItemAmount, leftSpaceAmount);

            samePlacedItemDatas[i].Update_CurrentAmount(placedItemAmount + amountToAdd);
            setItemAmount -= amountToAdd;

            if (setItemAmount <= 0) return null;
        }

        // spawn new
        for (int i = 0; i < _maxItemPlaceCount; i++)
        {
            if (setItemAmount <= 0) return null;
            if (_placedItems.Count >= _maxItemPlaceCount) break;

            GameObject spawnedItem = Instantiate(setItem.itemPrefab, _placeableItemsPrefabs);
            PlaceableItem newPlacedItem = spawnedItem.GetComponent<PlaceableItem>();

            int spawnSetAmount = Mathf.Min(setItemAmount, maxAmount);

            newPlacedItem.Set_Data(new(setItem, spawnSetAmount));
            setItemAmount -= spawnSetAmount;

            newPlacedItem.Track_CurrentTile(this);
            Track_PlacingItem(newPlacedItem);
        }

        return new(setItem, setItemAmount);
    }

    private void Update_PlacedItemOffsets()
    {
        int placedItemCount = _placedItems.Count;

        for (int i = 0; i < placedItemCount; i++)
        {
            PlaceableItem placedItem = _placedItems[i];
            Transform transform = placedItem.transform;

            Offset_PositionData offsetData = placedItem.data.itemScrObj.Offset_Data(i + placedItemCount - 1);

            transform.localPosition = offsetData.position;
            transform.rotation = Quaternion.Euler(0f, 0f, offsetData.rotationValue);
        }
    }


    public void Remove_PlacedItemData(PlaceableItem PlacedItem)
    {
        _placedItems.Remove(PlacedItem);
        _data.placedItemDatas.Remove(PlacedItem.data);

        Update_PlacedItemOffsets();
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

    public int ItemPlace_AvailableCount(Item_ScrObj placeItem)
    {
        if (placeItem == null) return 0;
        
        int availableCount = 0;
        int maxStackAmount = placeItem.maxAmount;

        List<ItemData> samePlacedItems = new(Placed_ItemDatas(placeItem));

        for (int i = 0; i < samePlacedItems.Count; i++)
        {
            int leftSpaceAmount = maxStackAmount - samePlacedItems[i].amount;
            availableCount += leftSpaceAmount;
        }
        
        int newPlaceCount = Mathf.Max(0, _maxItemPlaceCount - _placedItems.Count);
        availableCount += newPlaceCount * maxStackAmount;

        return availableCount;
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
    private List<ItemData> Placed_ItemDatas(Item_ScrObj targetItem)
    {
        List<ItemData> placedItems = new();

        for (int i = 0; i < _placedItems.Count; i++)
        {
            ItemData data = _placedItems[i].data;

            if (targetItem != data.itemScrObj) continue;
            placedItems.Add(data);
        }
        return placedItems;
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
}