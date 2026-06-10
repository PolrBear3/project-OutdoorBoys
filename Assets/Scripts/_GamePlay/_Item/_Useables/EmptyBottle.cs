using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EmptyBottle : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [SerializeField] private Item_ScrObj _emptyBottle;

    [Space(20)]
    [SerializeField] private TileUpdate_ItemData[] _updateItemDatas;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Update_OnFill;
        _useableItem.OnUseDestroy += Update_OnEmpty;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Update_OnFill;
        _useableItem.OnUseDestroy -= Update_OnEmpty;
    }


    // Main
    private void Update_OnFill(Tile useTile)
    {
        if (_emptyBottle != _useableItem.data.itemScrObj) return;

        for (int i = 0; i < _updateItemDatas.Length; i++)
        {
            Item_ScrObj updateItem = _updateItemDatas[i].TargetTilePlaced_UpdateItem(useTile);
            if (updateItem == null) continue;

            ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;

            itemCursor.Set_Data(new(updateItem, updateItem.maxAmount));
            itemCursor.Update_Visuals();

            return;
        }
    }

    private void Update_OnEmpty()
    {
        if (_emptyBottle == _useableItem.data.itemScrObj) return;

        ItemCursor itemCursor = InGame_Manager.instance.cursor.itemCursor;

        itemCursor.Set_Data(new(_emptyBottle, _emptyBottle.maxAmount));
        itemCursor.Update_Visuals();
    }
}