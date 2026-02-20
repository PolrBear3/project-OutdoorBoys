using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventPointer _pointer;

    [SerializeField] private Transform _setPosition;
    public Transform setPosition => _setPosition;

    [Space(20)]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private SpriteRenderer _pointerRenderer;
    public SpriteRenderer pointerRenderer => _pointerRenderer;

    [SerializeField] private SpriteRenderer _shadowRenderer;

    [Space(20)]
    [SerializeField] private Transform _placeableItemsPrefabs;
    public Transform placeableItemsPrefabs => _placeableItemsPrefabs;

    [SerializeField] private Transform _droppedItemsPrefabs;
    public Transform droppedItemsPrefabs => _droppedItemsPrefabs;

    [Space(20)]
    [SerializeField][Range(0, 1)] private float _shadowValue;
    [SerializeField][Range(0, 10)] private float _shadowUpdateTime;


    private TileData _data;
    public TileData data => _data;

    private List<PlaceableItem> _placedItems = new();
    public List<PlaceableItem> placedItems => _placedItems;
    
    
    private bool _pointerToggled;
    public bool pointerToggled => _pointerToggled;

    private bool _shadowToggled;
    public bool shadowToggled => _shadowToggled;

    private Coroutine _shadowUpdateCoroutine;


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
        
        _spriteRenderer.sprite = isBaseTile ? sprites[1] : sprites[0];
    }


    // EventPointer
    public void Toggle_Pointer()
    {
        Toggle_Pointer(_pointer.pointerDetected);
    }
    private void Toggle_Pointer(bool toggle)
    {
        Cursor cursor = InGame_Manager.instance.cursor;
        bool activeToggle = toggle && cursor.PointingTile_InRange(this);

        _pointerToggled = activeToggle;
        _pointerRenderer.gameObject.SetActive(activeToggle);

        Tile cursorPointTile = activeToggle ? this : null;
        cursor.Track_PointingTile(cursorPointTile);
    }


    // Shadow
    public void Toggle_Shadow(bool toggle)
    {
        _shadowToggled = toggle;
        
        if (_shadowUpdateCoroutine != null)
        {
            StopCoroutine(_shadowUpdateCoroutine);
            _shadowUpdateCoroutine = null;

            LeanTween.cancel(_shadowRenderer.gameObject);
        }

        _shadowUpdateCoroutine = StartCoroutine(Shadow_Update(toggle));
    }
    private IEnumerator Shadow_Update(bool toggle)
    {
        GameObject shadow = _shadowRenderer.gameObject;
        if (toggle) shadow.SetActive(true);
        
        float shadowValue = toggle ? _shadowValue : 0f;
        LeanTween.alpha(shadow, shadowValue, _shadowUpdateTime);

        yield return new WaitForSeconds(_shadowUpdateTime);
        
        if (toggle) yield break;
        shadow.SetActive(false);
    }


    // Item Datas
    public void Track_PlacingItem(PlaceableItem placingItem)
    {
        _placedItems.Add(placingItem);
        _data.placedItemDatas.Add(placingItem.data);
    }

    public void Remove_PlacedItem(PlaceableItem placingItem)
    {
        _placedItems.Remove(placingItem);
        _data.placedItemDatas.Remove(placingItem.data);

        Destroy(placingItem.gameObject);
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
}