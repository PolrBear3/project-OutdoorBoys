using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { place, use, nonpickable }

[System.Serializable]
public class Offset_PositionData
{
    [SerializeField] private Vector2 _position;
    public Vector2 position => _position;

    [SerializeField] private float _rotationValue;
    public float rotationValue => _rotationValue;
}

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item")]
public class Item_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private Sprite _inventorySprite;
    public Sprite inventorySprite => _inventorySprite;

    [SerializeField] private Sprite _placedSprite;

    [SerializeField] private Sprite _microSprite;
    public Sprite microSprite => _microSprite;

    [Space(20)]
    [SerializeField] private string _itemName;
    public string itemName => _itemName;

    [SerializeField][Multiline] private string _description;
    public string description => _description;

    [Space(20)]
    [SerializeField] private ItemType _itemType;
    public ItemType itemType => _itemType;

    [SerializeField] private GameObject _itemPrefab;
    public GameObject itemPrefab => _itemPrefab;

    [Space(20)]
    [SerializeField] private Offset_PositionData[] _offsetData;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _maxAmount;
    public int maxAmount => _maxAmount;

    [SerializeField][Range(0, 100)] private int _itemWeight;
    public int itemWeight => _itemWeight;

    [SerializeField][Range(0, 10)] private int _triggerRange;
    public int triggerRange => _triggerRange;

    [SerializeField][Range(0, 10)] private float _coolTime;
    public float coolTime => _coolTime;

    [Space(20)]
    [SerializeField] private TimeRange_Data _discoverTimeRangeData;
    public TimeRange_Data discoverTimeRangeData => _discoverTimeRangeData;

    [Space(20)]
    [SerializeField] private ItemRule_ScrObj[] _placeRestrictions;
    public ItemRule_ScrObj[] placeRestrictions => _placeRestrictions;

    [SerializeField] private ItemRule_ScrObj[] _selectRestrictions;
    public ItemRule_ScrObj[] selectRestrictions => _selectRestrictions;

    [Space(20)]
    [SerializeField] private ItemData[] _itemIngredientDatas;

    [SerializeField] private ItemData[] _craftRequiredPlacedItemDatas;
    public ItemData[] craftRequiredPlacedItemDatas => _craftRequiredPlacedItemDatas;


    // Data
    public Offset_PositionData Offset_Data(int offsetIndex)
    {
        if (_offsetData.Length <= 0) return null;
        return _offsetData[Mathf.Clamp(offsetIndex, 0, _offsetData.Length - 1)];
    }

    public Sprite PlacedSprite()
    {
        return _placedSprite != null ? _placedSprite : _inventorySprite;
    }


    // Restrictions
    public bool Place_Available(ItemData currentData, Tile targetTile)
    {
        if (_placeRestrictions.Length <= 0) return true;

        for (int i = 0; i < _placeRestrictions.Length; i++)
        {
            if (_placeRestrictions[i].Available(currentData, targetTile) == false) return false;
        }
        return true;
    }

    public bool Select_Available(ItemData currentData, Tile targetTile)
    {
        if (_selectRestrictions.Length <= 0) return true;

        for (int i = 0; i < _selectRestrictions.Length; i++)
        {
            if (_selectRestrictions[i].Available(currentData, targetTile) == false) return false;
        }
        return true;
    }


    // Ingredients
    public List<ItemData> Item_IngredientDatas()
    {
        List<ItemData> combinedDatas = new();

        for (int i = 0; i < _itemIngredientDatas.Length; i++)
        {
            ItemData ingredientData = _itemIngredientDatas[i];
            Item_ScrObj ingredientItem = ingredientData.itemScrObj;
            int ingredientAmount = Mathf.Max(1, ingredientData.amount);

            bool duplicateFound = false;

            for (int j = 0; j < combinedDatas.Count; j++)
            {
                ItemData combinedData = combinedDatas[j];
                if (ingredientData.itemScrObj != combinedData.itemScrObj) continue;

                combinedData.Update_CurrentAmount(combinedData.amount + ingredientAmount);
                duplicateFound = true;
                break;
            }

            if (duplicateFound) continue;
            combinedDatas.Add(new(ingredientItem, ingredientAmount));
        }

        return combinedDatas;
    }

    public int Available_CraftCount(List<ItemData> checkItemDatas)
    {
        List<ItemData> ingredientDatas = new(Item_IngredientDatas());

        if (ingredientDatas.Count <= 0) return 0;
        if (_discoverTimeRangeData.Is_ActiveDay() == false) return 0;
        if (_discoverTimeRangeData.Is_ActiveTime() == false || _discoverTimeRangeData.Is_RestrictTime()) return 0;

        int maxCraftCount = int.MaxValue;

        for (int i = 0; i < ingredientDatas.Count; i++)
        {
            ItemData ingredientData = ingredientDatas[i];
            Item_ScrObj ingredientItem = ingredientData.itemScrObj;

            bool isUseItem = ingredientItem.itemType == ItemType.use;
            int haveAmount = 0;

            for (int j = 0; j < checkItemDatas.Count; j++)
            {
                ItemData checkItemData = checkItemDatas[j];
                if (checkItemData?.itemScrObj != ingredientItem) continue;

                int checkItemAmount = checkItemData.amount;

                if (isUseItem == false)
                {
                    haveAmount += checkItemAmount;
                    continue;
                }

                // counts only max amount useable items as 1 (change for gameplay)
                if (checkItemAmount < ingredientItem.maxAmount) continue;
                haveAmount++;
            }

            if (haveAmount < ingredientData.amount) return 0;
            int craftByThisIngredient = haveAmount / ingredientData.amount;

            if (craftByThisIngredient >= maxCraftCount) continue;
            maxCraftCount = craftByThisIngredient;
        }

        return maxCraftCount;
    }
}