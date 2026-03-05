using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;
    
    [Space(20)]
    [SerializeField] private Item_ScrObj[] _treeItems;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Use_OnTree;
        _useableItem.OnUseDestroy += DestroyCheck;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Use_OnTree;
        _useableItem.OnUseDestroy -= DestroyCheck;
    }


    // Main
    private void Use_OnTree(Tile useTile)
    {
        int treeCount = 0;
        
        for (int i = 0; i < _treeItems.Length; i++)
        {
            PlaceableItem treeItem = useTile.PlacedItem(_treeItems[i]);
            if (treeItem == null) continue;

            treeCount++;
            treeItem.animPlayer.Play(0);
        }

        _useableItem.Update_UseAmount(treeCount);
    }

    private void DestroyCheck()
    {
        Debug.Log("destroy check");
    }
}
