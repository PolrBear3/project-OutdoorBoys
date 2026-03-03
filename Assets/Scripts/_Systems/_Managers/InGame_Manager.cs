using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame_Manager : MonoBehaviour
{
    public static InGame_Manager instance;


    [Space(20)]
    [SerializeField] private InGameUI_Manager _ingameUI;
    public InGameUI_Manager ingameUI => _ingameUI;


    [Space(20)]
    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;
    
    [SerializeField] private Time_Manager _time;
    public Time_Manager time => _time;

    [SerializeField] private Inventory_Manager _inventory;
    public Inventory_Manager inventory => _inventory;


    [Space(20)]
    [SerializeField] private Tile_Generator _tileGenerator;
    public Tile_Generator tileGenerator => _tileGenerator;

    [SerializeField] private Tiles_Controller _tilesController;
    public Tiles_Controller tilesController => _tilesController;


    [Space(20)]
    [SerializeField] private Player_Controller _player;
    public Player_Controller player => _player;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventBus_Manager.Run_BusEvents();
    }
}
