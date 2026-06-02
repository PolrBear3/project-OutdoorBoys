using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Sunlight : WeatherEvent
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _activationRange;


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{activationRange}", _activationRange.ToString())
            .Replace("{temperatureUpdateValue}", playerDataModifier.temperatureUpdateValue.ToString());
    }


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
        Update_TileStates();

        if (ActivationTiles_PlayerDetected() == false) return;
        
        playerDataModifier.Update_Data();
    }
}
