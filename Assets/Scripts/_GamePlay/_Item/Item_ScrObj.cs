using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { place, use }

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

    [Space(20)]
    [SerializeField] private GameObject _itemPrefab;
    public GameObject itemPrefab => _itemPrefab;

    [SerializeField] private Vector2 _offsetPosition;
    public Vector2 offsetPosition => _offsetPosition;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _maxAmount;
    public int maxAmount => _maxAmount;

    [SerializeField][Range(0, 100)] private int _itemWeight;
    public int itemWeight => _itemWeight;

    [Space(20)]
    [SerializeField] private bool _stackable;
    public bool stackable => _stackable;

    [SerializeField][Range(0, 10)]  private int _triggerRange;
    public int triggerRange => _triggerRange;

    [Space(20)]
    [SerializeField] private ItemData[] _itemIngredientDatas;


    // _itemIngredientDatas
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
