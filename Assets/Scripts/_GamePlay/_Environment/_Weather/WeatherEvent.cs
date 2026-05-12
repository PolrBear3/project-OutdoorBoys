using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeatherEvent : MonoBehaviour
{
    public abstract List<Tile> Event_ActivationTiles();
    
    public abstract void Activate_Event();
}