using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGame_Manager : MonoBehaviour
{
    public static InGame_Manager instance;
    
    
    [Space(20)]
    [SerializeField] private Tile_Generator _tileGenerator;
    public Tile_Generator tileGenerator => _tileGenerator;

    [SerializeField] private Cursor _cursor;
    public Cursor cursor => _cursor;


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
