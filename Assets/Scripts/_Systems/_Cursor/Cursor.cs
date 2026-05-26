using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemCursor _itemCursor;
    public ItemCursor itemCursor => _itemCursor;

    [SerializeField] private RectTransform _rect;

    [Space(20)]
    [SerializeField] private Image _cursorImage;

    [SerializeField] private TextMeshProUGUI _amountText;
    public TextMeshProUGUI amountText => _amountText;

    [Space(10)]
    [SerializeField] private FillBar_UI _durabilityBar;
    public FillBar_UI durabilityBar => _durabilityBar;

    [Space(10)]
    [SerializeField] private Sprite _defaultPointerSprite;
    [SerializeField] private Sprite _pressedPointerSprite;

    [Space(20)]
    [SerializeField] private RectTransform _hoverInfoPanel;
    [SerializeField] private ItemSlot_Manager _placedItemsSlotManager;

    [Space(10)]
    [SerializeField] private GameObject _tileStateSlotsGroup;
    [SerializeField] private TileState_IndicationSlot[] _tileStateSlots;


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
        input.OnLeftClickStated -= Update_PointerSprite;
        input.OnCursorControl -= Movement_Update;

        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileHover -= Track_PointingTile;

        tilesController.OnTileHoldHover -= Toggle_HoverInfoPanel;
        tilesController.OnTileItemsUpdate -= Update_HoverInfoPanel;
        tilesController.OnTilesStatesTimeCount -= Toggle_HoverInfoPanel;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller input = Input_Controller.instance;

        input.OnAnyInput += Toggle_PointerVisibility;
        input.OnLeftClickStated += Update_PointerSprite;
        input.OnCursorControl += Movement_Update;

        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileHover += Track_PointingTile;

        tilesController.OnTileHoldHover += Toggle_HoverInfoPanel;
        tilesController.OnTileItemsUpdate += Update_HoverInfoPanel;
        tilesController.OnTilesStatesTimeCount += Toggle_HoverInfoPanel;

        Toggle_HoverInfoPanel(_pointingTile);
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
        _cursorImage.sprite = sprite != null ? sprite : _defaultPointerSprite;
    }
    private void Update_PointerSprite(bool isPressed)
    {
        if (_itemCursor.data != null) return;

        _cursorImage.sprite = isPressed ? _pressedPointerSprite : _defaultPointerSprite;
    }

    public void Update_AmountText(string updateString)
    {
        _amountText.text = updateString;

        if (_amountText.gameObject.activeSelf) return;
        _amountText.gameObject.SetActive(true);
    }
    public void Update_AmountText(int updateValue)
    {
        Update_AmountText(updateValue.ToString());
    }


    // Tile Pointing
    private void Track_PointingTile(Tile pointingTile)
    {
        _pointingTile = pointingTile;
    }

    public void Update_TilePointerRange(int range)
    {
        _tilePointRange = Mathf.Max(0, range);

        OnTilePointRangeUpdate?.Invoke();
    }
    public bool PointingTile_InRange(Tile pointingTile)
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();

        return playerTile.DistanceTo_TargetTile(pointingTile) <= _tilePointRange;
    }


    private void Toggle_HoverInfoPanel(Tile hoveringTile)
    {
        Dictionary<TileState, int> stateDatas = hoveringTile != null ? new(hoveringTile.data.stateDatas) : new();
        bool hasStateData = stateDatas.Count > 0;

        List<ItemData> placedItemDatas = hoveringTile != null ? new(hoveringTile.Placed_ItemDatas()) : new();
        int placedItemCount = placedItemDatas.Count;

        bool togglePanel = hoveringTile != null && hasStateData || placedItemCount > 0;
        _hoverInfoPanel.gameObject.SetActive(togglePanel);

        if (togglePanel == false) return;

        // tile states
        _tileStateSlotsGroup.SetActive(hasStateData);

        foreach (TileState_IndicationSlot slot in _tileStateSlots)
        {
            slot.gameObject.SetActive(false);
        }

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;
        int stateSlotsIndex = 0;

        foreach (var data in stateDatas)
        {
            if (stateSlotsIndex >= _tileStateSlots.Length) break;

            TileState_IndicationSlot stateSlot = _tileStateSlots[stateSlotsIndex];
            Sprite stateSprite = tilesController.TileState_VisualSprite(data.Key);

            stateSlot.gameObject.SetActive(stateSprite != null);

            stateSlot.stateIcon.sprite = stateSprite;
            stateSlot.timeCountText.text = "<sprite=0> " + data.Value;

            stateSlotsIndex++;
        }

        // placed items
        _placedItemsSlotManager.gameObject.SetActive(placedItemCount > 0);
        _placedItemsSlotManager.Clear_Datas();

        List<ItemSlot> itemSlots = _placedItemsSlotManager.slots;

        for (int i = 0; i < placedItemCount; i++)
        {
            if (i + 1 > itemSlots.Count) break;
            itemSlots[i].Set_Data(placedItemDatas[i]);
        }
        _placedItemsSlotManager.Update_Visuals();
    }
    private void Toggle_HoverInfoPanel()
    {
        if (_pointingTile == null || _pointingTile.pointer.pointerHoldCoroutine != null) return;

        Toggle_HoverInfoPanel(_pointingTile);
    }

    private void Update_HoverInfoPanel(Tile updateTile)
    {
        if (_pointingTile != updateTile) return;

        Toggle_HoverInfoPanel();
    }
}