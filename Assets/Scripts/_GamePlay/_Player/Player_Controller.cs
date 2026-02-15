using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animationPlayer;
    public AnimationPlayer animationPlayer => _animationPlayer;

    [SerializeField] private Player_Movement _movement;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_Position);
    }

    void Start()
    {
        _animationPlayer.Play("Player_Idle");
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_Position);
    }


    // Game Load
    private void Set_Position()
    {
        Tile setTile = InGame_Manager.instance.tilesController.Current_Tile(TileType.softGround);
        _movement.MoveTo_Tile(setTile);
    }
}