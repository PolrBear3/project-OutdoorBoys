using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Sunlight : WeatherEvent
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _activationRange;
    [SerializeField][Range(0, 10)] private int _temperatureIncreaseValue;


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;
        List<Tile> allTiles = new(tilesController.currentTiles);
        
        Tile pivotTile = allTiles[Random.Range(0, allTiles.Count)];
        return TilePatterns_Utility.PivotDistanced_Tiles(pivotTile, _activationRange);
    }

    public override void Activate_Event()
    {
        if (ActivationTiles_PlayerDetected() == false) return;

        Player_Controller player = InGame_Manager.instance.player;
        player.Update_Temperature(player.data.temperature + _temperatureIncreaseValue);
    }


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{activationRange}", _activationRange.ToString())
            .Replace("{temperatureIncreaseValue}", _temperatureIncreaseValue.ToString());
    }
}
