using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeatherEvent : MonoBehaviour
{
    private Weather_Manager _manager;

    
    [Space(20)]
    [SerializeField] private TileIndicator_VisualData _activateTileVisuals;
    public TileIndicator_VisualData activateTileVisuals => _activateTileVisuals;

    [Space(10)]
    [SerializeField] private TileState[] _tileStatesToRemove;
    [SerializeField] private TileState[] _tileStatesToAdd;


    private List<Tile> _reservedActivationTiles = new();
    public List<Tile> reservedActivationTiles => _reservedActivationTiles;


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

        foreach (Tile tile in _reservedActivationTiles)
        {
            for (int i = 0; i < _tileStatesToRemove.Length; i++)
            {
                tile.Remove_State(_tileStatesToRemove[i]);
            }
            for (int i = 0; i < _tileStatesToAdd.Length; i++)
            {
                tile.Add_State(_tileStatesToAdd[i]);
            }
        }
    }


    // Text Template
    public virtual string Description()
    {
        return _manager.TargetEvent_Weather(this).descriptionText;
    }
}