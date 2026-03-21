using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [Space(20)]
    [SerializeField] private Item_ScrObj _woodItem;
    [SerializeField] private Item_ScrObj[] _treeItems;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _minWoodDropAmount;


    private PlaceableItem_DurabilityData _targetTreeData;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Use_OnTree;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Use_OnTree;
    }


    // Main
    private PlaceableItem PlacedTree(Tile useTile)
    {
        List<PlaceableItem> placedItems = useTile.placedItems;

        for (int i = 0; i < placedItems.Count; i++)
        {
            PlaceableItem placedItem = placedItems[i];

            for (int j = 0; j < _treeItems.Length; j++)
            {
                if (placedItem.data.itemScrObj != _treeItems[j]) continue;
                return placedItem;
            }
        }
        return null;
    }

    private void Update_TargetTree(PlaceableItem targetItem)
    {
        if (_targetTreeData != null && _targetTreeData.placeableItem == targetItem) return;
        
        if (targetItem == null)
        {
            _targetTreeData = null;
            return;
        }

        _targetTreeData = new(targetItem, targetItem.data.itemScrObj.itemWeight);
    }


    private void Use_OnTree(Tile useTile)
    {
        PlaceableItem placedTree = PlacedTree(useTile);
        if (placedTree == null) return;
        
        Update_TargetTree(placedTree);

        _useableItem.Update_UseAmount(1);
        placedTree.animPlayer.Play(0);

        if (_targetTreeData.Update_DurabilityCount(_targetTreeData.durabilityCount - 1) > 0) return;

        placedTree.AnimationDelay_Remove();
        DropWood_OnDestroy(useTile, placedTree.data.itemScrObj);
    }

    private void DropWood_OnDestroy(Tile useTile, Item_ScrObj treeItem)
    {
        int treeWeight = treeItem.itemWeight;
        int dropAmount = Random.Range(Mathf.Min(_minWoodDropAmount, treeWeight), treeWeight);
        
        useTile.Set_PlacingItem(new(_woodItem, dropAmount));
    }
}
