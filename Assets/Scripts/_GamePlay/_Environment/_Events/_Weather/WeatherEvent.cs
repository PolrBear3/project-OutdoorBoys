using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeatherEvent : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Tile_Indicator _tileIndicator;
    public Tile_Indicator tileIndicator => _tileIndicator;

    public abstract void Activate_Event();
}