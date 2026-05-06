using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class TileTracker : MonoBehaviour
{
    private const float _clampDistance = 0.7f;
    private const float _clampduration = 0.5f;

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
    public bool Inside_TileArea()
    {
        Tile currentTile = _data.CurrentTile();
        if (currentTile == null) return false;

        List<Tile> edgedTiles = InGame_Manager.instance.tilesController.Current_EdgedTiles();
        if (edgedTiles.Contains(currentTile) == false) return true;

        List<Tile> peripheralTiles = Peripheral_Tiles(currentTile);

        for (int i = 0; i < peripheralTiles.Count; i++)
        {
            float distance = Vector2.Distance(transform.position, peripheralTiles[i].transform.position);
            if (distance <= _clampDistance) return true;
        }
        return false;
    }

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

    public Tile TrackUpdate_CurrentTile(Tile targetTile)
    {
        if (targetTile == null) return _data?.CurrentTile();

        _data.TrackTile(targetTile);
        gameObject.transform.SetParent(targetTile.setPosition);

        OnTrackUpdate?.Invoke();
        OnTileTrackUpdate?.Invoke(targetTile);

        return targetTile;
    }
    public Tile TrackUpdate_CurrentTile()
    {
        Tile currentTile = _data?.CurrentTile();
        if (currentTile == null) return null;

        List<Tile> peripheralTiles = Peripheral_Tiles(currentTile);
        if (peripheralTiles.Count <= 0) return currentTile;

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

        if (currentTile == closestTile) return currentTile;

        _data.TrackTile(closestTile);
        gameObject.transform.SetParent(closestTile.setPosition);

        OnTrackUpdate?.Invoke();
        OnTileTrackUpdate?.Invoke(closestTile);

        return closestTile;
    }


    public void Clamp_toTile(Tile targetTile)
    {
        if (targetTile == null) return;
        if (_clampCoroutine != null) return;

        LeanTween.move(gameObject, targetTile.transform.position, _clampduration).setEase(LeanTweenType.easeOutElastic);
        _clampCoroutine = StartCoroutine(ClampInside_StateUpdate());
    }
    public void Clamp_toCurrentTile()
    {
        Clamp_toTile(data.CurrentTile());
    }
    
    private IEnumerator ClampInside_StateUpdate()
    {
        yield return new WaitForSeconds(_clampduration);
        _clampCoroutine = null;
    }
} 