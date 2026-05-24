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

    private Dictionary<ActionUpdateBus, Action<Tile>> _trackUpdateBus = new();

    private Coroutine _clampCoroutine;
    public Coroutine clampCoroutine => _clampCoroutine;


    // Data
    public void Set_Data(Tile startingCurrentTile)
    {
        if (startingCurrentTile == null) return;

        _data = new(startingCurrentTile);
        startingCurrentTile.Set_CurrentPrefab(gameObject);
    }


    // Update Bus
    public void Register(ActionUpdateBus updateBus, Action<Tile> targetAction)
    {
        if (_trackUpdateBus.ContainsKey(updateBus) == false)
        {
            _trackUpdateBus.Add(updateBus, targetAction);
            return;
        }
        _trackUpdateBus[updateBus] += targetAction;
    }
    public void UnRegister(ActionUpdateBus updateBus, Action<Tile> targetAction)
    {
        _trackUpdateBus[updateBus] -= targetAction;
    }

    private void RunRegistered_UpdateBus(Tile updateTile)
    {
        for (int i = 0; i < _trackUpdateBus.Count; i++)
        {
            ActionUpdateBus runBus = (ActionUpdateBus)i;

            if (_trackUpdateBus.TryGetValue(runBus, out Action<Tile> action) == false) continue;
            action?.Invoke(updateTile);
        }
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
        targetTile.Set_CurrentPrefab(gameObject);

        RunRegistered_UpdateBus(targetTile);
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
        closestTile.Set_CurrentPrefab(gameObject);

        Dictionary<TileState, int> stateDatas = closestTile.data.stateDatas;
        foreach (var stateData in stateDatas)
        {
            Debug.Log(stateData.Key + " " + stateData.Value);
        }

        RunRegistered_UpdateBus(closestTile);
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