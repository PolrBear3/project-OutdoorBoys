using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private RectTransform _rect;
    [SerializeField] private Image _image;

    private Tile _pointTile;
    public Tile pointTile => _pointTile;


    // MonoBehaviour
    private void Start()
    {
        Input_Controller.instance.OnCursorControl += Movement_Update;
    }

    private void OnDestroy()
    {
        Input_Controller.instance.OnCursorControl -= Movement_Update;
    }


    // Pointer
    private void Movement_Update(Vector2 cursorPosition)
    {
        _rect.position = cursorPosition;
    }

    public void Update_PointTile(Tile pointTile)
    {
        _pointTile = pointTile;
    }
}
