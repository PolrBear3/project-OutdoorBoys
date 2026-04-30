using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Player_Controller _controller;

    [SerializeField] private TileTracker _tileTracker;
    public TileTracker tileTracker => _tileTracker;

    [Space(20)]
    [SerializeField][Range(0, 10)] private float _defaultSpeed;


    private Vector2 _inputDirection;

    private Vector2 _previousPosition;
    private Vector2 _movingPosition;

    private bool _isMoving;
    public bool isMoving => _isMoving;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller.instance.OnMovement -= Update_InputDirection;
        InGame_Manager.instance.time.UnRegister(TimeUpdateBus.AwakeUpdate, _tileTracker.Clamp_toCurrentTile);
    }

    private void Update()
    {
        Movement_Update();
        MovementState_Update();
        MovementAnimation_Update();
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnMovement += Update_InputDirection;
        InGame_Manager.instance.time.Register(TimeUpdateBus.AwakeUpdate, _tileTracker.Clamp_toCurrentTile);
    }


    // Updates
    private void Update_InputDirection(Vector2 direction)
    {
        _inputDirection = direction;
    }

    private void Movement_Update()
    {
        if (_tileTracker.clampCoroutine != null) return;

        if (InGame_Manager.instance.time.timeUpdateActions.Count > 0 || _controller.data.currentStamina <= 0)
        {
            _tileTracker.Clamp_toCurrentTile();
            return;
        }

        transform.Translate(_inputDirection * _defaultSpeed * Time.deltaTime);
        if (_inputDirection == Vector2.zero) return;

        _tileTracker.TrackUpdate_CurrentTile();

        if (_tileTracker.Inside_TileArea()) return;
        _tileTracker.Clamp_toCurrentTile();
    }
    private void MovementState_Update()
    {
        Vector2 currentPosition = transform.position;

        _previousPosition = currentPosition - _previousPosition;
        _isMoving = _previousPosition.sqrMagnitude > 0.000001f;
        _previousPosition = currentPosition;
    }

    private void MovementAnimation_Update()
    {
        AnimationPlayer animPlayer = _controller.animationPlayer;

        int animIndexNum = _isMoving ? 1 : 0;
        if (animPlayer.Animation_Playing(animPlayer.AnimationClip(animIndexNum))) return;

        animPlayer.Play(animIndexNum);
    }
}