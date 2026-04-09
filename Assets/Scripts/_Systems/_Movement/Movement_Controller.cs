using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum MovementState { stunned, knockback }

public class Movement_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Vector2 _offset;
    public Vector2 offset => _offset;

    [SerializeField][Range(0, 1)] private float _moveDuration;
    public float moveDuration => _moveDuration;


    private TileTrackerData _tileTrackerData;
    public TileTrackerData tileTrackerData => _tileTrackerData;

    private Vector2 _currentOffset;
    
    private float _currentMoveDuration;
    public float currentMoveDuration => _currentMoveDuration;

    private Dictionary<MovementState, int> _currentStateDatas = new();


    public Action OnMovement;
    public Action<Vector2> OnMovementDirection;
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
        _tileTrackerData = new();

        if (InGame_Manager.instance?.movements.allMovementControllers.Add(this) == false) return;

        Update_Offset();
        Update_MoveDurationValue();
    }


    public Vector2 CurrentTile_OffsetPosition()
    {
        return (Vector2)_tileTrackerData.CurrentTile().setPosition.position + _currentOffset;
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


    public int CurrentState_Count(MovementState checkState)
    {
        return _currentStateDatas.TryGetValue(checkState, out int count) ? count : 0;
    }

    public void Update_CurrentState(MovementState updateState, int updateCount)
    {
        if (updateCount <= 0)
        {
            _currentStateDatas.Remove(updateState);
            return;
        }
        _currentStateDatas[updateState] = updateCount;
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

        Tile previousTile = _tileTrackerData.CurrentTile();
        _tileTrackerData.TrackTile(destinationTile);

        Vector2 destination = CurrentTile_OffsetPosition();

        if (previousTile == null)
        {
            transform.position = destination; // spawn
            transform.SetParent(destinationTile.setPosition);
            return;
        }

        OnMovement?.Invoke();

        Tile currentTile = _tileTrackerData.CurrentTile();
        transform.SetParent(currentTile.setPosition);

        Vector2 previousTilePos = previousTile.transform.position;
        Vector2 destinationTilePos = currentTile.transform.position;

        Vector2 direction = destinationTilePos - previousTilePos;
        OnMovementDirection?.Invoke(direction);
        
        int moveDistance = Utility.Chebyshev_Distance(previousTilePos, destinationTilePos);
        OnMovementDistanced?.Invoke(moveDistance);

        Start_MovementStateUpdate(moveDistance);

        LeanTween.move(gameObject, destination, _currentMoveDuration * moveDistance); // move
    }
    public void MoveTo_Tile(Vector2 direction)
    {
        InGame_Manager manager = InGame_Manager.instance;
        if (manager.movements.AllMovements_Complete() == false) return;

        Tiles_Controller controller = manager.tilesController;
        Tile destinationTile = controller.Current_Tile((Vector2)_tileTrackerData.CurrentTile().transform.position + direction);

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
