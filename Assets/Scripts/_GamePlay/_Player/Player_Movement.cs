using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [SerializeField] private Player_Controller _controller;

    [Space(20)]
    [SerializeField][Range(0, 10)]  private float _moveSpeed;

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
        animPlayer.Play(1);
        
        yield return new WaitForSeconds(_moveSpeed);

        animPlayer.Play(0);
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

        InGame_Manager manager = InGame_Manager.instance;

        ItemData currentItem = manager.cursor.itemCursor.itemData;
        bool hasInventoryBagpack = currentItem != null && currentItem.itemScrObj == _controller.inventoryBagpack;

        int distanceToTile = Mathf.RoundToInt(Vector2.Distance(destination, previousTile.transform.position));
        
        int currentInventoryWeight = hasInventoryBagpack ? manager.inventory.Total_ItemWeight() : 0;
        int currentItemWeight = currentItem != null ? currentItem.Item_Weight() + currentInventoryWeight : 0;

        manager.time.Update_Data(distanceToTile + currentItemWeight * distanceToTile);

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
