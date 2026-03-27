using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[System.Serializable]
public struct TileGenerate_ResoulationData
{
    [SerializeField] private Vector2 _maxGenerateSize;
    public Vector2 maxGenerateSize => _maxGenerateSize;

    [SerializeField] private Vector2 _resolution;
    public Vector2 resolution => _resolution;
}

[System.Serializable]
public class BackgroundRenderer_Data
{
    [Space(10)]
    [SerializeField][Range(0, 10)] private float _animationSpeed;
    public float animationSpeed => _animationSpeed;

    [SerializeField]private Color _colorA;
    public Color colorA => _colorA;

    [SerializeField] private Color _colorB;
    public Color colorB => _colorB;

    [SerializeField] private Color _colorC;
    public Color colorC => _colorC;
}

public class WorldMap_Generator : MonoBehaviour
{
    [Space(20)]
    [SerializeField] PixelPerfectCamera _pixelCamera;
    [SerializeField] TileGenerate_ResoulationData[] resolutionDatas;

    [Space(20)]
    [SerializeField] private Renderer _backgroundRenderer;
    [SerializeField] private BackgroundRenderer_Data _defaultBackgroundRendererData;

    [Space(20)]
    [SerializeField] private Transform _tileMapShadow;
    [SerializeField] private Vector3 _tileMapShadowOffset;

    [Space(20)]
    [SerializeField] private WorldMapScrObj _defaultWorldMap;


    private MaterialPropertyBlock _backgroundMaterialblock;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Generate_PresetTiles);
        EventBus_Manager.Register(EventBus.AwakeLoad, Generate_Tiles);
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_MapEventsPrefab);

        EventBus_Manager.Register(EventBus.AwakeLoad, Update_Resolution);
        EventBus_Manager.Register(EventBus.AwakeLoad, Load_BackgroundRenderer);
        EventBus_Manager.Register(EventBus.AwakeLoad, Load_TileMapShadow);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Generate_PresetTiles);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Generate_Tiles);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_MapEventsPrefab);

        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Update_Resolution);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Load_BackgroundRenderer);
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Load_TileMapShadow);
    }


    // Data
    public Vector2 Converted_GenerateSize()
    {
        Vector2 generateSize = _defaultWorldMap.generateSize;
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


    // Pre Load Data
    private List<TileType> DensityConverted_TileTypes(int convertCount)
    {
        List<TileType> tileTypes = new();

        for (int i = 0; i < convertCount; i++)
        {
            bool isHarshGround = _defaultWorldMap.harshGroundDensity > UnityEngine.Random.Range(0, 100);
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


    // Generate
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
        Tile_PresetDatas[] presetTileDatas = _defaultWorldMap.presetTileDatas;
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
            Generate_Tile(data.Key, dataManager.TileScrObj(data.Value));
        }
    }


    private void Set_MapEventsPrefab()
    {
        GameObject eventsPrefab = _defaultWorldMap.worldMapEventsPrefab;
        
        if (eventsPrefab == null) return;
        Instantiate(eventsPrefab, transform);
    }


    // Camera
    private void Update_Resolution()
    {
        for (int i = 0; i < resolutionDatas.Length; i++)
        {
            TileGenerate_ResoulationData resData = resolutionDatas[i];

            Vector2 maxSize = resData.maxGenerateSize;
            Vector2 generateSize = _defaultWorldMap.generateSize;
            
            if (generateSize.x > maxSize.x || generateSize.y > maxSize.y) continue;

            _pixelCamera.refResolutionX = (int)resData.resolution.x;
            _pixelCamera.refResolutionY = (int)resData.resolution.y;
            
            return;
        }
    }


    // Environment Visuals
    private void Load_BackgroundRenderer(BackgroundRenderer_Data loadData)
    {
        _backgroundRenderer.GetPropertyBlock(_backgroundMaterialblock);

        _backgroundMaterialblock.SetFloat("_Speed", loadData.animationSpeed);
        _backgroundMaterialblock.SetColor("_ColorA", loadData.colorA);
        _backgroundMaterialblock.SetColor("_ColorB", loadData.colorB);
        _backgroundMaterialblock.SetColor("_ColorC", loadData.colorC);

        _backgroundRenderer.SetPropertyBlock(_backgroundMaterialblock);
    }
    private void Load_BackgroundRenderer()
    {
        _backgroundMaterialblock = new MaterialPropertyBlock();
        Load_BackgroundRenderer(_defaultBackgroundRendererData);
    }

    private void Load_TileMapShadow()
    {
        Vector2 currentMapSize = Converted_GenerateSize();
        Vector3 shadowScale = _tileMapShadow.localScale;

        shadowScale.x = currentMapSize.x;
        shadowScale.y = currentMapSize.y;

        _tileMapShadow.localScale = shadowScale;
        _tileMapShadow.position += _tileMapShadowOffset;
    }
}