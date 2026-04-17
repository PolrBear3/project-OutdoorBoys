using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileTrackerData
{
    private List<Tile> _trackingTiles = new();
    public List<Tile> trackingTiles => _trackingTiles;

    private const int _maxTrackCount = 10;


    public TileTrackerData(Tile startingTile)
    {
        _trackingTiles.Add(startingTile);
    }


    public Tile CurrentTile()
    {
        int trackTileCount = _trackingTiles.Count;

        if (trackTileCount <= 0) return null;
        return _trackingTiles[trackTileCount - 1];
    }

    public Tile PreviousTile()
    {
        int trackTileCount = _trackingTiles.Count;

        if (trackTileCount <= 0) return null;
        if (trackTileCount <= 1) return _trackingTiles[0];
        return _trackingTiles[trackTileCount - 2];
    }


    public Tile TrackTile(Tile trackTile)
    {
        Tile currentTile = CurrentTile();
        if (currentTile != null && currentTile == trackTile) return currentTile;
        
        _trackingTiles.Add(trackTile);

        if (_trackingTiles.Count <= _maxTrackCount) return trackTile;
        _trackingTiles.RemoveAt(0);

        return trackTile;
    }
}
