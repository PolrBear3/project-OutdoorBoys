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
    [SerializeField] private Tile_Indicator _cookTileIndicator;

    [Space(20)]
    [SerializeField] private ItemData[] _burnUpdateItems;
    [SerializeField][Range(0, 100)] private int _burnDecreaseValue;

    [Space(20)]
    [SerializeField] private Item_ScrObj _coalItem;
    [SerializeField][Range(0, 100)] private int _coalGenerateAmount;

    private int _coalGeneratedCount;


    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnTimeCount += UpdatePlaced_BurnItems;
        time.OnTimeCount += Update_BurningState;

        time.OnTimeCount += Update_CookTiles;
        time.OnTimeCount += Toggle_CookTileIndicators;

        Tiles_Controller tiles = manager.tilesController;

        tiles.OnTileHover += Toggle_FillBar;
        tiles.OnTileHover += Toggle_CookTileIndicators;
    }

    private void Start()
    {
        _fillBarController.Set_FillBar(transform);
        Toggle_FillBar(InGame_Manager.instance.cursor.pointingTile);

        Update_CookTiles(0);
        Toggle_CookTileIndicators(0);
    }

    private void OnDestroy()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnTimeCount -= UpdatePlaced_BurnItems;
        time.OnTimeCount -= Update_BurningState;

        time.OnTimeCount -= Update_CookTiles;
        time.OnTimeCount -= Toggle_CookTileIndicators;

        Tiles_Controller tiles = manager.tilesController;

        tiles.OnTileHover -= Toggle_FillBar;
        tiles.OnTileHover -= Toggle_CookTileIndicators;
    }


    // Visuals
    private void Toggle_FillBar(Tile hoveringTile)
    {
        bool toggle = hoveringTile != null && hoveringTile == _placeableItem.currentTile;
        _fillBarController.Toggle(toggle);

        if (toggle == false) return;
        _fillBarController.Update_CurrentBarFill(_placeableItem.data.itemScrObj.maxAmount, _placeableItem.data.amount);
    }

    private void Toggle_CookTileIndicators(Tile hoveringTile)
    {
        bool toggle = hoveringTile != null && hoveringTile == _placeableItem.currentTile;
        _cookTileIndicator.Toggle_CurrentIndicators(toggle);
    }
    private void Toggle_CookTileIndicators(int _)
    {
        Toggle_CookTileIndicators(InGame_Manager.instance.cursor.pointingTile);
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
        if (_placeableItem.data.amount > 0)
        {
            ItemData itemData = _placeableItem.data;

            itemData.Update_CurrentAmount(itemData.amount - _burnDecreaseValue);
            _fillBarController.Update_CurrentBarFill(itemData.itemScrObj.maxAmount, itemData.amount);

            _coalGeneratedCount += _coalGenerateAmount;
            return;
        }

        Tile currentTile = _placeableItem.currentTile;

        currentTile.Remove_PlacedItemData(_placeableItem);
        currentTile.Set_Item(new(_coalItem, _coalGeneratedCount));

        Destroy(_placeableItem.gameObject);
    }

    private void Update_CookTiles(int _)
    {
        ItemData itemData = _placeableItem.data;
        int burnCount = itemData.amount;

        if (burnCount <= 0)
        {
            _cookTileIndicator.Clear_CurrentIndicators();
            return;
        }

        int maxTileCount = _cookTileIndicator.defaultTilePositions.Length;

        int calculatedTileCount = Mathf.CeilToInt((float)burnCount / itemData.itemScrObj.maxAmount * maxTileCount);
        int updateTileCount = Mathf.Clamp(calculatedTileCount, 1, _cookTileIndicator.defaultTilePositions.Length);

        if (updateTileCount == _cookTileIndicator.currentIndicateDatas.Count) return;

        List<Vector2> defaultPositions = _cookTileIndicator.Default_TilePositions();
        defaultPositions.Remove(Vector2.zero);

        List<Vector2> updatePositions = new()
        {Vector2.zero};

        while (updatePositions.Count < updateTileCount)
        {
            int randIndex = Random.Range(0, defaultPositions.Count);

            updatePositions.Add(defaultPositions[randIndex]);
            defaultPositions.RemoveAt(randIndex);
        }
        _cookTileIndicator.Set_Indicators(_placeableItem.currentTile, updatePositions);
    }
}