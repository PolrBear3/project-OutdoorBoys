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

    public Action<bool> OnMovementState;
    public Action<Vector2> OnMovementDirection;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller.instance.OnMovement -= Update_InputDirection;
        OnMovementState -= Update_MovementAnimation;
        OnMovementDirection -= _controller.animationPlayer.Update_Flip;
    }

    private void Update()
    {
        Movement_Update();
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnMovement += Update_InputDirection;
        OnMovementState += Update_MovementAnimation;
        OnMovementDirection += _controller.animationPlayer.Update_Flip;
    }


    // Updates
    private void Update_InputDirection(Vector2 direction)
    {
        _inputDirection = direction;

        OnMovementState?.Invoke(_inputDirection != Vector2.zero);
        OnMovementDirection?.Invoke(direction);
    }
    private void Movement_Update()
    {
        if (_tileTracker.clampCoroutine != null) return;

        if (InGame_Manager.instance.time.timeUpdateActions.Count > 0)
        {
            _tileTracker.ClampInside_CurrentTile();
            return;
        }

        transform.Translate(_inputDirection * _defaultSpeed * Time.deltaTime);
        if (_inputDirection == Vector2.zero) return;

        _tileTracker.TrackUpdate_CurrentTile();

        if (_tileTracker.Inside_TileArea()) return;
        _tileTracker.ClampInside_CurrentTile();
    }

    private void Update_MovementAnimation(bool isMoving)
    {
        AnimationPlayer animPlayer = _controller.animationPlayer;
        int animIndexNum = isMoving && InGame_Manager.instance.time.timeUpdateActions.Count <= 0 ? 1 : 0;

        if (animPlayer.Animation_Playing(animPlayer.AnimationClip(animIndexNum))) return;
        animPlayer.Play(animIndexNum);
    }
}
