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
        _placeableItem.placedTile.OnSetPrefab += Update_Transparency;

        TileTracker playerTileTracker = InGame_Manager.instance.player.movement.tileTracker;
        playerTileTracker.Register(ActionUpdateBus.AwakeUpdate, Update_Transparency);

        Update_Transparency(playerTileTracker.data.CurrentTile());
    }

    private void OnDestroy()
    {
        _placeableItem.placedTile.OnSetPrefab -= Update_Transparency;

        TileTracker playerTileTracker = InGame_Manager.instance.player.movement.tileTracker;
        playerTileTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Update_Transparency);
    }


    // Visuals
    private void Update_Transparency(Tile playerCurrentTile)
    {
        SpriteRenderer renderer = _placeableItem.animPlayer.spriteRenderer;
        float transparencyValue = playerCurrentTile == _placeableItem.placedTile ? _transparencyValue : 1f;

        Color color = renderer.color;
        color.a = Mathf.Clamp01(transparencyValue);

        renderer.color = color;
    }
    private void Update_Transparency(GameObject _)
    {
        Update_Transparency(InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile());
    }
}
