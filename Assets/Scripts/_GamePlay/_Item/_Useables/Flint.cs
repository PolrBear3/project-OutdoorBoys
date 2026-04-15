using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class Flint : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [Space(20)]
    [SerializeField] private FillBar_Controller _fillBarController;

    [Space(20)]
    [SerializeField] private Item_ScrObj _stoneItem;
    [SerializeField] private Item_ScrObj _fireItem;

    [Space(10)]
    [SerializeField] private ItemData[] _fireActivateItemDatas;

    [Space(20)]
    [SerializeField][Range(0, 50)] private int _fireActivationCount;
    [SerializeField][Range(0, 50)] private float _heatSustainTime;

    private PlaceableItem_DurabilityData _targetRockData;
    private Coroutine _heatSustainCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.CanUse += Rock_Placed;
        _useableItem.OnUse += Spawn_Fire;
    }
    
    private void OnDestroy()
    {
        _useableItem.CanUse -= Rock_Placed;
        _useableItem.OnUse -= Spawn_Fire;
    }


    // Rock Targeting
    private void Update_TargetRock(PlaceableItem targetRockItem)
    {
        if (targetRockItem == null) return;

        _useableItem.Update_UseAmount(1);

        if (_targetRockData != null && _targetRockData.placeableItem == targetRockItem) return;
        _targetRockData = new(targetRockItem, 0);

        _fillBarController.Set_FillBar(targetRockItem.transform);
        _fillBarController.Update_CurrentBarFill(_fireActivationCount, _targetRockData.durabilityCount);
    }

    private void Update_HeatSustainTime()
    {
        if (_heatSustainCoroutine != null)
        {
            StopCoroutine(_heatSustainCoroutine);
            _heatSustainCoroutine = null;
        }
        _heatSustainCoroutine = StartCoroutine(HeatSustain_Update());
    }
    private IEnumerator HeatSustain_Update()
    {
        while (_targetRockData != null && _targetRockData.durabilityCount > 0)
        {
            yield return new WaitForSeconds(_heatSustainTime);

            _targetRockData.Update_DurabilityCount(_targetRockData.durabilityCount - 1);
            _fillBarController.Update_CurrentBarFill(_fireActivationCount, _targetRockData.durabilityCount);
        }

        _targetRockData = null;
        _heatSustainCoroutine = null;

        _fillBarController.Refresh_CurrentFillBar();
    }


    // Fire Activation
    private int ActivateItem_BurnCount(Item_ScrObj activateItem)
    {
        for (int i = 0; i < _fireActivateItemDatas.Length; i++)
        {
            ItemData activateData = _fireActivateItemDatas[i];
            
            if (activateItem != activateData.itemScrObj) continue;
            return activateData.amount;
        }
        return 0;
    }
    private Dictionary<PlaceableItem, int> FireActivate_BurnCountDatas(Tile useTile)
    {
        if (_fireActivateItemDatas.Length <= 0) return null;
        List<Item_ScrObj> activateItems = new();

        foreach (ItemData activateData in _fireActivateItemDatas)
        {
            activateItems.Add(activateData.itemScrObj);
        }

        List<PlaceableItem> placedActivateItems = useTile.PlacedItems(activateItems);
        Dictionary<PlaceableItem, int> burnCountDatas = new();

        for (int i = 0; i < placedActivateItems.Count; i++)
        {
            PlaceableItem placeableItem = placedActivateItems[i];
            ItemData placedItemData = placeableItem.data;

            burnCountDatas[placeableItem] = placedItemData.amount * ActivateItem_BurnCount(placedItemData.itemScrObj);
        }
        return burnCountDatas;
    }

    private bool Rock_Placed(Tile checkTile)
    {
        return checkTile.PlacedItems(_stoneItem).Count > 0;
    } 
    private void Spawn_Fire(Tile useTile)
    {
        PlaceableItem placedRockItem = useTile.PlacedItem(_stoneItem);
        Update_TargetRock(placedRockItem);
        
        _targetRockData.Update_DurabilityCount(_targetRockData.durabilityCount + 1);
        _fillBarController.Update_CurrentBarFill(_fireActivationCount, _targetRockData.durabilityCount);

        if (_targetRockData.durabilityCount < _fireActivationCount)
        {
            Update_HeatSustainTime();
            return;
        }

        ItemData rockItemData = placedRockItem.data;

        rockItemData.Update_CurrentAmount(rockItemData.amount - 1);
        useTile.Remove_EmptyPlacedItems();

        Dictionary<PlaceableItem, int> activateDatas = FireActivate_BurnCountDatas(useTile);
        if (activateDatas.Count <= 0) return;

        int totalBurnCount = 0;

        foreach (var data in activateDatas)
        {
            totalBurnCount += data.Value;
        }
        useTile.Set_Item(new(_fireItem, totalBurnCount));
    }
}