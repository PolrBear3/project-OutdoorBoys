using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Controller : MonoBehaviour
{
    [SerializeField] private TileTracker _tileTracker;
    public TileTracker tileTracker => _tileTracker;

    [Space(20)]
    [SerializeField][Range(0, 10)] private float _defaultSpeed;

    private float _currentSpeed;

    private Coroutine _moveCoroutine;
    public Coroutine moveCoroutine => _moveCoroutine;

    private Vector2 _destination;

    public Action<bool> OnMovementState;
    public Action<Vector2> OnMovementDirection;


    // MonoBehaviour
    private void Awake()
    {
        _currentSpeed = _defaultSpeed;
    }

    private void Start()
    {
        InGame_Manager.instance.movements.movementControllers.Add(this);
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.movements.movementControllers.Remove(this);
    }

    private void Update()
    {
        Movement_Update();
    }


    // Move Update
    private void Movement_Update()
    {
        if (_moveCoroutine == null) return;

        transform.position = Vector2.MoveTowards(transform.position, _destination, _currentSpeed * Time.deltaTime);

        if (_tileTracker == null) return;
        _tileTracker.TrackUpdate_CurrentTile();
    }

    public void Update_CurrentSpeed(float updateValue)
    {
        _currentSpeed = Mathf.Max(0.01f, updateValue);
    }


    public bool At_Destination()
    {
        return Vector2.Distance(transform.position, _destination) <= 0.01f;
    }
    private IEnumerator MovementState_Update()
    {
        while (At_Destination() == false) yield return null;
        Stop();
    }


    public void Stop()
    {
        OnMovementState?.Invoke(false);

        if (_moveCoroutine == null) return;

        StopCoroutine(_moveCoroutine);
        _moveCoroutine = null;
    }

    public void Move(Vector2 customPosition)
    {
        _destination = customPosition;

        OnMovementState?.Invoke(true);
        OnMovementDirection?.Invoke((_destination - (Vector2)transform.position).normalized);

        _moveCoroutine = StartCoroutine(MovementState_Update());
    }
    public void Move(Tile targetTile)
    {
        Move(targetTile.transform.position);
    }
}
