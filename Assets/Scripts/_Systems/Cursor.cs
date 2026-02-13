using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private RectTransform _rect;
    [SerializeField] private Image _image;

    private bool _pointerVisible;

    private Tile _pointTile;
    public Tile pointTile => _pointTile;


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


    public void Update_PointTile(Tile pointTile)
    {
        _pointTile = pointTile;
    }
}
