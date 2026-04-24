using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class WorldMap_Generator : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private WorldMapScrObj _defaultWorldMap;


    private WorldMapScrObj _currentWorldMap;

    private GameObject _currentMapEventsPrefab;
    public GameObject currentMapEventsPrefab => _currentMapEventsPrefab;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Load_WorldMapData);

        EventBus_Manager.Register(EventBus.AwakeLoad, Generate_PresetTiles);
        EventBus_Manager.Register(EventBus.AwakeLoad, Generate_Tiles);

        EventBus_Manager.Register(EventBus.AwakeLoad, Set_MapEventsPrefab);
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Background);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Load_WorldMapData);

        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Generate_PresetTiles);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Generate_Tiles);

        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_MapEventsPrefab);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Background);
    }


    // Datas
    public void Load_WorldMapData(WorldMapScrObj loadData)
    {
        _currentWorldMap = loadData;
    }
    private void Load_WorldMapData()
    {
        Load_WorldMapData(_defaultWorldMap);
    }


    public Vector2 Converted_GenerateSize()
    {
        Vector2 generateSize = _currentWorldMap.generateSize;
        return new(Mathf.RoundToInt(generateSize.x), Mathf.RoundToInt(generateSize.y));
    }

    public Vector2 Generate_StartPosition()
    {
        Vector2 convertedSize = Converted_GenerateSize();
        return new(-(convertedSize.x - 1) / 2, (convertedSize.y - 1) / 2);
    }

    private List<Vector2> Generate_Positions()
    {
        List<Vector2> positions = new();

        Vector2 convertedSize = Converted_GenerateSize();
        Vector2 generateStartPos = Generate_StartPosition();

        int generateCount = Mathf.RoundToInt(convertedSize.x * convertedSize.y);
        int horizontalCount = 0;

        for (int i = 0; i < generateCount; i++)
        {
            positions.Add(new(generateStartPos.x + horizontalCount, generateStartPos.y));
            horizontalCount++;

            if (horizontalCount < convertedSize.x) continue;

            horizontalCount = 0;
            generateStartPos.y--;
        }

        return positions;
    }


    // Pre Load Datas
    private List<TileType> DensityConverted_TileTypes(int convertCount)
    {
        List<TileType> tileTypes = new();

        for (int i = 0; i < convertCount; i++)
        {
            bool isHarshGround = _currentWorldMap.harshGroundDensity > UnityEngine.Random.Range(0, 100);
            TileType setType = isHarshGround ? TileType.harshGround : TileType.softGround;

            tileTypes.Add(setType);
        }

        return tileTypes;
    }

    private Dictionary<Vector2, TileType> Iterated_TileDatas()
    {
        Dictionary<Vector2, TileType> datas = new();

        List<Vector2> positions = Generate_Positions();
        List<TileType> tileTypes = DensityConverted_TileTypes(positions.Count);

        for (int i = 0; i < positions.Count; i++)
        {
            List<Vector2> surroundingPositions = Utility.Surrounding_Positions(positions[i]);
            int harshGroundCount = 0;

            for (int j = 0; j < surroundingPositions.Count; j++)
            {
                bool positionFound = false;

                for (int k = 0; k < positions.Count; k++)
                {
                    if (surroundingPositions[j] != positions[k]) continue;
                    positionFound = true;

                    if (tileTypes[k] != TileType.harshGround) break;

                    // harsh ground count
                    harshGroundCount++;
                    if (harshGroundCount >= 4) break;
                }

                if (positionFound) continue;

                // empty position count
                harshGroundCount++;
                if (harshGroundCount >= 4) break;
            }

            TileType iteratedType = harshGroundCount >= 4 ? TileType.harshGround : TileType.softGround;
            datas.Add(positions[i], iteratedType);
        }

        return datas;
    }


    // Load
    private Tile Generate_Tile(Vector2 generatePos, TileScrObj generateTile)
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;
        List<Tile> currentTiles = tilesController.currentTiles;
        
        for (int i = 0; i < currentTiles.Count; i++)
        {
            if ((Vector2)currentTiles[i].transform.position != generatePos) continue;
            return null;
        }

        GameObject generatedTile = Instantiate(generateTile.prefab, generatePos, quaternion.identity);

        if (!generatedTile.TryGetComponent(out Tile tile))
        {
            Debug.Log("Tile Script Not Attached!");
            return null;
        }

        tile.Set_Data(generateTile);

        tile.transform.SetParent(tilesController.transform);
        currentTiles.Add(tile);

        return tile;
    }

    private void Generate_PresetTiles()
    {
        Tile_PresetDatas[] presetTileDatas = _currentWorldMap.presetTileDatas;
        if (presetTileDatas.Length <= 0) return;

        List<Vector2> generatePositions = new(Generate_Positions());

        for (int i = 0; i < presetTileDatas.Length; i++)
        {
            for (int j = 0; j < presetTileDatas[i].generateAmount; j++)
            {
                if (generatePositions.Count <= 0) return;

                int randInex = UnityEngine.Random.Range(0, generatePositions.Count);
                
                Generate_Tile(generatePositions[randInex], presetTileDatas[i].tileScrObj);
                generatePositions.RemoveAt(randInex);
            }
        }
    }
    private void Generate_Tiles()
    {
        Data_Manager dataManager = Data_Manager.instance;
        Dictionary<Vector2, TileType> generateDatas = Iterated_TileDatas();

        foreach (var data in generateDatas)
        {
            Generate_Tile(data.Key, _currentWorldMap.GenerateTile(data.Value));
        }
    }

    private void Set_MapEventsPrefab()
    {
        GameObject eventsPrefab = _currentWorldMap.worldMapEventsPrefab;
        
        if (eventsPrefab == null) return;
        _currentMapEventsPrefab = Instantiate(eventsPrefab, transform);
    }
    private void Set_Background()
    {
        InGame_Manager.instance.environmentVisuals.backgroundRenderer.Load_Renderer(_currentWorldMap.backgroundWarpRenderData);
    }
}