using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;
    
    [Space(20)]
    [SerializeField] private Item_ScrObj[] _treeItems;

    Coroutine _useUpdateCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Use_OnTree;
        _useableItem.OnUse += Set_Wood;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Use_OnTree;
        _useableItem.OnUse -= Set_Wood;
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
        Update_OnUse(placedTree);
    }

    private void Update_OnUse(PlaceableItem placedTree)
    {
        if (_useUpdateCoroutine != null) return;

        // damage (change this to something like (health > 0) return)
        if (Random.Range(0f, 100f) < 5f)
        {
            placedTree.animPlayer.Play(0);
            return;
        }

        // destroy (aim system success)
        _useUpdateCoroutine = StartCoroutine(UseUpdate(placedTree));
    }
    private IEnumerator UseUpdate(PlaceableItem placedTree)
    {
        placedTree.currentTile.Remove_PlacedItemData(placedTree);

        AnimationPlayer animPlayer = placedTree.animPlayer;
        animPlayer.Play(1);
        
        while (animPlayer.Animation_Playing()) yield return null;
        Destroy(placedTree.gameObject);

        _useUpdateCoroutine = null;
        yield break;
    }

    private void Set_Wood(Tile useTile)
    {
        
    }
}
