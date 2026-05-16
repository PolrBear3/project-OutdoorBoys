using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeatherEvent : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private TileIndicator_VisualData _activateTileVisuals;
    public TileIndicator_VisualData activateTileVisuals => _activateTileVisuals;

    private List<Tile> _reservedActivationTiles = new();
    public List<Tile> reservedActivationTiles => _reservedActivationTiles;


    public abstract List<Tile> Generated_ActivationTiles();

    public void Reserve_ActivationTiles()
    {
        _reservedActivationTiles = Generated_ActivationTiles() ?? new();
    }

    public abstract void Activate_Event();
}