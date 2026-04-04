using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField] private FillBar_Controller _fillBarController;
    [SerializeField][Range(0, 100)] private int _fillBarDecreasePoint;

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

        time.OnTimeUpdate += UpdatePlaced_BurnItems;
        time.OnTimeUpdate += Update_BurningState;

        manager.tilesController.OnTileHover += Toggle_FillBar;
    }

    private void Start()
    {
        _fillBarController.Set_FillBar(transform);
    }

    private void OnDestroy()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnTimeUpdate -= UpdatePlaced_BurnItems;
        time.OnTimeUpdate -= Update_BurningState;

        manager.tilesController.OnTileHover -= Toggle_FillBar;
    }


    // Visuals
    private void Toggle_FillBar(Tile hoveringTile)
    {
        _fillBarController.Toggle(hoveringTile == _placeableItem.currentTile);
    }


    // Main
    private void UpdatePlaced_BurnItems()
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

                itemData.Update_CurrentAmount(itemData.amount + (placedItem.data.amount * updateItem.amount));
                _fillBarController.Update_CurrentBarFill(_fillBarDecreasePoint, itemData.amount);

                currentTile.Remove_PlacedItemData(placedItem);
                Destroy(placedItem.gameObject);

                break;
            }
        }
    }

    private void Update_BurningState()
    {
        if (_placeableItem.data.amount > 0)
        {
            ItemData itemData = _placeableItem.data;

            itemData.Update_CurrentAmount(itemData.amount - _burnDecreaseValue);
            _fillBarController.Update_CurrentBarFill(_fillBarDecreasePoint, itemData.amount);

            _coalGeneratedCount += _coalGenerateAmount;
            return;
        }

        Tile currentTile = _placeableItem.currentTile;

        currentTile.Remove_PlacedItemData(_placeableItem);
        currentTile.Set_Item(new(_coalItem, _coalGeneratedCount));

        Destroy(_placeableItem.gameObject);
    }
}