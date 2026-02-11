using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private EventPointer _pointer;

    [Space(20)]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [SerializeField] private SpriteRenderer _pointerRenderer;
    public SpriteRenderer pointerRenderer => _pointerRenderer;


    private TileData _data;
    public TileData data => _data;

    private bool _pointerToggled;
    public bool pointerToggled => _pointerToggled;


    // MonoBehaviour
    private void Awake()
    {
        _pointer.OnEnter += Toggle_Pointer;
        _pointer.OnExit += Toggle_Pointer;
    }

    private void OnDestroy()
    {
        _pointer.OnEnter -= Toggle_Pointer;
        _pointer.OnExit -= Toggle_Pointer;
    }


    // Data
    public TileData Set_Data(TileScrObj setTile)
    {
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


    // Pointer
    private void Toggle_Pointer()
    {
        Toggle_Pointer(_pointer.pointerDetected);
    }
    private void Toggle_Pointer(bool toggle)
    {
        _pointerToggled = toggle;
        _pointerRenderer.gameObject.SetActive(toggle);

        Tile cursorPointTile = toggle ? this : null;
        InGame_Manager.instance.cursor.Update_PointTile(cursorPointTile);
    }
}