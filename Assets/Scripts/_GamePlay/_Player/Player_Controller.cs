using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animationPlayer;
    public AnimationPlayer animationPlayer => _animationPlayer;

    [SerializeField] private Player_Movement _movement;
    public Player_Movement movement => _movement;

    [SerializeField] private Player_Interaction _interaction;
    public Player_Interaction interaction => _interaction;


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
        // Tile setTile = InGame_Manager.instance.tilesController.Current_Tile(TileType.softGround);
        Tile setTile = InGame_Manager.instance.tilesController.currentTiles[1];

        if (setTile == null)
        {
            Debug.Log("Random softGround Tile not Found!");
            return;
        }

        _movement.MoveTo_Tile(setTile);
    }
}