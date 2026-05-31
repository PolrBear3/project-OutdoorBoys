using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyBottle : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [Space(20)]
    [SerializeField] private TileUpdate_ItemData[] _updateItemDatas;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Update_OnFill;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Update_OnFill;
    }


    // Main
    private void Update_OnFill(Tile useTile)
    {
        
    }
}