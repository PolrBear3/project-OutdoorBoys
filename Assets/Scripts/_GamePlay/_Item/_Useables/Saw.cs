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


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Use_OnTree;
        _useableItem.OnUse += SetWood_OnDestroy;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Use_OnTree;
        _useableItem.OnUse -= SetWood_OnDestroy;
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


    private void Use_OnTree(Tile useTile) // set this action to Aim System
    {
        PlaceableItem placedTree = PlacedTree(useTile);
        if (placedTree == null) return;

        _useableItem.Update_UseAmount(1);

        if (Random.Range(0f, 100f) < 5f)
        {
            placedTree.animPlayer.Play(0);
            return;
        }
        placedTree.AnimationDelay_Remove();
    }

    private void SetWood_OnDestroy(Tile useTile)
    {

    }
}
