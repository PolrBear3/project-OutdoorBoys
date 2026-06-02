using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Storm : WeatherEvent
{
    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{temperatureUpdateValue}", playerDataModifier.temperatureUpdateValue.ToString());
    }


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        return new(InGame_Manager.instance.tilesController.currentTiles);
    }

    public override void Activate_Event()
    {
        playerDataModifier.Update_Data();
    }
}
