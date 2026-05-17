using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_ColdNight : WeatherEvent
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _temperatureDecreaseValue;


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        InGame_Manager manager = InGame_Manager.instance;
        List<Tile> allTiles = new(manager.tilesController.currentTiles);
        
        Time_Manager time = manager.time;

        int totalNightCount = time.Total_NightTimeCount();
        int nightRunCount = time.Current_NightTimeCount();

        if (nightRunCount >= totalNightCount) return allTiles;

        int tileCount = allTiles.Count / totalNightCount * nightRunCount;
        List<Tile> activateTiles = new();

        while (tileCount > 0 && allTiles.Count > 0)
        {
            tileCount--;
            int randIndex = Random.Range(0, allTiles.Count);

            activateTiles.Add(allTiles[randIndex]);
            allTiles.RemoveAt(randIndex);
        }
        return activateTiles;
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
