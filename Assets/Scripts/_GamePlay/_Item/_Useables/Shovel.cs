using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Shovel_DigUpdateData
{
    [SerializeField][Range(0, 100)] private int _chanceRate;
    public int chanceRate => _chanceRate;

    [SerializeField] private TileUpdate_ItemData _tileDigItemData;
    public TileUpdate_ItemData tileDigItemData => _tileDigItemData;
}

public class Shovel : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;
    public UseableItem useableItem => _useableItem;

    [Space(20)]
    [SerializeField] private Shovel_DigUpdateData[] _digUpdateDatas;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Dig_TargetTile;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Dig_TargetTile;
    }


    // Main
    private Shovel_DigUpdateData WeightRandom_DigUpdateData(Tile targetTile)
    {
        List<Shovel_DigUpdateData> targetTileUpdateDatas = new();

        for (int i = 0; i < _digUpdateDatas.Length; i++)
        {
            Shovel_DigUpdateData digUpdateData = _digUpdateDatas[i];
            TileUpdate_ItemData digItemData = digUpdateData.tileDigItemData;

            if (targetTile.data.tileScrObj != digItemData.tile) continue;
            if (digItemData.updateItem.discoverTimeRangeData.CurrentDayTime_InRange() == false) continue;

            targetTileUpdateDatas.Add(digUpdateData);
        }
        if (targetTileUpdateDatas.Count <= 0) return null;

        int totalWeight = 0;
        foreach (Shovel_DigUpdateData data in targetTileUpdateDatas)
        {
            totalWeight += Mathf.Max(0, data.chanceRate);
        }

        int randomValue = Random.Range(0, totalWeight);
        for (int i = 0; i < targetTileUpdateDatas.Count; i++)
        {
            Shovel_DigUpdateData updateData = targetTileUpdateDatas[i];
            int weight = Mathf.Max(0, updateData.chanceRate);

            if (randomValue < weight) return updateData;
            randomValue -= weight;
        }
        return null;
    }
    private void Dig_TargetTile(Tile targetTile)
    {
        Shovel_DigUpdateData digUpdateData = WeightRandom_DigUpdateData(targetTile);
        if (digUpdateData == null) return;

        Item_ScrObj digUpdateItem = digUpdateData.tileDigItemData.TilePlaced_UpdateItem(targetTile);
        if (digUpdateItem == null) return;

        targetTile.Set_PlacingItem(new(digUpdateItem, 1));
        _useableItem.Update_UseAmount(1);
    }
}
