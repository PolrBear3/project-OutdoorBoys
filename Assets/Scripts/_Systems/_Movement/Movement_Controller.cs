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

    private Coroutine _moveCoroutine;
    public Coroutine moveCoroutine => _moveCoroutine;

    private Vector2 _destination;

    public Action<bool> OnMovementState;
    public Action<Vector2> OnMovementDirection;


    // MonoBehaviour
    private void Awake()
    {

    }

    private void OnDestroy()
    {

    }

    private void Update()
    {
        Movement_Update();
    }


    // Move Update
    private void Movement_Update()
    {
        if (_moveCoroutine == null) return;

        transform.position = Vector2.MoveTowards(transform.position, _destination, _defaultSpeed * Time.deltaTime);

        _tileTracker.TrackUpdate_CurrentTile();
    }

    public void Stop()
    {
        OnMovementState?.Invoke(false);

        if (_moveCoroutine == null) return;

        StopCoroutine(_moveCoroutine);
        _moveCoroutine = null;
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
