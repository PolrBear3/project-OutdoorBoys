using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private LeanTweenType _moveTweenType;

    [SerializeField][Range(0, 10)] private float _moveDuration;
    public float moveDuration => _moveDuration;


    private LeanTweenType _currentMoveTweenType;

    private Tile _currentTile;
    public Tile currentTile => _currentTile;

    public Action<int> OnMovement;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
    }


    // Data
    private void Set_Data()
    {
        Update_MoveTweenType(_moveTweenType);
    }

    public void Update_MoveTweenType(LeanTweenType tweenType)
    {
        _currentMoveTweenType = tweenType;
    }


    // Movement
    public void MoveTo_Tile(Tile destinationTile)
    {
        if (destinationTile == null) return;
        if (LeanTween.isTweening(gameObject)) return;

        Tile previousTile = _currentTile;
        Vector2 destination = destinationTile.setPosition.position;

        _currentTile = destinationTile;
        _currentTile.Toggle_Transparency(true);

        if (previousTile == null)
        {
            transform.position = destination; // spawn
            return;
        }
        previousTile.Toggle_Transparency(false);

        int moveDistance = Mathf.RoundToInt(Vector2.Distance(destination, previousTile.transform.position));
        OnMovement?.Invoke(moveDistance);

        LeanTween.move(gameObject, destination, _moveDuration).setEase(_currentMoveTweenType); // move
    }

    public void MoveTo_Tile(Vector2 direction)
    {
        Tiles_Controller controller = InGame_Manager.instance.tilesController;
        Tile destinationTile = controller.Current_Tile((Vector2)_currentTile.transform.position + direction);

        if (destinationTile == null) return;
        MoveTo_Tile(destinationTile);
    }
}
