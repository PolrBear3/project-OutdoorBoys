using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class HeatUpdate_ItemData
{
    [SerializeField] private Item_ScrObj _preHeatItem;
    public Item_ScrObj preHeatItem => _preHeatItem;

    [SerializeField] private Item_ScrObj _heatedItem;
    public Item_ScrObj heatedItem => _heatedItem;

    [SerializeField][Range(0, 100)] private int _updatePointValue;
    public int updatePointValue => _updatePointValue;
}

public class Fire : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField] private FillBar_Controller _fillBarController;
    [SerializeField] private Tile_Indicator _heatTileIndicator;

    [Space(20)]
    [SerializeField] private ItemData[] _burnUpdateItems;
    [SerializeField][Range(0, 100)] private int _burnDecreaseValue;

    [Space(20)]
    [SerializeField] private HeatUpdate_ItemData[] _heatItemDatas;

    [Space(20)]
    [SerializeField] private Item_ScrObj _coalItem;
    [SerializeField][Range(0, 100)] private int _coalGenerateAmount;


    private Dictionary<PlaceableItem, int> _trackingDatas = new();
    private int _coalGeneratedCount;


    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnTimeCount += UpdatePlaced_BurnItems;
        time.OnTimeCount += Update_BurningState;

        time.OnTimeCount += Update_HeatTiles;
        time.OnTimeCount += Toggle_HeatTiles;

        time.OnTimeCount += Track_HeatingItems;
        time.OnTimeCount += Update_HeatingItems;

        Tiles_Controller tiles = manager.tilesController;

        tiles.OnTileHover += Toggle_FillBar;
        tiles.OnTileHover += Toggle_HeatTiles;
    }

    private void Start()
    {
        _fillBarController.Set_FillBar(transform);
        Toggle_FillBar(InGame_Manager.instance.cursor.pointingTile);

        Update_HeatTiles(0);
        Toggle_HeatTiles(0);
    }

    private void OnDestroy()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnTimeCount -= UpdatePlaced_BurnItems;
        time.OnTimeCount -= Update_BurningState;

        time.OnTimeCount -= Update_HeatTiles;
        time.OnTimeCount -= Toggle_HeatTiles;

        time.OnTimeCount -= Track_HeatingItems;
        time.OnTimeCount -= Update_HeatingItems;

        Tiles_Controller tiles = manager.tilesController;

        tiles.OnTileHover -= Toggle_FillBar;
        tiles.OnTileHover -= Toggle_HeatTiles;
    }


    // Visuals
    private void Toggle_FillBar(Tile hoveringTile)
    {
        bool toggle = hoveringTile != null && hoveringTile == _placeableItem.currentTile;
        _fillBarController.Toggle(toggle);

        if (toggle == false) return;
        _fillBarController.Update_CurrentBarFill(_placeableItem.data.itemScrObj.maxAmount, _placeableItem.data.amount);
    }

    private void Toggle_HeatTiles(Tile hoveringTile)
    {
        bool toggle = hoveringTile != null && hoveringTile == _placeableItem.currentTile;
        _heatTileIndicator.Toggle_CurrentIndicators(toggle);
    }
    private void Toggle_HeatTiles(int _)
    {
        Toggle_HeatTiles(InGame_Manager.instance.cursor.pointingTile);
    }


    // Main
    private void UpdatePlaced_BurnItems(int _)
    {
        Tile currentTile = _placeableItem.currentTile;
        List<PlaceableItem> placedBurnItems = currentTile.placedItems;

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

    private void Update_BurningState(int _)
    {
        ItemData itemData = _placeableItem.data;

        itemData.Update_CurrentAmount(itemData.amount - _burnDecreaseValue);
        _fillBarController.Update_CurrentBarFill(itemData.itemScrObj.maxAmount, itemData.amount);

        if (itemData.amount > 0)
        {
            _coalGeneratedCount += _coalGenerateAmount;
            return;
        }

        Transfer_HeatingItems();

        Tile currentTile = _placeableItem.currentTile;

        currentTile.Remove_PlacedItemData(_placeableItem);
        currentTile.Set_Item(new(_coalItem, _coalGeneratedCount));

        Destroy(_placeableItem.gameObject);
    }

    private void Update_HeatTiles(int _)
    {
        ItemData itemData = _placeableItem.data;
        int burnCount = itemData.amount;

        if (burnCount <= 0)
        {
            _heatTileIndicator.Clear_CurrentIndicators();
            return;
        }

        int maxTileCount = _heatTileIndicator.defaultTilePositions.Length;
        int calculatedTileCount = Mathf.CeilToInt((float)burnCount / itemData.itemScrObj.maxAmount * maxTileCount);

        Tile currentTile = _placeableItem.currentTile;

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
        _heatTileIndicator.Set_Indicators(currentTile, updatePositions);
    }


    // Placed Items Heating
    private HeatUpdate_ItemData Update_ItemData(Item_ScrObj checkItem)
    {
        for (int i = 0; i < _heatItemDatas.Length; i++)
        {
            HeatUpdate_ItemData updateItemData = _heatItemDatas[i];

            if (checkItem != updateItemData.preHeatItem && checkItem != updateItemData.heatedItem) continue;
            return updateItemData;
        }
        return null;
    }

    private List<PlaceableItem> Placed_FireItems()
    {
        Item_ScrObj fireItem = _placeableItem.data.itemScrObj;
        return InGame_Manager.instance.tilesController.PlacedItems(fireItem);
    }
    private Fire ItemTracking_OtherFire(PlaceableItem trackingItem)
    {
        List<PlaceableItem> placedFireItems = Placed_FireItems();

        for (int i = placedFireItems.Count - 1; i >= 0; i--)
        {
            PlaceableItem placedItem = placedFireItems[i];

            if (placedItem == _placeableItem) continue;
            if (placedItem.TryGetComponent(out Fire fire) == false) continue;

            Dictionary<PlaceableItem, int> trackingDatas = fire._trackingDatas;

            foreach (var data in trackingDatas)
            {
                if (data.Key != trackingItem) continue;
                return fire;
            }
        }
        return null;
    }


    private void Track_HeatingItems(int _)
    {
        List<Tile> currentHeatTiles = _heatTileIndicator.Current_IndicateTiles();

        for (int i = 0; i < currentHeatTiles.Count; i++)
        {
            Tile heatTile = currentHeatTiles[i];
            List<PlaceableItem> placedItems = heatTile.PlacedItems();

            for (int j = 0; j < placedItems.Count; j++)
            {
                PlaceableItem placedItem = placedItems[j];

                if (Update_ItemData(placedItem.data.itemScrObj) == null) continue;
                if (_trackingDatas.ContainsKey(placedItem)) continue;

                Fire otherTrackingFire = ItemTracking_OtherFire(placedItem);
                if (otherTrackingFire == null)
                {
                    _trackingDatas[placedItem] = 0;
                    continue;
                }

                List<Tile> heatingTiles = otherTrackingFire._heatTileIndicator.Current_IndicateTiles();
                if (heatingTiles.Contains(placedItem.currentTile)) continue;

                _trackingDatas[placedItem] = otherTrackingFire._trackingDatas[placedItem] + 1;
                otherTrackingFire._trackingDatas.Remove(placedItem);
            }
        }
    }

    private void Update_HeatingItems(int _)
    {
        if (_placeableItem.data.amount <= 0) return;

        List<Tile> currentHeatTiles = _heatTileIndicator.Current_IndicateTiles();

        foreach (PlaceableItem trackingItem in _trackingDatas.Keys.ToList())
        {
            if (trackingItem == null)
            {
                _trackingDatas.Remove(trackingItem);
                continue;
            }

            Tile trackingItemTile = trackingItem.currentTile;
            int currentValue = _trackingDatas[trackingItem] += currentHeatTiles.Contains(trackingItemTile) ? 1 : -1;

            if (currentValue <= 0)
            {
                _trackingDatas.Remove(trackingItem);
                continue;
            }

            Item_ScrObj currentItem = trackingItem.data.itemScrObj;
            HeatUpdate_ItemData updateData = Update_ItemData(currentItem);

            if (updateData == null) continue;
            if (currentValue < updateData.updatePointValue) continue;

            int replaceAmount = trackingItem.data.amount;

            trackingItemTile.Remove_PlacedItemData(trackingItem);
            Destroy(trackingItem.gameObject);

            _trackingDatas.Remove(trackingItem);

            Item_ScrObj replaceItem = currentItem == updateData.preHeatItem ? updateData.heatedItem : _coalItem;
            trackingItemTile.Set_Item(new(replaceItem, replaceAmount));
        }
    }

    private void Transfer_HeatingItems()
    {
        List<PlaceableItem> placedFireItems = Placed_FireItems();
        placedFireItems.Remove(_placeableItem);

        PlaceableItem closestPlacedFire = null;
        int closestDistance = int.MaxValue;

        for (int i = 0; i < placedFireItems.Count; i++)
        {
            PlaceableItem otherPlacedFireItem = placedFireItems[i];
            int checkDistance = _placeableItem.currentTile.DistanceTo_TargetTile(otherPlacedFireItem.currentTile);

            if (checkDistance >= closestDistance) continue;

            closestPlacedFire = otherPlacedFireItem;
            closestDistance = checkDistance;
        }

        if (closestPlacedFire == null) return;
        if (closestPlacedFire.TryGetComponent(out Fire placedOtherFire) == false) return;

        foreach (var data in _trackingDatas.ToList())
        {
            PlaceableItem transferItem = data.Key;
            int transferHeatCount = data.Value;

            bool otherFireTracking = placedOtherFire._trackingDatas.TryGetValue(transferItem, out int heatCount);
            if (otherFireTracking && heatCount >= transferHeatCount) continue;

            placedOtherFire._trackingDatas[transferItem] = data.Value;
        }
    }
}