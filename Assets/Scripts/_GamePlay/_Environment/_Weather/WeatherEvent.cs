using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeatherEvent : MonoBehaviour
{
    private Weather_Manager _manager;

    [SerializeField] private PlayerData_Modifier _playerDataModifier;
    public PlayerData_Modifier playerDataModifier => _playerDataModifier;

    private List<Tile> _reservedActivationTiles = new();
    public List<Tile> reservedActivationTiles => _reservedActivationTiles;

    private int _persistTimeCount;
    public int persistTimeCount => _persistTimeCount;


    // Data
    public void Set_Manager(Weather_Manager manager)
    {
        _manager = manager;
    }


    // Activation
    public abstract List<Tile> Generated_ActivationTiles();
    public void Reserve_ActivationTiles()
    {
        _reservedActivationTiles = Generated_ActivationTiles() ?? new();
    }

    public abstract void Activate_Event();


    // Custom Activations
    public bool ActivationTiles_PlayerDetected()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();

        return _reservedActivationTiles.Contains(playerTile);
    }

    public void Update_TileStates()
    {
        if (_reservedActivationTiles.Count <= 0) return;

        Weather_ScrObj weatherData = _manager.TargetEvent_Weather(this);
        if (weatherData == null) return;

        TileState[] statesToRemove = weatherData.tileStatesToRemove;
        TileState[] statesToAdd = weatherData.tileStatesToAdd;

        foreach (Tile tile in _reservedActivationTiles)
        {
            for (int i = 0; i < statesToRemove.Length; i++)
            {
                tile.Remove_State(statesToRemove[i]);
            }
            for (int i = 0; i < statesToAdd.Length; i++)
            {
                tile.Add_State(statesToAdd[i]);
            }
        }
    }


    // Text Template
    public virtual string Description()
    {
        return _manager.TargetEvent_Weather(this).descriptionText;
    }
}