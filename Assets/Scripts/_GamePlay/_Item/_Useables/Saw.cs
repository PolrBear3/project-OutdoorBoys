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


    private void Use_OnTree(Tile useTile)
    {
        PlaceableItem placedTree = PlacedTree(useTile);
        if (placedTree == null) return;

        _useableItem.Update_UseAmount(1);

        if (Random.Range(0, 100) <= 10)
        {
            placedTree.animPlayer.Play(0);
            return;
        }

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
