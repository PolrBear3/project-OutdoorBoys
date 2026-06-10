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
    [SerializeField] private Shovel_DigUpdateData[] _discoverUpdateDatas;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _discoverDigCount;

    private const int _maxDigProgressDatasCount = 10;
    private Dictionary<Tile, int> _digProgressDatas = new();


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
    private void Update_DiggingTiles()
    {
        List<Tile> digProgressTiles = new();

        foreach (var data in _digProgressDatas)
        {
            digProgressTiles.Add(data.Key);
        }

        for (int i = 0; i < digProgressTiles.Count; i++)
        {
            Tile progressTile = digProgressTiles[i];

            if (_digProgressDatas[progressTile] > 0) continue;
            _digProgressDatas[progressTile] = _discoverDigCount + 1;
        }

        if (digProgressTiles.Count <= _maxDigProgressDatasCount) return;
        _digProgressDatas.Remove(digProgressTiles[0]);
    }
    private int Update_DigProgression(Tile diggingTile)
    {
        if (_digProgressDatas.ContainsKey(diggingTile))
        {
            _digProgressDatas[diggingTile]--;
            return _digProgressDatas[diggingTile];
        }

        _digProgressDatas[diggingTile] = _discoverDigCount;
        return _digProgressDatas[diggingTile];
    }

    private Shovel_DigUpdateData WeightRandom_DigUpdateData(Tile targetTile, Shovel_DigUpdateData[] digUpdateDatas)
    {
        List<Shovel_DigUpdateData> targetTileUpdateDatas = new();

        for (int i = 0; i < digUpdateDatas.Length; i++)
        {
            Shovel_DigUpdateData digUpdateData = digUpdateDatas[i];
            TileUpdate_ItemData digItemData = digUpdateData.tileDigItemData;

            if (targetTile.data.tileScrObj != digItemData.tile) continue;
            if (digItemData.updateItem.discoverTimeRangeData.CurrentDayTime_InRange() == false) continue;

            List<Tile> surroundingTiles = TilePatterns_Utility.PivotDistanced_Tiles(targetTile, 1);
            if (digItemData.CustomTiles_ItemsPlaced(surroundingTiles) == false) continue;

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
        bool discover = Update_DigProgression(targetTile) <= 0;
        Update_DiggingTiles();

        Shovel_DigUpdateData[] digItemDatas = discover ? _discoverUpdateDatas : _digUpdateDatas;

        Shovel_DigUpdateData digItemData = WeightRandom_DigUpdateData(targetTile, digItemDatas);
        if (digItemData == null) return;

        Item_ScrObj digUpdateItem = digItemData.tileDigItemData.TargetTilePlaced_UpdateItem(targetTile);
        if (digUpdateItem == null) return;

        targetTile.Set_PlacingItem(new(digUpdateItem, 1));
        _useableItem.Update_UseAmount(1);
    }
}
