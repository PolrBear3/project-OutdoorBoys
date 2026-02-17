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
        _pointer.OnClick -= Click_Interact;
    }


    // Data
    public TileData Set_Data(TileScrObj setTile)
    {
        _pointer.OnEnter += Toggle_Pointer;
        _pointer.OnExit += Toggle_Pointer;
        _pointer.OnClick += Click_Interact;

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
    private void Toggle_Pointer()
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

    private void Click_Interact()
    {
        if (_pointerToggled == false) return;
        
        Tiles_Controller controller = InGame_Manager.instance.tilesController;
        controller.OnTileInteract?.Invoke(this);
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
}