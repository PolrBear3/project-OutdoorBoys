using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Tile_Generator : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Vector2 _generateSize;
    public Vector2 generateSize => _generateSize;

    [SerializeField][Range(0, 100)] private float _harshGroundDensity;


    private List<Tile> _generatedTiles = new();
    public List<Tile> generatedTiles => _generatedTiles;


    // MonoBehaviour
    void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Generate_Tiles);
        EventBus_Manager.Register(EventBus.StartLoad, Update_TileSetSprite);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Generate_Tiles);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Update_TileSetSprite);
    }


    // Pre Data Load
    private List<TileType> DensityConverted_TileTypes(int convertCount)
    {
        List<TileType> tileTypes = new();

        for (int i = 0; i < convertCount; i++)
        {
            bool isHarshGround = _harshGroundDensity > UnityEngine.Random.Range(0, 100);
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
    private List<Vector2> Generate_Positions()
    {
        List<Vector2> positions = new();

        Vector2 convertedSize = new(Mathf.RoundToInt(_generateSize.x), Mathf.RoundToInt(_generateSize.y));

        float xStartingPoint = -(convertedSize.x - 1) / 2;
        float yStartingPoint = (convertedSize.y - 1) / 2;

        int generateCount = Mathf.RoundToInt(convertedSize.x * convertedSize.y);
        int horizontalCount = 0;

        for (int i = 0; i < generateCount; i++)
        {
            positions.Add(new(xStartingPoint + horizontalCount, yStartingPoint));
            horizontalCount++;

            if (horizontalCount < convertedSize.x) continue;

            horizontalCount = 0;
            yStartingPoint--;
        }

        return positions;
    }

    private void Generate_Tiles()
    {
        Data_Manager manager = Data_Manager.instance;
        Dictionary<Vector2, TileType> generateDatas = Iterated_TileDatas();

        foreach (var data in generateDatas)
        {
            GameObject tilePrefab = manager.TileScrObj(data.Value).prefab;
            GameObject generatedTile = Instantiate(tilePrefab, data.Key, quaternion.identity);

            if (!generatedTile.TryGetComponent(out Tile tile))
            {
                Debug.Log("Tile Script Not Attached!");
                return;
            }

            generatedTile.transform.SetParent(transform);
            _generatedTiles.Add(tile);

            tile.Set_Data(manager.TileScrObj(data.Value));
        }
    }


    // Generated Tiles
    private void Update_TileSetSprite()
    {
        for (int i = 0; i < _generatedTiles.Count; i++)
        {
            bool setOnBase = _generatedTiles[i].transform.position.y <= -((_generateSize.y - 1) / 2);
            _generatedTiles[i].Update_SetSprite(setOnBase);
        }
    }
}