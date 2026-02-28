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
        InGame_Manager.instance.player.movement.OnMovement += Update_Transparency;
    }

    private void Start()
    {
        Update_Transparency();
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.player.movement.OnMovement -= Update_Transparency;
    }


    // Main
    private void Update_Transparency()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
        float transparencyValue = playerTile == _placeableItem.currentTile ? _transparencyValue : 1f;

        LeanTween.cancel(gameObject);
        LeanTween.alpha(gameObject, transparencyValue, _transparencyUpdateDuration);
    }
}
