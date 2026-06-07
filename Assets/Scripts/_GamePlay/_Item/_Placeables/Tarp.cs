using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tarp : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField][Range(0, 1)] private float _transparencyValue;
    [SerializeField][Range(0, 10)] private float _transparencyUpdateDuration;


    // MonoBehaviour
    private void Start()
    {
        TileTracker playerTileTracker = InGame_Manager.instance.player.movement.tileTracker;
        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Update_Transparency);

        Update_Transparency(playerTileTracker.data.CurrentTile());
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.player.movement.tileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Update_Transparency);
    }


    // Main
    private void Update_Transparency(Tile playerCurrentTile)
    {
        float transparencyValue = playerCurrentTile == _placeableItem.placedTile ? _transparencyValue : 1f;

        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, transparencyValue, _transparencyUpdateDuration);
    }
}
