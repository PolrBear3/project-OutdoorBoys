using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTile : MonoBehaviour
{
    [SerializeField] private Tile _tile;
    
    [Space(20)]
    [SerializeField] private PlayerData_Modifier _playerDataModifier;

    
    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.tileTracker.Register(ActionUpdateBus.AwakeUpdate, Update_PlayerTemperature);
        manager.time.Register(ActionUpdateBus.AwakeUpdate, Update_PlayerTemperature);
    }
    
    private void OnDestroy()
    {
        InGame_Manager manager = InGame_Manager.instance;

        manager.player.movement.tileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Update_PlayerTemperature);
        manager.time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_PlayerTemperature);
    }


    // Main
    private void Update_PlayerTemperature(Tile playerTile)
    {
        if (playerTile != _tile) return;

        _playerDataModifier.Update_Data();
    }
    private void Update_PlayerTemperature()
    {
        Update_PlayerTemperature(InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile());
    }
}
