using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventPointer _pointer;
    public EventPointer pointer => _pointer;

    [SerializeField] private AnimationPlayer _selectorAnimPlayer;

    [Space(20)]
    [SerializeField] private SpriteRenderer _tileSpriteRenderer;

    [Space(10)]
    [SerializeField] private Transform _setPosition;
    [SerializeField] private SpriteRenderer _boundPointRenderer;

    [Space(20)]
    [SerializeField] private Item_ScrObj _useableItemDrop;


    private TileData _data;
    public TileData data => _data;

    private const int _maxItemPlaceCount = 2;
    private List<PlaceableItem> _placedItems = new();

    public Action<GameObject> OnSetPrefab;

    private const int _stateSetTime = 3;
    public Action<TileState, bool> OnStateUpdate;


    // MonoBehaviour
    private void OnDestroy()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        _pointer.OnEnter -= tilesController.Hover_Tile;
        _pointer.OnExit -= tilesController.Hover_Tile;

        _pointer.OnPointerHoldState -= tilesController.HoldHover_Tile;

        _pointer.OnEnter -= Toggle_SelectReady;
        _pointer.OnExit -= Toggle_SelectReady;
    }


    // Debug
    public int PlacedItem_IndexNum(PlaceableItem targetItem)
    {
        for (int i = 0; i < _placedItems.Count; i++)
        {
            if (targetItem == _placedItems[i]) return i;
        }
        return -1;
    }


    // Data
    public TileData Set_Data(TileScrObj setTile)
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        _pointer.OnEnter += tilesController.Hover_Tile;
        _pointer.OnExit += tilesController.Hover_Tile;

        _pointer.OnPointerHoldState += tilesController.HoldHover_Tile;

        _pointer.OnEnter += Toggle_SelectReady;
        _pointer.OnExit += Toggle_SelectReady;

        if (setTile == null) return null;

        _data = new(setTile);
        Load_StaticStates();

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

    public void Set_CurrentPrefab(GameObject setPrefab)
    {
        if (setPrefab == null) return;

        setPrefab.transform.SetParent(_setPosition);
        OnSetPrefab?.Invoke(setPrefab);
    }
    public List<GameObject> All_CurrentPrefabs()
    {
        List<GameObject> prefabs = new();

        foreach (Transform child in _setPosition)
        {
            prefabs.Add(child.gameObject);
        }
        return prefabs;
    }


    public Vector2 Random_BoundPoint()
    {
        Bounds bounds = _boundPointRenderer.bounds;

        float randomX = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
        float randomY = UnityEngine.Random.Range(bounds.min.y, bounds.max.y);

        return new(randomX, randomY);
    }

    public int DistanceTo_TargetTile(Tile targetTile)
    {
        if (targetTile == null || targetTile == this) return 0;

        return Utility.Chebyshev_Distance(transform.position, targetTile.transform.position);
    }


    // Select Toggles
    public void Toggle_SelectPreview(bool toggle)
    {
        _selectorAnimPlayer.spriteRenderer.gameObject.SetActive(toggle);
    }

    public void Toggle_SelectReady(bool toggle)
    {
        if (!_selectorAnimPlayer.spriteRenderer.gameObject.activeSelf && !toggle) return;
        int clipIndex = toggle ? 1 : 0;

        if (_selectorAnimPlayer.Animation_Playing(_selectorAnimPlayer.AnimationClip(clipIndex))) return;
        _selectorAnimPlayer.Play(clipIndex);
    }
    public void Toggle_SelectReady()
    {
        Toggle_SelectReady(_pointer.pointerDetected && InGame_Manager.instance.tilesController.Tile_Pointable(this));
    }


    // State
    private void Load_StaticStates()
    {
        TileState[] staticStates = _data.tileScrObj.staticStates;

        foreach (TileState state in staticStates)
        {
            _data.stateDatas.Add(new(state, _stateSetTime));
        }
    }

    public void Add_State(TileState stateToAdd)
    {
        if (_data.tileScrObj.Is_StaticState(stateToAdd) == false)
        {
            _data.Add_StateData(new(stateToAdd, _stateSetTime + 1));
        }
        OnStateUpdate?.Invoke(stateToAdd, true);
    }
    public void Remove_State(TileState stateToRemove)
    {
        if (_data.tileScrObj.Is_StaticState(stateToRemove) == false)
        {
            _data.Remove_StateData(stateToRemove);
        }
        OnStateUpdate?.Invoke(stateToRemove, false);
    }


    // Current Placed Items
    public void Track_PlacingItem(PlaceableItem placingItem)
    {
        if (_placedItems == null) return;

        _placedItems.Add(placingItem);
        _data.placedItemDatas.Add(placingItem.data);

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        tilesController.placedItems.Add(placingItem);
        tilesController.OnTileItemsUpdate?.Invoke(this);

        Set_CurrentPrefab(placingItem.gameObject);
    }

    /// <returns>
    /// Leftover data
    /// </returns>
    public ItemData Set_PlacingItem(ItemData setItemData)
    {
        if (setItemData == null) return setItemData;

        Item_ScrObj setItem = setItemData.itemScrObj;
        if (setItem.Place_Available(setItemData, this) == false) return setItemData;

        int setItemAmount = setItemData.amount;
        if (setItemAmount <= 0) return setItemData;

        if (setItem.itemType == ItemType.use) return setItemData;

        int maxAmount = setItem.maxAmount;
        List<PlaceableItem> samePlacedItems = PlacedItems(setItem);

        // update amount
        for (int i = 0; i < samePlacedItems.Count; i++)
        {
            PlaceableItem placedItem = samePlacedItems[i];
            ItemData placedItemData = placedItem.data;

            int placedItemAmount = placedItemData.amount;
            if (placedItemAmount >= maxAmount) continue;

            int leftSpaceAmount = maxAmount - placedItemAmount;
            int amountToAdd = Mathf.Min(setItemAmount, leftSpaceAmount);

            placedItemData.Update_CurrentAmount(placedItemAmount + amountToAdd);
            setItemAmount -= amountToAdd;

            placedItem.Play_PlaceAnimation();

            if (setItemAmount > 0) continue;

            InGame_Manager.instance.tilesController.OnTileItemsUpdate(this);
            return null;
        }

        // spawn new
        for (int i = 0; i < _maxItemPlaceCount; i++)
        {
            if (setItemAmount <= 0) return null;
            if (_placedItems.Count >= _maxItemPlaceCount) break;

            GameObject spawnedItem = Instantiate(setItem.itemPrefab);
            Set_CurrentPrefab(spawnedItem);

            PlaceableItem newPlacedItem = spawnedItem.GetComponent<PlaceableItem>();
            int spawnSetAmount = Mathf.Min(setItemAmount, maxAmount);

            newPlacedItem.Set_Data(new(setItem, spawnSetAmount));
            setItemAmount -= spawnSetAmount;

            newPlacedItem.Track_CurrentTile(this);

            Track_PlacingItem(newPlacedItem);
            Update_PlacedItemOffsets();

            newPlacedItem.Play_PlaceAnimation();
        }

        return new(setItem, setItemAmount);
    }

    /// <returns>
    /// Check if item set successfully
    /// </returns>
    public bool Set_UseItem(ItemData setItemData)
    {
        if (_placedItems.Count >= _maxItemPlaceCount) return false;

        if (setItemData == null || setItemData.amount <= 0) return false;
        Item_ScrObj setItem = setItemData.itemScrObj;

        if (setItem.itemType != ItemType.use) return false;

        GameObject spawnedDrop = Instantiate(_useableItemDrop.itemPrefab, _setPosition);
        Set_CurrentPrefab(spawnedDrop);

        PlaceableItem placedUseItem = spawnedDrop.GetComponent<PlaceableItem>();

        placedUseItem.Set_Data(new(setItem, setItemData.amount));
        placedUseItem.Track_CurrentTile(this);
        placedUseItem.animPlayer.spriteRenderer.sprite = setItem.microSprite;

        Track_PlacingItem(placedUseItem);
        Update_PlacedItemOffsets();

        return true;
    }

    /// <summary>
    /// Sets item according to item type
    /// </summary>
    /// <returns>
    /// Leftover data
    /// </returns>
    public ItemData Set_Item(ItemData setItemData)
    {
        if (setItemData == null) return setItemData;

        if (setItemData.itemScrObj.itemType != ItemType.use)
        {
            return Set_PlacingItem(setItemData);
        }

        if (Set_UseItem(setItemData)) return null;
        return setItemData;
    }
    public void SetPreserve_Item(ItemData setItemData)
    {
        ItemData leftOverData = Set_Item(setItemData);

        if (leftOverData == null) return;

        _data.Preserve_ItemData(leftOverData);
        InGame_Manager.instance.tilesController.OnTileItemsUpdate(this);
    }


    private void Update_PlacedItemOffsets()
    {
        int placedItemCount = _placedItems.Count;

        for (int i = 0; i < placedItemCount; i++)
        {
            PlaceableItem placedItem = _placedItems[i];
            Offset_PositionData positionData = placedItem.data.itemScrObj.Offset_Data(i + placedItemCount - 1);

            placedItem.transform.SetLocalPositionAndRotation(positionData.position, Quaternion.Euler(0f, 0f, positionData.rotationValue));
        }
    }
    public void ClampUpdate_PlacedItemOffsets(float clampDuration)
    {
        int placedItemCount = _placedItems.Count;

        for (int i = 0; i < placedItemCount; i++)
        {
            PlaceableItem placedItem = _placedItems[i];

            GameObject placedItemObject = placedItem.gameObject;
            LeanTween.cancel(placedItemObject);

            Offset_PositionData positionData = placedItem.data.itemScrObj.Offset_Data(i + placedItemCount - 1);

            LeanTween.moveLocal(placedItemObject, positionData.position, clampDuration).setEase(LeanTweenType.easeOutElastic);
            LeanTween.rotateLocal(placedItemObject, new(0f, 0f, positionData.rotationValue), clampDuration);
        }
    }


    public void Remove_PlacedItemData(PlaceableItem PlacedItem)
    {
        _placedItems.Remove(PlacedItem);
        _data.placedItemDatas.Remove(PlacedItem.data);

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;
        tilesController.placedItems.Remove(PlacedItem);

        List<ItemData> preservedDatas = new(_data.preservedItemDatas);
        _data.preservedItemDatas.Clear();

        for (int i = 0; i < preservedDatas.Count; i++)
        {
            ItemData leftOverData = Set_Item(preservedDatas[i]);

            if (leftOverData == null) continue;
            _data.Preserve_ItemData(leftOverData);
        }
        tilesController.OnTileItemsUpdate?.Invoke(this);

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
        if (placeItem.itemType == ItemType.use) return (_maxItemPlaceCount - _placedItems.Count) * placeItem.maxAmount;

        int availableCount = 0;
        int maxStackAmount = placeItem.maxAmount;

        List<ItemData> samePlacedItems = new(Placed_ItemDatas(placeItem));

        for (int i = 0; i < samePlacedItems.Count; i++)
        {
            int leftSpaceAmount = Mathf.Max(0, maxStackAmount - samePlacedItems[i].amount);
            availableCount += leftSpaceAmount;
        }

        int newPlaceCount = Mathf.Max(0, _maxItemPlaceCount - _placedItems.Count);
        availableCount += newPlaceCount * maxStackAmount;

        return availableCount;
    }
    public int ItemPlace_AvailableCount(ItemData placedItemData)
    {
        if (placedItemData == null) return 0;

        Item_ScrObj placeItem = placedItemData.itemScrObj;
        if (placeItem.Place_Available(placedItemData, this) == false) return 0;

        return ItemPlace_AvailableCount(placeItem);
    }


    public PlaceableItem PlacedItem(Item_ScrObj targetItem)
    {
        for (int i = 0; i < _placedItems.Count; i++)
        {
            PlaceableItem placedItem = _placedItems[i];

            if (targetItem != placedItem.data.itemScrObj) continue;
            return placedItem;
        }
        return null;
    }

    public List<PlaceableItem> PlacedItems()
    {
        List<PlaceableItem> placedItems = new();

        foreach (PlaceableItem item in _placedItems)
        {
            placedItems.Add(item);
        }
        return placedItems;
    }
    public List<PlaceableItem> PlacedItems(Item_ScrObj targetItem)
    {
        if (targetItem == null) return null;

        List<PlaceableItem> placedItems = new();

        for (int i = 0; i < _placedItems.Count; i++)
        {
            PlaceableItem placedItem = _placedItems[i];

            if (targetItem != placedItem.data.itemScrObj) continue;
            placedItems.Add(placedItem);
        }
        return placedItems;
    }
    public List<PlaceableItem> PlacedItems(List<Item_ScrObj> targetItems)
    {
        if (targetItems.Count <= 0) return null;

        List<PlaceableItem> placedItems = new();

        for (int i = 0; i < _placedItems.Count; i++)
        {
            PlaceableItem placedItem = _placedItems[i];

            for (int j = 0; j < targetItems.Count; j++)
            {
                if (placedItem.data.itemScrObj != targetItems[j]) continue;
                placedItems.Add(placedItem);
            }
        }
        return placedItems;
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
    public List<ItemData> Placed_ItemDatas(Item_ScrObj targetItem)
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
}