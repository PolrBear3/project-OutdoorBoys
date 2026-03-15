using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private ResourceGenerate_Data[] _generateDatas;


    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager.instance.time.OnTimeCount += Generate;
    }
    
    private void OnDestroy()
    {
        InGame_Manager.instance.time.OnTimeCount -= Generate;
    }
    

    // Generate
    private ResourceGenerate_Data GenerateData()
    {
        // get total wieght
        int totalWeight = 0;
        foreach (ResourceGenerate_Data data in _generateDatas)
        {
            totalWeight += data.maxGenerateAmount;
        }

        // track values
        int randValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        // get random food according to weight
        for (int i = 0; i < _generateDatas.Length; i++)
        {
            ResourceGenerate_Data data = _generateDatas[i];
            cumulativeWeight += data.maxGenerateAmount;

            if (randValue >= cumulativeWeight) continue;
            return data;
        }

        return null;
    }

    private bool Generate_Available(Tile generateTile, Tile playerTile, ResourceGenerate_Data generateData)
    {
        float checkDistance = Utility.Chebyshev_Distance(generateTile.transform.position, playerTile.transform.position);
        
        if (checkDistance < generateData.generatePointDistance) return false;
        if (!generateData.TargetTile_Match(generateTile.data.tileScrObj)) return false;
        if (generateTile.ItemPlace_AvailableCount(generateData.resourceItem) <= 0) return false;

        return true;
    }

    private void Generate()
    {
        InGame_Manager manager = InGame_Manager.instance;

        ResourceGenerate_Data generateData = GenerateData();
        Item_ScrObj generateItem = generateData.resourceItem;

        // if generateItem amount is max amount, return ?

        Tile playerTile = manager.player.movement.currentTile;
        List<Tile> tiles = new(manager.tilesController.currentTiles);

        for (int i = tiles.Count - 1; i >= 0 ; i--)
        {
            if (Generate_Available(tiles[i], playerTile, generateData)) continue;
            tiles.RemoveAt(i);
        }

        if (tiles.Count <= 0) return;
        tiles[Random.Range(0, tiles.Count)].Set_PlacingItem(new(generateItem, 1));
    }
}