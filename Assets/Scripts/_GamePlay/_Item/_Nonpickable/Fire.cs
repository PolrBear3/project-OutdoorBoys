using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField] private FillBar_Controller _fillBarController;
    [SerializeField] private Tile_Indicator _heatTileIndicator;

    [Space(20)]
    [SerializeField] private ItemData[] _burnUpdateItems;
    [SerializeField] private Item_ScrObj[] _wetStateProtectItems;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _burnDecreaseValue;
    [SerializeField][Range(0, 10)] private int _heatTilesUpdateThresholdPoint;

    [Space(20)]
    [SerializeField] private ConvertUpdate_ItemData[] _heatItemDatas;

    [Space(20)]
    [SerializeField] private Item_ScrObj _coalItem;
    [SerializeField][Range(0, 100)] private int _coalGenerateAmount;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _temperatureIncreaseValue;

    private int _coalGeneratedCount;


    // MonoBehaviour
    private void Start()
    {
        _placeableItem.placedTile.OnStateUpdate += RemoveUpdate_WetState;

        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.Register(ActionUpdateBus.AwakeUpdate, UpdatePlaced_BurnItems);
        time.Register(ActionUpdateBus.AwakeUpdate, Update_BurningState);

        time.Register(ActionUpdateBus.AwakeUpdate, Update_HeatTiles);
        time.Register(ActionUpdateBus.AwakeUpdate, Toggle_HeatTiles);

        time.Register(ActionUpdateBus.AwakeUpdate, Update_PlayerTemperature);
        time.Register(ActionUpdateBus.AwakeUpdate, Update_HeatingItems);

        Tiles_Controller tiles = manager.tilesController;

        tiles.OnTileHover += Toggle_FillBar;
        tiles.OnTileHover += Toggle_HeatTiles;


        _fillBarController.Set_FillBar(transform);
        Toggle_FillBar(InGame_Manager.instance.cursor.pointingTile);

        Update_HeatTiles();
        Toggle_HeatTiles();
    }

    private void OnDestroy()
    {
        _placeableItem.placedTile.OnStateUpdate -= RemoveUpdate_WetState;

        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.UnRegister(ActionUpdateBus.AwakeUpdate, UpdatePlaced_BurnItems);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_BurningState);

        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_HeatTiles);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Toggle_HeatTiles);

        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_PlayerTemperature);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_HeatingItems);

        Tiles_Controller tiles = manager.tilesController;

        tiles.OnTileHover -= Toggle_FillBar;
        tiles.OnTileHover -= Toggle_HeatTiles;


        _heatTileIndicator.Clear_CurrentIndicators();
    }


    // Visuals
    private void Toggle_FillBar(Tile hoveringTile)
    {
        bool toggle = hoveringTile != null && hoveringTile == _placeableItem.placedTile;
        _fillBarController.Toggle(toggle);

        if (toggle == false) return;
        _fillBarController.Update_CurrentBarFill(_placeableItem.data.itemScrObj.maxAmount, _placeableItem.data.amount);
    }

    private void Toggle_HeatTiles(Tile hoveringTile)
    {
        bool toggle = hoveringTile != null && hoveringTile == _placeableItem.placedTile;
        _heatTileIndicator.Toggle_CurrentIndicators(toggle);
    }
    private void Toggle_HeatTiles()
    {
        Toggle_HeatTiles(InGame_Manager.instance.cursor.pointingTile);
    }


    // Main
    private void Remove()
    {
        Tile currentTile = _placeableItem.placedTile;

        currentTile.Remove_PlacedItemData(_placeableItem);
        currentTile.Set_Item(new(_coalItem, _coalGeneratedCount));

        Destroy(_placeableItem.gameObject);
    }
    private void RemoveUpdate_WetState(TileState updateState, bool activated)
    {
        if (activated == false) return;
        if (updateState != TileState.wet) return;
        
        Tile currentTile = _placeableItem.placedTile;
        
        foreach (Item_ScrObj item in _wetStateProtectItems)
        {
            if (currentTile.Placed_ItemCount(item) > 0) return;
        }
        Remove();
    }

    private void UpdatePlaced_BurnItems()
    {
        Tile currentTile = _placeableItem.placedTile;
        List<PlaceableItem> placedBurnItems = currentTile.PlacedItems();

        for (int i = placedBurnItems.Count - 1; i >= 0; i--)
        {
            PlaceableItem placedItem = placedBurnItems[i];

            for (int j = 0; j < _burnUpdateItems.Length; j++)
            {
                ItemData updateItem = _burnUpdateItems[j];
                if (placedItem.data.itemScrObj != updateItem.itemScrObj) continue;

                ItemData itemData = _placeableItem.data;
                int updateAmount = itemData.amount + (placedItem.data.amount * updateItem.amount);

                itemData.Update_CurrentAmount(Mathf.Min(updateAmount, itemData.itemScrObj.maxAmount));
                _fillBarController.Update_CurrentBarFill(itemData.itemScrObj.maxAmount, itemData.amount);

                currentTile.Remove_PlacedItemData(placedItem);
                Destroy(placedItem.gameObject);

                break;
            }
        }
    }
    private void Update_BurningState()
    {
        ItemData itemData = _placeableItem.data;

        itemData.Update_CurrentAmount(itemData.amount - _burnDecreaseValue);
        _fillBarController.Update_CurrentBarFill(itemData.itemScrObj.maxAmount, itemData.amount);

        if (itemData.amount > 0)
        {
            _coalGeneratedCount += _coalGenerateAmount;
            return;
        }
        Remove();
    }
    
    private void Update_HeatTiles()
    {
        ItemData itemData = _placeableItem.data;
        int burnCount = itemData.amount;

        if (burnCount <= 0)
        {
            _heatTileIndicator.Clear_CurrentIndicators();
            return;
        }

        Tile currentTile = _placeableItem.placedTile;
        int maxTileCount = _heatTileIndicator.defaultTilePositions.Length;

        float thresholdAmount = itemData.itemScrObj.maxAmount * (_heatTilesUpdateThresholdPoint / 10f);
        int calculatedTileCount = burnCount >= thresholdAmount ? maxTileCount : Mathf.CeilToInt(burnCount / thresholdAmount * maxTileCount);

        List<Vector2> defaultPositions = _heatTileIndicator.Available_DefaultPositions(currentTile);
        int updateTileCount = Mathf.Clamp(calculatedTileCount, 1, defaultPositions.Count);

        if (updateTileCount == _heatTileIndicator.currentIndicateDatas.Count) return;

        defaultPositions.Remove(Vector2.zero);

        List<Vector2> updatePositions = new()
        {Vector2.zero};

        while (updatePositions.Count < updateTileCount)
        {
            int randIndex = Random.Range(0, defaultPositions.Count);

            updatePositions.Add(defaultPositions[randIndex]);
            defaultPositions.RemoveAt(randIndex);
        }

        _heatTileIndicator.Clear_CurrentIndicators();
        _heatTileIndicator.Set_Indicators(currentTile, updatePositions);
    }
    private void Update_PlayerTemperature()
    {
        Player_Controller player = InGame_Manager.instance.player;

        if (_heatTileIndicator.Current_IndicateTiles().Contains(player.movement.tileTracker.data.CurrentTile()) == false) return;
        player.Update_Temperature(player.data.temperature + _temperatureIncreaseValue);
    }


    // Placed Items Heating
    private ConvertUpdate_ItemData Update_ItemData(Item_ScrObj checkItem)
    {
        for (int i = 0; i < _heatItemDatas.Length; i++)
        {
            ConvertUpdate_ItemData updateItemData = _heatItemDatas[i];

            if (checkItem != updateItemData.preUpdateItem && updateItemData.Is_ConvertedItem(checkItem) == false) continue;
            return updateItemData;
        }
        return null;
    }

    private void Update_HeatingItems()
    {
        if (_placeableItem.data.amount <= 0) return;

        List<Tile> currentHeatTiles = _heatTileIndicator.Current_IndicateTiles();
        
        for (int i = 0; i < currentHeatTiles.Count; i++)
        {
            Tile heatTile = currentHeatTiles[i];
            List<PlaceableItem> placedItems = heatTile.PlacedItems();
            
            for (int j = placedItems.Count - 1; j >= 0 ; j--)
            {
                PlaceableItem placedItem = placedItems[j];

                Item_ScrObj itemToUpdate = placedItem.data.itemScrObj;
                ConvertUpdate_ItemData updateData = Update_ItemData(itemToUpdate);
                
                if (updateData == null) continue;

                int replaceAmount = placedItem.data.amount;

                heatTile.Remove_PlacedItemData(placedItem);
                Destroy(placedItem.gameObject);

                Item_ScrObj replaceItem = itemToUpdate == updateData.preUpdateItem ? updateData.Converted_ItemData().itemScrObj : _coalItem;
                heatTile.Set_Item(new(replaceItem, replaceAmount));
            }
        }
    }
}