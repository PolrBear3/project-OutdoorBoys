using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_ColdNight : WeatherEvent
{
    public override List<Tile> Event_ActivationTiles()
    {
        return null;
    }

    public override void Activate_Event()
    {
        Debug.Log("Cold Night");
    }
}
