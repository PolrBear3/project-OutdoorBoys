using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { place, use }

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

    [SerializeField][Range(0, 10)]  private int _triggerRange;
    public int triggerRange => _triggerRange;

    [Space(20)]
    [SerializeField] private ItemData[] _itemIngredientDatas;


    // Data
    public Offset_PositionData Offset_Data(int offsetIndex)
    {
        return _offsetData[Mathf.Clamp(offsetIndex, 0, _offsetData.Length - 1)];
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

        int maxCraftCount = int.MaxValue;

        for (int i = 0; i < ingredientDatas.Count; i++)
        {
            ItemData ingredientData = ingredientDatas[i];
            Item_ScrObj ingredientItem = ingredientData.itemScrObj;

            int haveAmount = 0;

            for (int j = 0; j < checkItemDatas.Count; j++)
            {
                ItemData checkItemData = checkItemDatas[j];
                if (checkItemData?.itemScrObj != ingredientItem) continue;

                haveAmount += checkItemData.amount;
            }

            if (haveAmount < ingredientData.amount) return 0;
            int craftByThisIngredient = haveAmount / ingredientData.amount;

            if (craftByThisIngredient >= maxCraftCount) continue;
            maxCraftCount = craftByThisIngredient;
        }

        return maxCraftCount;
    }
}
