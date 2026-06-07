using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Storm : WeatherEvent
{
    [Space(20)]
    [SerializeField] private Item_ScrObj[] _protectItems;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _positionUpdateCount;
    [SerializeField] private Item_ScrObj[] _positionUpdateItems;

    [Space(20)]
    [SerializeField][Range(0, 1)] private float _itemDropRate;
    [SerializeField] private ConvertUpdate_ItemData[] _dropItemDatas;


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{temperatureUpdateValue}", playerDataModifier.temperatureUpdateValue.ToString());
    }


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        return new(InGame_Manager.instance.tilesController.currentTiles);
    }

    public override void Activate_Event()
    {
        Update_TileStates();
        Update_PlayerData();

        PositionUpdate_PlacedItems();
        DropUpdate_PlacedItems();

        RemoveUpdate_ProtectItems();
    }


    // Custom Activations
    private void Update_PlayerData()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();

        for (int i = 0; i < _protectItems.Length; i++)
        {
            if (playerTile.Placed_ItemCount(_protectItems[i]) > 0) return;
        }
        playerDataModifier.Update_Data();
    }


    private bool Is_PositionUpdateItem(Item_ScrObj checkItem)
    {
        for (int i = 0; i < _positionUpdateItems.Length; i++)
        {
            if (checkItem == _positionUpdateItems[i]) return true;
        }
        return false;
    }
    private List<PlaceableItem> PositionUpdate_PlacedItems(Vector2 updateDirection)
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        List<Tile> activationTiles = new(reservedActivationTiles);
        List<PlaceableItem> updateItems = new();

        while (updateItems.Count < _positionUpdateCount && activationTiles.Count > 0)
        {
            Tile randomTile = activationTiles[Random.Range(0, activationTiles.Count)];
            Tile updateTile = tilesController.Current_Tile((Vector2)randomTile.transform.position + updateDirection);

            List<PlaceableItem> placedItems = new(randomTile.PlacedItems());

            if (placedItems.Count <= 0 || updateTile == null)
            {
                activationTiles.Remove(randomTile);
                continue;
            }
            for (int i = placedItems.Count - 1; i >= 0; i--)
            {
                PlaceableItem placedItem = placedItems[i];

                if (updateItems.Contains(placedItem))
                {
                    placedItems.RemoveAt(i);
                    continue;
                }

                ItemData placedItemData = placedItem.data;

                if (Is_PositionUpdateItem(placedItemData.itemScrObj) == false) continue;
                if (updateTile.ItemPlace_AvailableCount(placedItemData) <= 0) continue;

                updateItems.Add(placedItem);
                break;
            }

            if (placedItems.Count > 0) continue;
            activationTiles.Remove(randomTile);

        }
        return updateItems;
    }

    private void PositionUpdate_PlacedItems()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        List<Vector2> allDirections = Utility.Surrounding_Directions();
        Vector2 updateDirection = allDirections[Random.Range(0, allDirections.Count - 1)];

        List<PlaceableItem> updateItems = PositionUpdate_PlacedItems(updateDirection);

        for (int i = 0; i < updateItems.Count; i++)
        {
            Tile updateItemTile = updateItems[i].currentTile;
            Tile updateTile = tilesController.Current_Tile((Vector2)updateItemTile.transform.position + updateDirection);

            Debug.Log(updateItems[i] + " " + updateItemTile.gameObject + updateTile.gameObject);
        }

        /*
        activationTile.Remove_PlacedItemData(placeableItem);

        Tile updateTile = tilesController.Current_Tile((Vector2)activationTile.transform.position + updateDirection);
        if (updateTile == null)
        {
            Destroy(placeableItem.gameObject);
            continue;
        }
        updateTile.Set_Item(placedData);
        Destroy(placeableItem.gameObject);
        */
    }


    private ItemData DropUpdate_ItemData(Item_ScrObj droppingItem)
    {
        for (int i = 0; i < _dropItemDatas.Length; i++)
        {
            if (droppingItem != _dropItemDatas[i].preUpdateItem) continue;
            return _dropItemDatas[i].Converted_ItemData();
        }
        return null;
    }
    private void DropUpdate_PlacedItems()
    {
        for (int i = 0; i < reservedActivationTiles.Count; i++)
        {
            Tile activationTile = reservedActivationTiles[i];
            List<PlaceableItem> placedItems = activationTile.PlacedItems();

            for (int j = placedItems.Count - 1; j >= 0; j--)
            {
                PlaceableItem placeableItem = placedItems[j];
                ItemData placedData = placeableItem.data;

                Item_ScrObj placedItem = placedData.itemScrObj;
                int placedAmount = placedData.amount;

                float amountRatio = Mathf.Clamp01((float)placedAmount / placedItem.maxAmount);
                float dropRate = Mathf.Lerp(0f, _itemDropRate, amountRatio);

                if (Random.value >= dropRate) continue;

                ItemData dropData = DropUpdate_ItemData(placedData.itemScrObj);
                if (dropData == null) continue;

                int dropAmount = Random.Range(1, dropData.amount);
                activationTile.Set_PlacingItem(new(dropData.itemScrObj, dropAmount));
            }
        }
    }

    private void RemoveUpdate_ProtectItems()
    {
        for (int i = 0; i < reservedActivationTiles.Count; i++)
        {
            Tile activationTile = reservedActivationTiles[i];

            for (int j = 0; j < _protectItems.Length; j++)
            {
                List<PlaceableItem> placedProtectItems = activationTile.PlacedItems(_protectItems[j]);
                int placedCount = placedProtectItems.Count;

                if (placedCount <= 0) continue;
                PlaceableItem placedProtectItem = placedProtectItems[placedCount - 1];

                activationTile.Remove_PlacedItemData(placedProtectItem);
                Destroy(placedProtectItem.gameObject);

                break;
            }
        }
    }
}