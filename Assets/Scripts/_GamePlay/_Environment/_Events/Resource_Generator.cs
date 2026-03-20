using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ResourceGenerate_Data
{
    [SerializeField] private Item_ScrObj _resourceItem;
    public Item_ScrObj resourceItem => _resourceItem;

    [Space(10)]
    [SerializeField] private TileScrObj[] _generateTiles;
    public TileScrObj[] generateTiles => _generateTiles;

    [Space(10)]
    [SerializeField][Range(0, 100)] private int _generateRate;
    public int generateRate => _generateRate;

    [SerializeField][Range(0, 100)] private int _generateAmount;
    public int generateAmount => _generateAmount;

    [SerializeField][Range(0, 100)] private int _maxGenerateAmount;
    public int maxGenerateAmount => _maxGenerateAmount;

    [SerializeField][Range(0, 10)] private int _generatePointDistance;
    public int generatePointDistance => _generatePointDistance;


    // Data
    public bool TargetTile_Match(TileScrObj targetTile)
    {
        for (int i = 0; i < _generateTiles.Length; i++)
        {
            if (targetTile == _generateTiles[i]) return true;
        }
        return false;
    }
}

public class Resource_Generator : MonoBehaviour
{
    [Space(20)]
    [SerializeField][Range(0, 10)] private int _generateCoolTime;

    private int _currentCoolTime;

    [Space(10)]
    [SerializeField] private ResourceGenerate_Data[] _generateDatas;


    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager.instance.time.Register(TimeUpdateBus.AwakeUpdate, Generate);
    }

    private void Start()
    {
        Generate_OnLoad();
    }

    private void OnDestroy()
    {
        InGame_Manager.instance.time.UnRegister(TimeUpdateBus.AwakeUpdate, Generate);
    }


    // Generate
    private ResourceGenerate_Data Random_GenerateData()
    {
        // get total wieght
        int totalWeight = 0;
        foreach (ResourceGenerate_Data data in _generateDatas)
        {
            totalWeight += data.generateRate;
        }

        // track values
        int randValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        // get random according to weight
        for (int i = 0; i < _generateDatas.Length; i++)
        {
            ResourceGenerate_Data data = _generateDatas[i];
            cumulativeWeight += data.generateRate;

            if (randValue >= cumulativeWeight) continue;
            return data;
        }

        return null;
    }

    private bool MaxAmount_Generated(ResourceGenerate_Data generateData)
    {
        ItemsSource_Manager ingameItemSources = InGame_Manager.instance.itemSourceManager;
        return ingameItemSources.ItemData_Count(generateData.resourceItem) >= generateData.maxGenerateAmount;
    }
    private bool Generate_Available(Tile generateTile, Tile playerTile, ResourceGenerate_Data generateData)
    {
        float checkDistance = Utility.Chebyshev_Distance(generateTile.transform.position, playerTile.transform.position);

        if (checkDistance < generateData.generatePointDistance) return false;
        if (!generateData.TargetTile_Match(generateTile.data.tileScrObj)) return false;
        if (generateTile.ItemPlace_AvailableCount(generateData.resourceItem) <= 0) return false;

        return true;
    }

    private bool Generate(ResourceGenerate_Data generateData, int generateAmount)
    {
        if (generateData == null) return false;
        if (MaxAmount_Generated(generateData)) return false;

        InGame_Manager manager = InGame_Manager.instance;

        Tile playerTile = manager.player.movement.currentTile;
        List<Tile> tiles = new(manager.tilesController.currentTiles);

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            if (Generate_Available(tiles[i], playerTile, generateData)) continue;
            tiles.RemoveAt(i);
        }

        Item_ScrObj generateItem = generateData.resourceItem;
        bool itemGenerated = false;

        for (int i = 0; i < generateAmount; i++)
        {
            if (tiles.Count <= 0) break;
            if (MaxAmount_Generated(generateData)) break;

            Tile generateTile = tiles[Random.Range(0, tiles.Count)];
            generateTile.Set_PlacingItem(new(generateItem, 1));

            itemGenerated = true;

            if (generateTile.ItemPlace_AvailableCount(generateItem) > 0) continue;
            tiles.Remove(generateTile);
        }

        return itemGenerated;
    }

    private void Generate_OnLoad()
    {
        for (int i = 0; i < _generateDatas.Length; i++)
        {
            ResourceGenerate_Data data = _generateDatas[i];
            Generate(data, Random.Range(0, data.maxGenerateAmount + 1));
        }
    }
    private void Generate()
    {
        _currentCoolTime = Mathf.Clamp(_currentCoolTime + 1, 0, _generateCoolTime + 1);

        if (_currentCoolTime <= _generateCoolTime) return;

        ResourceGenerate_Data generateData = Random_GenerateData();
        Generate(generateData, generateData.generateAmount);

        _currentCoolTime = 0;
    }
}