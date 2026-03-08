using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Movement_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Vector2 _offset;

    [SerializeField][Range(0, 10)] private float _moveDuration;
    public float moveDuration => _moveDuration;


    private Tile _currentTile;
    public Tile currentTile => _currentTile;

    private float _currentMoveDuration;

    public Action OnMovement;
    public Action<int> OnMovementDistanced;
    public Action<bool> OnMovementStated;

    private Coroutine _movementCoroutine;
    public Coroutine movementCoroutine => _movementCoroutine;


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
        Update_MoveDurationValue(0);
    }

    public void Update_MoveDurationValue(float value)
    {
        value = value <= 0 ? _moveDuration : value;
        _currentMoveDuration = value;
    }


    // Movement
    public void MoveTo_Tile(Tile destinationTile)
    {
        if (destinationTile == null) return;
        if (LeanTween.isTweening(gameObject)) return;

        Tile previousTile = _currentTile;
        _currentTile = destinationTile;

        Vector2 destination = (Vector2)destinationTile.setPosition.position + _offset;

        if (previousTile == null)
        {
            transform.position = destination; // spawn
            return;
        }

        OnMovement?.Invoke();

        int moveDistance = Mathf.RoundToInt(Vector2.Distance(destination, previousTile.transform.position));
        OnMovementDistanced?.Invoke(moveDistance);

        Start_MovementStateUpdate();

        LeanTween.move(gameObject, destination, _currentMoveDuration); // move
    }
    public void MoveTo_Tile(Vector2 direction)
    {
        Tiles_Controller controller = InGame_Manager.instance.tilesController;
        Tile destinationTile = controller.Current_Tile((Vector2)_currentTile.transform.position + direction);

        if (destinationTile == null) return;
        MoveTo_Tile(destinationTile);
    }

    private void Start_MovementStateUpdate()
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
        _movementCoroutine = StartCoroutine(MovementState_Update());
    }
    private IEnumerator MovementState_Update()
    {
        OnMovementStated(true);

        yield return new WaitForSeconds(_currentMoveDuration);
        OnMovementStated(false);

        _movementCoroutine = null;
    }
}
