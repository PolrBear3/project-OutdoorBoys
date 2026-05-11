using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Rain : WeatherEvent
{
    [SerializeField] private TileScrObj[] _excludeTiles;


    public override void Update_EventPreview()
    {
        List<Tile> rainTiles = TilePatterns_Utility.CheckBoard_Tiles(true);

        foreach (Tile tile in rainTiles)
        {
            bool tileExcluded = false;

            for (int i = 0; i < _excludeTiles.Length; i++)
            {
                if (tile.data.tileScrObj != _excludeTiles[i]) continue;

                tileExcluded = true;
                break;
            }

            if (tileExcluded) continue;
            tileIndicator.Set_Indicator(tile);
        }

        Toggle_TileIndicator(false);
    }

    public override void Activate_Event()
    {
        Debug.Log("Rain");
    }
}
