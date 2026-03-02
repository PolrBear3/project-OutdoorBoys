using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [SerializeField] private Player_Controller _controller;

    [Space(20)]
    [SerializeField][Range(0, 10)]  private float _moveSpeed;
    [SerializeField][Range(0, 100)] private int _moveTimeCost;

    private Tile _currentTile;
    public Tile currentTile => _currentTile;

    public Action OnMovement;

    private Coroutine _moveAnimationCoroutine;
    
    
    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller.instance.OnMovement -= MoveTo_Tile;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnMovement += MoveTo_Tile;
    }


    // Movement
    private IEnumerator PlayAnimation_Movement()
    {
        AnimationPlayer animPlayer = _controller.animationPlayer;
        animPlayer.Play("Player_Move");
        
        yield return new WaitForSeconds(_moveSpeed);

        animPlayer.Play("Player_Idle");
        yield break;
    }

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
            transform.position = destination;
            return;
        }
        previousTile.Toggle_Transparency(false);

        int distance = Mathf.RoundToInt(Vector2.Distance(destination, previousTile.transform.position));
        InGame_Manager.instance.time.Update_Data(distance * _moveTimeCost);

        if (_moveAnimationCoroutine != null)
        {
            StopCoroutine(_moveAnimationCoroutine);
            _moveAnimationCoroutine = null;
        }
        _moveAnimationCoroutine = StartCoroutine(PlayAnimation_Movement());
        
        LeanTween.move(gameObject, destination, _moveSpeed);
    }

    private void MoveTo_Tile(Vector2 direction)
    {
        Tiles_Controller controller = InGame_Manager.instance.tilesController;
        Tile destinationTile = controller.Current_Tile((Vector2)_currentTile.transform.position + direction);
        
        if (destinationTile == null) return;
        
        MoveTo_Tile(destinationTile);
        OnMovement?.Invoke();
    }
}
