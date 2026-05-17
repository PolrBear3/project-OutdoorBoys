using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Rain : WeatherEvent
{
    [Space(20)]
    [SerializeField] private TileScrObj[] _excludeTiles;

    [Space(10)]
    [SerializeField][Range(0, 10)] private int _temperatureDecreaseValue;


    // Activation
    private bool Is_ExcludeTile(Tile checkTile)
    {
        for (int i = 0; i < _excludeTiles.Length; i++)
        {
            if (checkTile.data.tileScrObj != _excludeTiles[i]) continue;
            return true;
        }
        return false;
    }

    public override List<Tile> Generated_ActivationTiles()
    {
        List<Tile> rainTiles = TilePatterns_Utility.CheckBoard_Tiles(Random.value < 0.5f);

        for (int i = rainTiles.Count - 1; i >= 0; i--)
        {
            if (Is_ExcludeTile(rainTiles[i]) == false) continue;
            rainTiles.RemoveAt(i);
        }
        return rainTiles;
    }

    public override void Activate_Event()
    {
        if (ActivationTiles_PlayerDetected() == false) return;

        Player_Controller player = InGame_Manager.instance.player;
        player.Update_Temperature(player.data.temperature - _temperatureDecreaseValue);
    }


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{temperatureDecreaseValue}", _temperatureDecreaseValue.ToString());
    }
}
