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
    [SerializeField][Range(0, 100)] private int _chopDamage;
    [SerializeField][Range(0, 100)] private int _minWoodDropAmount;

    private PlaceableItem_DurabilityData _choppingTreeData;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Chop_Tree;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Chop_Tree;
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

    private void Update_ChoppingTree(PlaceableItem targetItem)
    {
        if (_choppingTreeData != null && _choppingTreeData.placeableItem == targetItem) return;
        
        if (targetItem == null)
        {
            _choppingTreeData = null;
            return;
        }

        _choppingTreeData = new(targetItem, targetItem.data.itemScrObj.itemWeight);
    }


    private void Chop_Tree(Tile useTile)
    {
        PlaceableItem placedTree = PlacedTree(useTile);
        if (placedTree == null) return;
        
        Update_ChoppingTree(placedTree);

        _useableItem.Update_UseAmount(1);
        placedTree.animPlayer.Play(0);

        if (_choppingTreeData.Update_DurabilityCount(_choppingTreeData.durabilityCount - _chopDamage) > 0) return;

        Update_ChoppingTree(null);
        placedTree.AnimationDelay_Remove();

        Drop_Wood(useTile, placedTree.data.itemScrObj);
    }

    private void Drop_Wood(Tile useTile, Item_ScrObj treeItem)
    {
        int treeWeight = treeItem.itemWeight;
        int dropAmount = Random.Range(Mathf.Min(_minWoodDropAmount, treeWeight), treeWeight);
        
        useTile.Set_PlacingItem(new(_woodItem, dropAmount));
    }
}
