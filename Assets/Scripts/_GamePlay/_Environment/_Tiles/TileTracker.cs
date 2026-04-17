using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTracker : MonoBehaviour
{
    private const float _clampDistance = 0.7f;
    private const float _clampduration = 0.1f;
    
    private TileTrackerData _data;
    public TileTrackerData data => _data;

    public Action OnTrackUpdate;
    public Action<Tile> OnTileTrackUpdate;

    private Coroutine _clampCoroutine;
    public Coroutine clampCoroutine => _clampCoroutine;


    // Data
    public void Set_Data(Tile startingCurrentTile)
    {
        if (startingCurrentTile == null) return;
        _data = new(startingCurrentTile);
    }


    // Tracking
    private List<Tile> Peripheral_Tiles(Tile pivotTile)
    {
        if (pivotTile == null) return null;
        Vector2 pivotTilePos = pivotTile.transform.position;

        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        List<Vector2> peripheralPositions = Utility.Surrounding_Positions(pivotTilePos);
        List<Tile> peripheralTiles = new() { pivotTile };

        foreach (Vector2 position in peripheralPositions)
        {
            Tile peripheralTile = tilesController.Current_Tile(position);
            if (peripheralTile == null) continue;

            peripheralTiles.Add(peripheralTile);
        }
        return peripheralTiles;
    }

    public void TrackUpdate_CurrentTile()
    {
        Tile currentTile = _data.CurrentTile();
        if (currentTile == null) return;

        List<Tile> peripheralTiles = Peripheral_Tiles(currentTile);
        if (peripheralTiles.Count <= 0) return;

        Vector2 currentPos = transform.position;

        Tile closestTile = currentTile;
        float closestDistance = (currentPos - (Vector2)currentTile.transform.position).sqrMagnitude;

        for (int i = 0; i < peripheralTiles.Count; i++)
        {
            Tile checkTile = peripheralTiles[i];
            float checkDistance = (currentPos - (Vector2)checkTile.transform.position).sqrMagnitude;

            if (checkDistance >= closestDistance) continue;

            closestDistance = checkDistance;
            closestTile = checkTile;
        }

        if (closestTile == currentTile) return;
        _data.TrackTile(closestTile);

        OnTrackUpdate?.Invoke();
        OnTileTrackUpdate?.Invoke(closestTile);
    }


    public bool Inside_TileArea()
    {
        Tile currentTile = _data.CurrentTile();
        if (currentTile == null) return false;

        List<Tile> peripheralTiles = Peripheral_Tiles(currentTile);

        for (int i = 0; i < peripheralTiles.Count; i++)
        {
            float distance = Vector2.Distance(transform.position, peripheralTiles[i].transform.position);
            if (distance <= _clampDistance) return true;
        }
        return false;
    }
    
    public void ClampInside_CurrentTile()
    {
        Tile currentTile = data.CurrentTile();

        if (currentTile == null) return;
        if (Inside_TileArea()) return;
        
        LeanTween.move(gameObject, currentTile.transform.position, _clampduration);
        _clampCoroutine = StartCoroutine(ClampInside_StateUpdate());
    }
    private IEnumerator ClampInside_StateUpdate()
    {
        yield return new WaitForSeconds(_clampduration);
        _clampCoroutine = null;
    }
}