using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTemperature_SustainItem : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _sustainValue;


    // MonoBehaviour
    private void Start()
    {
        TileTracker playerTileTracker = InGame_Manager.instance.player.movement.tileTracker;
        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Update_SustainState);

        Update_SustainState(playerTileTracker.data.CurrentTile());
    }

    private void OnDestroy()
    {
        Player_Controller player = InGame_Manager.instance.player;
        player.movement.tileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Update_SustainState);

        player.UnRegister_TemperatureSustainData(gameObject);
    }


    // Main
    private void Update_SustainState(Tile playerTile)
    {
        Player_Controller player = InGame_Manager.instance.player;

        if (playerTile != _placeableItem.placedTile)
        {
            player.UnRegister_TemperatureSustainData(gameObject);
            return;
        }
        player.Register_TemperatureSustainData(gameObject, new(_placeableItem.data.itemScrObj, _sustainValue));
    }
}
