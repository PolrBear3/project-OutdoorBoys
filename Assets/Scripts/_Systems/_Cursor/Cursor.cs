using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Cursor : MonoBehaviour
{
    [SerializeField] private AnimationPlayerUI _animPlayer;
    public AnimationPlayerUI animPlayer => _animPlayer;

    [Space(10)]
    [SerializeField] private ItemCursor _itemCursor;
    public ItemCursor itemCursor => _itemCursor;
    
    [Space(20)]
    [SerializeField] private RectTransform _rect;

    [SerializeField] private TextMeshProUGUI _amountText;
    public TextMeshProUGUI amountText => _amountText;

    [Space(20)]
    [SerializeField] private Sprite _defaultPointerSprite;


    private bool _pointerVisible;

    private int _tilePointRange;
    public int tilePointRange => _tilePointRange;

    public Action OnTilePointRangeUpdate;

    private Tile _pointingTile;
    public Tile pointingTile => _pointingTile;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.Register(EventBus.AwakeLoad, Toggle_PointerVisibility);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Toggle_PointerVisibility);

        Input_Controller input = Input_Controller.instance;

        input.OnAnyInput -= Toggle_PointerVisibility;
        input.OnCursorControl -= Movement_Update;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;
        
        input.OnAnyInput += Toggle_PointerVisibility;
        input.OnCursorControl += Movement_Update;
    }


    // Pointer
    private void Toggle_PointerVisibility()
    {
        Toggle_PointerVisibility(true);
    }
    private void Toggle_PointerVisibility(bool toggle)
    {
        _pointerVisible = toggle;

        _rect.gameObject.SetActive(toggle);
        UnityEngine.Cursor.visible = !toggle;
    }

    private void Movement_Update(Vector2 cursorPosition)
    {
        if (_pointerVisible == false) return;
        _rect.position = cursorPosition;
    }


    public void Update_PointerSprite(Sprite sprite)
    {
        Sprite updateSprite = sprite != null ? sprite : _defaultPointerSprite;

        _animPlayer.image.sprite = updateSprite;
    }

    public void Update_AmountText(int updateAmount)
    {
        _amountText.text = updateAmount.ToString();
        
        if (_amountText.gameObject.activeSelf) return;
        _amountText.gameObject.SetActive(true);
    }


    // Tile Pointing
    public void Update_TilePointerRange(int range)
    {
        _tilePointRange = Mathf.Max(0, range);

        OnTilePointRangeUpdate?.Invoke();
    }


    public bool PointingTile_InRange(Tile pointTile)
    {
        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
        
        Vector2 playerTilePos = playerTile.transform.position;
        Vector2 pointTilePos = pointTile.transform.position;

        float xDistance = Mathf.Abs(playerTilePos.x - pointTilePos.x);
        float ydistance = Mathf.Abs(playerTilePos.y - pointTilePos.y);

        return Mathf.Max(xDistance, ydistance) <= _tilePointRange;
    }

    public void Track_PointingTile(Tile pointTile)
    {
        _pointingTile = pointTile;
    }
}