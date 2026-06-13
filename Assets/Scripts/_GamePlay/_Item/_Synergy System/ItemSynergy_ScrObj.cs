using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item Synergy")]
public class ItemSynergy_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private string _synergyName;

    [SerializeField][Multiline] private string _description;
    public string description => _description;

    [Space(20)]
    [SerializeField] private TileScrObj[] _targetTiles;
    public TileScrObj[] targetTiles => _targetTiles;
    
    [SerializeField] private ItemData[] _requiredItemDatas;
    public ItemData[] requiredItemDatas => _requiredItemDatas;

    [SerializeField] private TimeRange_Data _timeRangeData;
    public TimeRange_Data timeRangeData => _timeRangeData;

    [Space(20)]
    [SerializeField] private ItemSynergy_EffectData[] _effectDatas;
    public ItemSynergy_EffectData[] effectDatas => _effectDatas;


    public bool TargetTile_Match(TileScrObj checkTile)
    {
        if (_targetTiles.Length <= 0) return true;
        
        for (int i = 0; i < _targetTiles.Length; i++)
        {
            if (checkTile == _targetTiles[i]) return true;
        }
        return false;
    }

    public bool RequiredItems_Match(List<ItemData> checkItemDatas)
    {
        if (_requiredItemDatas.Length <= 0) return true;
        if (checkItemDatas.Count <= 0) return false;

        List<ItemData> combinedRequiredDatas = new();

        for (int i = 0; i < _requiredItemDatas.Length; i++)
        {
            ItemData requiredData = _requiredItemDatas[i];

            Item_ScrObj requiredItem = requiredData.itemScrObj;
            int requiredAmount = Mathf.Max(1, requiredData.amount);

            bool duplicateFound = false;

            for (int j = 0; j < combinedRequiredDatas.Count; j++)
            {
                ItemData combinedData = combinedRequiredDatas[j];
                if (requiredItem != combinedData.itemScrObj) continue;

                combinedData.Update_CurrentAmount(combinedData.amount + requiredAmount);
                duplicateFound = true;

                break;
            }

            if (duplicateFound) continue;
            combinedRequiredDatas.Add(new(requiredItem, requiredAmount));
        }

        for (int i = 0; i < combinedRequiredDatas.Count; i++)
        {
            ItemData requiredData = combinedRequiredDatas[i];
            int requiredAmount = requiredData.amount;

            for (int j = 0; j < checkItemDatas.Count; j++)
            {
                ItemData checkData = checkItemDatas[j];

                if (checkData.itemScrObj != requiredData.itemScrObj) continue;
                requiredAmount -= checkData.amount;
            }
            if (requiredAmount > 0) return false;
        }
        return true;
    }
}