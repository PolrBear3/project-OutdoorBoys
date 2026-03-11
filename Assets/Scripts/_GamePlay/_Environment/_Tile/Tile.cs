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
    public Transform placeableItemsPrefabs => _placeableItemsPrefabs;

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


    // Item Datas
    public void Track_PlacingItem(PlaceableItem placingItem)
    {
        _placedItems.Add(placingItem);
        _data.placedItemDatas.Add(placingItem.data);
    }

    public void Remove_PlacedItemData(PlaceableItem placingItem)
    {
        _placedItems.Remove(placingItem);
        _data.placedItemDatas.Remove(placingItem.data);
    }
    public void Remove_PlacedItem(PlaceableItem placingItem)
    {
        Remove_PlacedItemData(placingItem);
        Destroy(placingItem.gameObject);
    }
    public void RemoveUpdate_PlacedItems()
    {
        for (int i = _placedItems.Count - 1; i >= 0; i--)
        {
            PlaceableItem placedItem = _placedItems[i];

            if (placedItem.data.amount > 0) continue;
            Remove_PlacedItem(placedItem);
        }
    }


    public int Placed_StackableItems()
    {
        int count = 0;

        for (int i = 0; i < _placedItems.Count; i++)
        {
            if (_placedItems[i].data.itemScrObj.stackable == false) continue;
            count++;
        }

        return count;
    }

    public bool NonStackableItem_Placed()
    {
        for (int i = 0; i < _placedItems.Count; i++)
        {
            if (_placedItems[i].data.itemScrObj.stackable) continue;
            return true;
        }

        return false;
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
}