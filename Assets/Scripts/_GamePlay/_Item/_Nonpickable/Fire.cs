using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private PlaceableItem _placeableItem;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _burnDecreaseValue;

    [Space(10)]
    [SerializeField] private ItemData[] _burnUpdateItems;


    // MonoBehaviour
    private void Awake()
    {
        Time_Manager time = InGame_Manager.instance.time;

        time.OnTimeUpdate += UpdatePlaced_BurnItems;
        time.OnTimeUpdate += Update_BurningState;
    }
    
    private void OnDestroy()
    {
        Time_Manager time = InGame_Manager.instance.time;

        time.OnTimeUpdate -= UpdatePlaced_BurnItems;
        time.OnTimeUpdate -= Update_BurningState;
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

            return;
        }

        // drop coal item ?

        _placeableItem.currentTile.Remove_PlacedItemData(_placeableItem);
        Destroy(_placeableItem.gameObject);
    }
}