using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_StrongWind : WeatherEvent
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _pushDistanceValue;
    public int pushDistanceValue => _pushDistanceValue;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _pushItemsCount;
    [SerializeField][Range(0, 10)] private float _pushDuration;

    [Space(20)]
    [SerializeField] private Item_ScrObj[] _pushRestrictedItems;

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

        return new(InGame_Manager.instance.tilesController.currentTiles);
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
    private Tile Pushed_UpdateTile(Tile currentTile)
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        for (int i = 0; i < _pushDistanceValue; i++)
        {
            Vector2 distancedDirection = _pushDirection * Mathf.Max(1, i);
            Tile updateTile = tilesController.Current_Tile((Vector2)currentTile.transform.position + distancedDirection);

            if (updateTile == null) break;
            currentTile = updateTile;
        }
        return currentTile;
    }

    private void PushUpdate_Items()
    {
        List<PlaceableItem> allPlacedItems = new(InGame_Manager.instance.tilesController.placedItems);

        for (int i = allPlacedItems.Count - 1; i >= 0; i--)
        {
            Item_ScrObj placedItem = allPlacedItems[i].data.itemScrObj;

            for (int j = 0; j < _pushRestrictedItems.Length; j++)
            {
                if (placedItem != _pushRestrictedItems[j]) continue;

                allPlacedItems.RemoveAt(i);
                break;
            }
        }

        for (int i = 0; i < _pushItemsCount; i++)
        {
            if (allPlacedItems.Count <= 0) break;

            PlaceableItem randPlacedItem = allPlacedItems[Random.Range(0, allPlacedItems.Count)];
            allPlacedItems.Remove(randPlacedItem);

            Tile updateTile = Pushed_UpdateTile(randPlacedItem.placedTile);
            if (updateTile.ItemPlace_AvailableCount(randPlacedItem.data.itemScrObj) <= 0) continue;

            randPlacedItem.placedTile.Remove_PlacedItemData(randPlacedItem);

            updateTile.Track_PlacingItem(randPlacedItem);
            randPlacedItem.Track_CurrentTile(updateTile);

            updateTile.ClampUpdate_PlacedItemOffsets(_pushDuration);
        }
    }

    private void PushUpdate_Player()
    {
        TileTracker playerTracker = InGame_Manager.instance.player.movement.tileTracker;

        playerTracker.TrackUpdate_CurrentTile(Pushed_UpdateTile(playerTracker.data.CurrentTile()));
        playerTracker.Clamp_toCurrentTile();
    }
}
