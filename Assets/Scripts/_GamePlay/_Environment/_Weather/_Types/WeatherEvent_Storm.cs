using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEvent_Storm : WeatherEvent
{
    [SerializeField] private PlayerData_Modifier _passivePlayerDataModifier;

    [Space(20)]
    [SerializeField] private AnimationClipScrObj _itemRemoveAnimationclip;
    [SerializeField] private LeanTweenType _itemRemoveTweenType;

    [Space(10)]
    [SerializeField][Range(0, 10)] private float _updatesDuration;

    [Space(20)]
    [SerializeField] private Item_ScrObj[] _protectItems;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _positionUpdateCount;
    [SerializeField] private Item_ScrObj[] _positionUpdateItems;

    [Space(20)]
    [SerializeField][Range(0, 1)] private float _itemDropRate;
    [SerializeField] private ConvertUpdate_ItemData[] _dropItemDatas;

    private Vector2 _positionUpdateDirection;


    // Text Template
    public override string Description()
    {
        return base.Description()
            .Replace("{preTemperatureUpdateValue}", _passivePlayerDataModifier.temperatureUpdateValue.ToString())
            .Replace("{temperatureUpdateValue}", playerDataModifier.temperatureUpdateValue.ToString())
            .Replace("{positionUpdateDirection}", _positionUpdateDirection.ToString());
    }


    // Activation
    public override List<Tile> Generated_ActivationTiles()
    {
        List<Vector2> allDirections = Utility.Surrounding_Directions();
        _positionUpdateDirection = allDirections[Random.Range(0, allDirections.Count)];

        return new(InGame_Manager.instance.tilesController.currentTiles);
    }

    public override void Activate_Event()
    {
        Update_TileStates();
        Update_PlayerState();

        RemoveUpdate_ProtectItems();
        PositionUpdate_PlacedItems();
    }


    // Custom Activations
    private void Update_PlayerState()
    {
        InGame_Manager manager = InGame_Manager.instance;

        TileTracker playerTracker = manager.player.movement.tileTracker;
        Tile playerTile = playerTracker.data.CurrentTile();

        _passivePlayerDataModifier.Update_Data();

        for (int i = 0; i < _protectItems.Length; i++)
        {
            if (playerTile.Placed_ItemCount(_protectItems[i]) > 0) return;
        }
        playerDataModifier.Update_Data();

        Tile updateTile = manager.tilesController.Current_Tile((Vector2)playerTile.transform.position + _positionUpdateDirection);

        playerTracker.TrackUpdate_CurrentTile(updateTile);
        playerTracker.Clamp_toCurrentTile();
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
                PlaceableItem removeItem = placedProtectItems[placedCount - 1];

                removeItem.AnimationDelay_Remove(_itemRemoveAnimationclip);

                Vector2 movePosition = (Vector2)removeItem.transform.position + _positionUpdateDirection;
                LeanTween.move(removeItem.gameObject, movePosition, _updatesDuration).setEase(_itemRemoveTweenType);

                break;
            }
        }
    }

    private bool PositionUpdate_Available(PlaceableItem placedItem, Tile positionUpdateTile)
    {
        if (positionUpdateTile == null) return false;
        if (placedItem.placedTile == positionUpdateTile) return true;

        Item_ScrObj item = placedItem.data.itemScrObj;

        for (int i = 0; i < _positionUpdateItems.Length; i++)
        {
            if (item != _positionUpdateItems[i]) continue;
            if (positionUpdateTile.ItemPlace_AvailableCount(item) < item.maxAmount) return false;

            return true;
        }
        return false;
    }
    private void PositionUpdate_PlacedItems()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        List<PlaceableItem> allPlacedItems = new(tilesController.placedItems);

        Dictionary<PlaceableItem, Tile> positionUpdateItemDatas = new();
        List<Tile> activationTiles = new();

        while (positionUpdateItemDatas.Count < _positionUpdateCount && allPlacedItems.Count > 0)
        {
            int randItemIndex = Random.Range(0, allPlacedItems.Count);
            PlaceableItem randPlacedItem = allPlacedItems[randItemIndex];

            allPlacedItems.RemoveAt(randItemIndex);

            Tile placedItemTile = randPlacedItem.placedTile;
            if (activationTiles.Contains(placedItemTile)) continue; // no duplicate tile placed items position update

            Vector2 updateTilePos = (Vector2)placedItemTile.transform.position + _positionUpdateDirection;
            Tile updateTile = tilesController.Current_Tile(updateTilePos);

            if (PositionUpdate_Available(randPlacedItem, updateTile) == false) continue;

            positionUpdateItemDatas[randPlacedItem] = updateTile;
            activationTiles.Add(placedItemTile);
        }

        List<PlaceableItem> dropUpdateItems = new();

        foreach (var updateData in positionUpdateItemDatas)
        {
            PlaceableItem placedItem = updateData.Key;
            dropUpdateItems.Add(placedItem);

            Tile updateTile = updateData.Value;

            placedItem.placedTile.Remove_PlacedItemData(placedItem);

            updateTile.Track_PlacingItem(placedItem);
            placedItem.Track_CurrentTile(updateTile);

            updateTile.ClampUpdate_PlacedItemOffsets(_updatesDuration);
        }

        manager.time.timeUpdateActions.Add(this);
        StartCoroutine(DropUpdateDelay_PositionUpdateItems(dropUpdateItems));
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

    private void DropUpdate_PositionUpdateItems(List<PlaceableItem> positionUpdateItems)
    {
        for (int i = 0; i < positionUpdateItems.Count; i++)
        {
            PlaceableItem updateItems = positionUpdateItems[i];

            ItemData placedData = updateItems.data;
            Item_ScrObj placedItem = placedData.itemScrObj;

            int placedAmount = placedData.amount;

            float amountRatio = Mathf.Clamp01((float)placedAmount / placedItem.maxAmount);
            float dropRate = Mathf.Lerp(0f, _itemDropRate, amountRatio);

            if (Random.value >= dropRate) return;

            ItemData dropData = DropUpdate_ItemData(placedItem);
            if (dropData == null) return;

            int dropAmount = Random.Range(1, dropData.amount);
            updateItems.placedTile.Set_PlacingItem(new(dropData.itemScrObj, dropAmount));
        }
    }
    private IEnumerator DropUpdateDelay_PositionUpdateItems(List<PlaceableItem> positionUpdateItems)
    {
        yield return new WaitForSeconds(_updatesDuration);

        DropUpdate_PositionUpdateItems(positionUpdateItems);
        InGame_Manager.instance.time.timeUpdateActions.Remove(this);
    }
}