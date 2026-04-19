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
    private void Awake()
    {
        InGame_Manager.instance.player.movement.tileTracker.OnTileTrackUpdate += Update_Transparency;
    }

    private void Start()
    {
        Update_Transparency(InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile());
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.player.movement.tileTracker.OnTileTrackUpdate -= Update_Transparency;
    }


    // Main
    private void Update_Transparency(Tile playerCurrentTile)
    {
        float transparencyValue = playerCurrentTile == _placeableItem.currentTile ? _transparencyValue : 1f;

        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, transparencyValue, _transparencyUpdateDuration);
    }
}
