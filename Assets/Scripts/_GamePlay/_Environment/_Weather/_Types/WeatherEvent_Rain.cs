using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Rain : WeatherEvent
{
    [Space(20)]
    [SerializeField] private Item_ScrObj[] _excludeItems;


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{temperatureUpdateValue}", playerDataModifier.temperatureUpdateValue.ToString());
    }


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        return TilePatterns_Utility.CheckBoard_Tiles(Random.value < 0.5f);
    }

    private bool Has_ExcludeItem(Tile tile)
    {
        for (int i = 0; i < _excludeItems.Length; i++)
        {
            if (tile.Placed_ItemCount(_excludeItems[i]) > 0) return true;
        }
        return false;
    }
    public override void Activate_Event()
    {
        for (int i = reservedActivationTiles.Count - 1; i >= 0 ; i--)
        {
            if (Has_ExcludeItem(reservedActivationTiles[i]) == false) continue;
            reservedActivationTiles.RemoveAt(i);
        }
        Update_TileStates();
        
        if (ActivationTiles_PlayerDetected() == false) return;

        playerDataModifier.Update_Data();
    }
}