using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

public class Flint : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [Space(20)]
    [SerializeField] private Item_ScrObj _stoneItem;
    [SerializeField] private Item_ScrObj _fireItem;

    [Space(10)]
    [SerializeField] private Item_ScrObj[] _activateWoodItems;
    [SerializeField] private Item_ScrObj[] _activateTreeItems;

    [Space(20)]
    [SerializeField][Range(0, 50)] private int _fireActivationCount;
    [SerializeField][Range(0, 50)] private float _heatSustainTime;

    private PlaceableItem_DurabilityData _targetRockData;
    private Coroutine _heatSustainCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Activate_WoodFire;
    }
    
    private void OnDestroy()
    {
        _useableItem.OnUse -= Activate_WoodFire;
    }


    // Rock Targeting
    private void Update_TargetRock(PlaceableItem targetRockItem)
    {
        if (targetRockItem == null) return;

        _useableItem.Update_UseAmount(1);

        if (_targetRockData != null && _targetRockData.placeableItem == targetRockItem) return;
        _targetRockData = new(targetRockItem, 0);
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
        yield return new WaitForSeconds(_heatSustainTime);

        _targetRockData = null;
        _heatSustainCoroutine = null;
    }


    // Wood Fire Activation
    private PlaceableItem PlacedWood(Tile useTile)
    {
        if (_activateWoodItems.Length <= 0) return null;

        for (int i = 0; i < _activateWoodItems.Length; i++)
        {
            PlaceableItem placedItem = useTile.PlacedItem(_activateWoodItems[i]);

            if (placedItem == null) continue;
            return placedItem;
        }
        return null;
    }

    private void Activate_WoodFire(Tile useTile)
    {
        PlaceableItem placedRockItem = useTile.PlacedItem(_stoneItem);
        if (placedRockItem == null) return;

        Update_TargetRock(placedRockItem);
        _targetRockData.Update_DurabilityCount(_targetRockData.durabilityCount + 1);

        if (_targetRockData.durabilityCount < _fireActivationCount)
        {
            Update_HeatSustainTime();
            return;
        }

        ItemData rockItemData = placedRockItem.data;

        rockItemData.Update_CurrentAmount(rockItemData.amount - 1);
        useTile.Remove_EmptyPlacedItems();

        // fire spark animation ?

        PlaceableItem placedWoodItem = PlacedWood(useTile);
        if (placedWoodItem == null) return;

        int fireSpawnAmount = placedWoodItem.data.amount;
        placedWoodItem.AnimationDelay_Remove();

        useTile.Set_Item(new(_fireItem, fireSpawnAmount));
    }


    // Tree Fire Activation
    private List<PlaceableItem> PlacedTrees(Tile useTile)
    {
        if (_activateTreeItems.Length <= 0) return null;

        List<Item_ScrObj> treeItems = new();

        foreach (Item_ScrObj item in _activateTreeItems)
        {
            treeItems.Add(item);
        }
        return useTile.PlacedItems(treeItems);
    }
}