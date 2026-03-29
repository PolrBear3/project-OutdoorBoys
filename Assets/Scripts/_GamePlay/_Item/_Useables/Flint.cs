using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flint : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [Space(20)]
    [SerializeField] private Item_ScrObj[] _activateWoodItems;
    [SerializeField] private Item_ScrObj[] _activateTreeItems;

    [Space(20)]
    [SerializeField][Range(0, 50)] private float _heatSustainTime;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Detect_PlacedWood;
    }
    
    private void OnDestroy()
    {
        _useableItem.OnUse -= Detect_PlacedWood;
    }


    // Use on Wood
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

    private void Detect_PlacedWood(Tile useTile)
    {
        PlaceableItem placedItem = PlacedWood(useTile);

        if (placedItem == null) return;
        Debug.Log(placedItem.data.amount);
    }


    // Use on Tree
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