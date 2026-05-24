using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Rain : WeatherEvent
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _temperatureDecreaseValue;


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{temperatureDecreaseValue}", _temperatureDecreaseValue.ToString());
    }


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        return TilePatterns_Utility.CheckBoard_Tiles(Random.value < 0.5f);
    }

    public override void Activate_Event()
    {
        Update_TileStates();
        
        if (ActivationTiles_PlayerDetected() == false) return;

        Player_Controller player = InGame_Manager.instance.player;
        player.Update_Temperature(player.data.temperature - _temperatureDecreaseValue);
    }
}
