using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileIndicator_VisualData
{
    [SerializeField] private Sprite _indicatorSprite;
    public Sprite indicatorSprite => _indicatorSprite;

    [SerializeField] private Color _indicatorColor;
    public Color indicatorColor => _indicatorColor;
}

public class Tile_Indicator : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _indicatorPrefab;

    [Space(20)]
    [SerializeField] private TileIndicator_VisualData _defaultVisualData;

    [SerializeField] private TileIndicator_VisualData[] _visualDatas;
    public TileIndicator_VisualData[] visualDatas => _visualDatas;

    [Space(20)]
    [SerializeField] private Vector2[] _defaultTilePositions;
    public Vector2[] defaultTilePositions => _defaultTilePositions;


    private Dictionary<Tile, SpriteRenderer> _currentIndicateDatas = new();
    public Dictionary<Tile, SpriteRenderer> currentIndicateDatas => _currentIndicateDatas;


    // Main
    public List<Vector2> Available_DefaultPositions(Tile pivotTile)
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;
        List<Vector2> tilePositions = new();

        foreach (Vector2 position in _defaultTilePositions)
        {
            Tile checkTile = tilesController.Current_Tile((Vector2)pivotTile.transform.position + position);

            if (checkTile == null) continue;
            tilePositions.Add(position);
        }
        return tilePositions;
    }

    public List<Tile> Current_IndicateTiles()
    {
        List<Tile> currentTiles = new();

        foreach (var indicator in _currentIndicateDatas)
        {
            currentTiles.Add(indicator.Key);
        }
        return currentTiles;
    }


    public void Set_Indicator(Tile targetTile)
    {
        if (targetTile == null) return;
        if (currentIndicateDatas.ContainsKey(targetTile)) return;

        GameObject setIndicator = Instantiate(_indicatorPrefab, targetTile.transform);

        if (setIndicator.TryGetComponent(out SpriteRenderer sr) == false)
        {
            Destroy(setIndicator);
            return;
        }
        _currentIndicateDatas[targetTile] = sr;

        Sprite defaultSprite = _defaultVisualData.indicatorSprite;

        sr.sprite = defaultSprite != null ? defaultSprite : sr.sprite;
        sr.color = _defaultVisualData.indicatorColor;
    }

    public void Set_Indicators(Tile pivotTile, List<Vector2> setPositions)
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        for (int i = 0; i < setPositions.Count; i++)
        {
            Vector2 pivotTilePos = pivotTile.transform.position;
            Set_Indicator(tilesController.Current_Tile(pivotTilePos + setPositions[i]));
        }
    }
    public void Set_Indicators(Tile pivotTile)
    {
        List<Vector2> defaultPositions = new();

        foreach (Vector2 positions in _defaultTilePositions)
        {
            defaultPositions.Add(positions);
        }
        Set_Indicators(pivotTile, defaultPositions);
    }

    public void Clear_CurrentIndicators()
    {
        foreach (var indicator in _currentIndicateDatas)
        {
            Destroy(indicator.Value.gameObject);
        }
        _currentIndicateDatas.Clear();
    }


    public void Update_CurrentVisualDatas(TileIndicator_VisualData visualData)
    {
        if (_currentIndicateDatas.Count <= 0) return;

        foreach (var setIndicator in _currentIndicateDatas)
        {
            SpriteRenderer indicationRenderer = setIndicator.Value;

            indicationRenderer.sprite = visualData.indicatorSprite;
            indicationRenderer.color = visualData.indicatorColor;
        }
    }

    public void Toggle_CurrentIndicators(bool toggle)
    {
        if (_currentIndicateDatas.Count <= 0) return;

        foreach (var indication in _currentIndicateDatas)
        {
            indication.Value.gameObject.SetActive(toggle);
        }
    }
}