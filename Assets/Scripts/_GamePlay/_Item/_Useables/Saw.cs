using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [Space(20)]
    [SerializeField] private FillBar_Controller _fillBarController;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _chopDamage;
    [SerializeField] private ConvertUpdate_ItemData[] _chopItemDatas;

    private const int _maxChoppingItemDatasCount = 10;
    private List<PlaceableItem_DurabilityData> _choppingItemDatas = new();


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.CanUse += ChopItem_Placed;
        _useableItem.OnUse += Chop_PlacedItem;
    }

    private void OnDestroy()
    {
        _useableItem.CanUse -= ChopItem_Placed;
        _useableItem.OnUse -= Chop_PlacedItem;
    }


    // Main
    private PlaceableItem Placed_ChopItem(Tile useTile)
    {
        List<PlaceableItem> placedItems = useTile.PlacedItems();

        for (int i = 0; i < placedItems.Count; i++)
        {
            PlaceableItem placedItem = placedItems[i];

            for (int j = 0; j < _chopItemDatas.Length; j++)
            {
                if (placedItem.data.itemScrObj != _chopItemDatas[j].preUpdateItem) continue;
                return placedItem;
            }
        }
        return null;
    }
    private bool ChopItem_Placed(Tile checkTile)
    {
        return Placed_ChopItem(checkTile) != null;
    }

    private PlaceableItem_DurabilityData Chopping_ItemData(PlaceableItem choppingItem)
    {
        for (int i = 0; i < _choppingItemDatas.Count; i++)
        {
            PlaceableItem_DurabilityData choppingData = _choppingItemDatas[i];

            if (choppingItem != choppingData.placeableItem) continue;
            return choppingData;
        }
        return null;
    }

    private void Update_ChoppingItems()
    {
        for (int i = _choppingItemDatas.Count - 1; i >= 0; i--)
        {
            PlaceableItem_DurabilityData data = _choppingItemDatas[i];

            if (data.placeableItem != null && data.durabilityCount > 0) continue;
            _choppingItemDatas.RemoveAt(i);
        }

        if (_choppingItemDatas.Count <= _maxChoppingItemDatasCount) return;
        _choppingItemDatas.RemoveAt(0);
    }
    private PlaceableItem_DurabilityData Update_ChoppingItem(PlaceableItem targetItem)
    {
        PlaceableItem_DurabilityData existingData = Chopping_ItemData(targetItem);
        if (existingData != null) return existingData;

        int maxDurability = targetItem.data.itemScrObj.itemWeight;
        _choppingItemDatas.Add(new(targetItem, maxDurability));

        Update_ChoppingItems();

        _fillBarController.Set_FillBar(targetItem.transform);
        _fillBarController.Update_CurrentBarFill(maxDurability, maxDurability);

        return Chopping_ItemData(targetItem);
    }

    private void Chop_PlacedItem(Tile useTile)
    {
        PlaceableItem placedTree = Placed_ChopItem(useTile);
        PlaceableItem_DurabilityData updateData = Update_ChoppingItem(placedTree);

        _useableItem.Update_UseAmount(1);
        placedTree.animPlayer.Play(0);

        int currentDurability = updateData.Update_DurabilityCount(updateData.durabilityCount - _chopDamage);

        if (currentDurability > 0)
        {
            _fillBarController.Set_FillBar(placedTree.transform);
            _fillBarController.Update_CurrentBarFill(placedTree.data.itemScrObj.itemWeight, currentDurability);
            return;
        }
        _fillBarController.Refresh_CurrentFillBar();

        _choppingItemDatas.Remove(updateData);
        placedTree.AnimationDelay_Remove();

        DropUpdate_onChop(useTile, placedTree.data.itemScrObj);
    }
    private void DropUpdate_onChop(Tile useTile, Item_ScrObj chopItem)
    {
        int treeWeight = chopItem.itemWeight;

        for (int i = 0; i < _chopItemDatas.Length; i++)
        {
            ConvertUpdate_ItemData updateItemData = _chopItemDatas[i];
            if (chopItem != updateItemData.preUpdateItem) continue;

            ItemData dropItemData = updateItemData.Converted_ItemData();
            int dropAmount = Random.Range(Mathf.Min(dropItemData.amount, treeWeight), treeWeight);

            useTile.Set_PlacingItem(new(dropItemData.itemScrObj, dropAmount));
            return;
        }
    }
}