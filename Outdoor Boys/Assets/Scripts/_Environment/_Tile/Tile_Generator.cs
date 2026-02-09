using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Tile_Generator : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _tilePrefab;
    
    [SerializeField] private Vector2 _generateSize;
    public Vector2 generateSize => _generateSize;

    [Space(10)]
    [SerializeField][Range(0, 100)] private float _harshGroundDensity;


    private List<Tile> _generatedTiles = new();
    public List<Tile> generatedTiles => _generatedTiles;


    // MonoBehaviour
    private void Start()
    {
        Generate_Tiles();
        Update_TileSetSprite();
    }


    // Pre Data Load
    private List<TileType> DensityConverted_TileTypes()
    {
        Vector2 convertedSize = new(Mathf.RoundToInt(_generateSize.x), Mathf.RoundToInt(_generateSize.y));
        int generateCount = Mathf.RoundToInt(convertedSize.x * convertedSize.y);
        
        List<TileType> tileTypes = new();
        
        for (int i = 0; i < generateCount; i++)
        {
            // randomize from _harshGroundDensity and .Add
        }

        return tileTypes;
    }

    private Dictionary<Vector2, TileType> TileGenerateDatas()
    {
        List<TileType> tileTypes = DensityConverted_TileTypes();

        // get all generate positions
        // set tile types

        // iterate Cellular Automata

        return null;
    }


    // Generate
    private void Generate_Tiles()
    {
        Vector2 convertedSize = new(Mathf.RoundToInt(_generateSize.x), Mathf.RoundToInt(_generateSize.y));
        
        float xStartingPoint = -(convertedSize.x - 1) / 2;
        float yStartingPoint = (convertedSize.y - 1) / 2;
        
        int generateCount = Mathf.RoundToInt(convertedSize.x * convertedSize.y);
        int horizontalCount = 0;

        for (int i = 0; i < generateCount; i++)
        {
            Vector2 generatePos = new(xStartingPoint + horizontalCount, yStartingPoint);
            GameObject tilePrefab = Instantiate(_tilePrefab, generatePos, quaternion.identity);
            
            if (!tilePrefab.TryGetComponent(out Tile tile))
            {
                Debug.Log("Tile Script Not Attached!");
                return;
            }

            tilePrefab.transform.SetParent(transform);
            _generatedTiles.Add(tile);
            
            horizontalCount ++;

            if (horizontalCount < convertedSize.x) continue;
            
            horizontalCount = 0;
            yStartingPoint--;
        }
    }

    private void Update_TileSetSprite()
    {
        for (int i = 0; i < _generatedTiles.Count; i++)
        {
            bool setOnBase = _generatedTiles[i].transform.position.y <= -((_generateSize.y - 1) / 2);
            _generatedTiles[i].Update_SetSprite(setOnBase);
        }
    }
}