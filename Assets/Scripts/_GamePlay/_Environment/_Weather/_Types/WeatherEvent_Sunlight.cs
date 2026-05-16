using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Sunlight : WeatherEvent
{
    public override List<Tile> Generated_ActivationTiles()
    {
        return TilePatterns_Utility.CheckBoard_Tiles(true);
    }

    public override void Activate_Event()
    {
        Debug.Log("Sunlight");
    }
}
