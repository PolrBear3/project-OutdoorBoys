using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_StrongWind : WeatherEvent
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _pushDistanceValue;
    public int pushDistanceValue => _pushDistanceValue;

    [SerializeField][Range(0, 100)] private int _pushItemsCount;
    public int pushItemsCount => _pushItemsCount;

    [Space(10)]
    [SerializeField] private Item_ScrObj[] _pushItems;
    public Item_ScrObj[] pushItems => _pushItems;

    private Vector2 _pushDirection;


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{pushDirection}", _pushDirection.ToString())
            .Replace("{pushDistanceValue}", _pushDistanceValue.ToString());
    }


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        Load_PushDirection();

        return new();
    }

    public override void Activate_Event()
    {
        PushUpdate_Items();
        PushUpdate_Player();
    }


    // Custom Activations
    private void Load_PushDirection()
    {
        List<Vector2> allDirections = Utility.Surrounding_Directions();
        _pushDirection = allDirections[Random.Range(0, allDirections.Count)];
    }

    private void PushUpdate_Player()
    {

    }

    private void PushUpdate_Items()
    {

    }
}
