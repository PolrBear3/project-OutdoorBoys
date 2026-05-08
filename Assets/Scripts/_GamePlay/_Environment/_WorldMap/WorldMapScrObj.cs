using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New World Map")]
public class WorldMapScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private GameObject _worldMapEventsPrefab;
    public GameObject worldMapEventsPrefab => _worldMapEventsPrefab;

    [Space(20)]
    [SerializeField] private Vector2 _generateSize;
    public Vector2 generateSize => _generateSize;

    [Space(20)]
    [SerializeField] private TileScrObj[] _generateTiles;
    public TileScrObj[] _generateTile => _generateTiles;

    [SerializeField] private Tile_PresetDatas[] _presetTileDatas;
    public Tile_PresetDatas[] presetTileDatas => _presetTileDatas;

    [Space(20)]
    [SerializeField][Range(0, 100)] private float _harshGroundDensity;
    public float harshGroundDensity => _harshGroundDensity;

    [Space(20)]
    [SerializeField] private WarpRenderer_Data _backgroundWarpRenderData;
    public WarpRenderer_Data backgroundWarpRenderData => _backgroundWarpRenderData;


    // Generate Tiles
    public List<TileScrObj> GenerateTiles(TileType tileType)
    {
        List<TileScrObj> tiles = new();

        for (int i = 0; i < _generateTiles.Length; i++)
        {
            if (_generateTiles[i].type != tileType) continue;
            tiles.Add(_generateTiles[i]);
        }
        if (tiles.Count <= 0)
        {
            foreach (TileScrObj tile in _generateTiles)
            {
                tiles.Add(tile);
            }
        }

        return tiles;
    }

    public TileScrObj GenerateTile(TileType tileType)
    {
        List<TileScrObj> tiles = GenerateTiles(tileType);
        int randIndex = Random.Range(0, tiles.Count);

        if (tiles.Count <= 0) return null;
        return tiles[randIndex];
    }
}