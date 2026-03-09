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

    private Vector2 _currentOffset;
    private float _currentMoveDuration;

    public Action OnMovement;
    public Action<int> OnMovementDistanced;
    public Action<bool> OnMovementStated;

    private Coroutine _movementCoroutine;
    public Coroutine movementCoroutine => _movementCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        Set_Data();

        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.movements.allMovementControllers.Remove(this);

        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
    }


    // Data
    private void Set_Data()
    {
        if (InGame_Manager.instance?.movements.allMovementControllers.Add(this) == false) return;

        Update_Offset();
        Update_MoveDurationValue();
    }


    public void Update_Offset(Vector2 offSet)
    {
        _currentOffset = offSet;
    }
    /// <summary>
    /// Resets to default offset
    /// </summary>
    public void Update_Offset()
    {
        Update_Offset(_offset);
    }

    public Vector2 CurrentTile_OffsetPosition()
    {
        return (Vector2)_currentTile.setPosition.position + _currentOffset;
    }


    public void Update_MoveDurationValue(float value)
    {
        _currentMoveDuration = Mathf.Max(0f, value);
    }
    public void Update_MoveDurationValue()
    {
        Update_MoveDurationValue(_moveDuration);
    }


    // Movement
    public void MoveTo_Tile(Tile destinationTile)
    {
        if (destinationTile == null) return;
        if (LeanTween.isTweening(gameObject)) return;

        Tile previousTile = _currentTile;
        _currentTile = destinationTile;

        Vector2 destination = CurrentTile_OffsetPosition();

        if (previousTile == null)
        {
            transform.position = destination; // spawn
            return;
        }

        int moveDistance = Utility.Chebyshev_Distance(previousTile.transform.position, _currentTile.transform.position);

        OnMovement?.Invoke();
        OnMovementDistanced?.Invoke(moveDistance);
        Start_MovementStateUpdate(moveDistance);

        LeanTween.move(gameObject, destination, _currentMoveDuration * moveDistance); // move
    }
    public void MoveTo_Tile(Vector2 direction)
    {
        InGame_Manager manager = InGame_Manager.instance;
        if (manager.movements.AlllMovements_Complete() == false) return;

        Tiles_Controller controller = manager.tilesController;
        Tile destinationTile = controller.Current_Tile((Vector2)_currentTile.transform.position + direction);

        if (destinationTile == null) return;
        MoveTo_Tile(destinationTile);
    }

    private void Start_MovementStateUpdate(float moveDistance)
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
        _movementCoroutine = StartCoroutine(MovementState_Update(moveDistance));
    }
    private IEnumerator MovementState_Update(float moveDistance)
    {
        OnMovementStated?.Invoke(true);

        yield return new WaitForSeconds(_currentMoveDuration * moveDistance);
        OnMovementStated?.Invoke(false);

        _movementCoroutine = null;
    }
}
