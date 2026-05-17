using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour, IDamageable
{
    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;

    [Space(20)]
    [SerializeField] private Tile_Indicator _tileIndicator;
    public Tile_Indicator tileIndicator => _tileIndicator;

    [SerializeField] private AnimationPlayer _animation;
    [SerializeField] private FillBar_Controller _healthFillBar;

    [Space(10)]
    [SerializeField] private GameObject _alertIcon;

    [Space(20)]
    [SerializeField] private AnimationClipScrObj _deceasedAnimationClip;
    [SerializeField] private ItemData[] _deceasedDropItems;

    [Space(20)]
    [SerializeField] private AnimalAction[] _onTimeCountActions;
    [SerializeField] private AnimalAction[] _onAgroActions;


    private AnimalData _data;
    public AnimalData data => _data;

    private Coroutine _runActionCoroutine;


    // MonoBehaviour
    private void OnDestroy()
    {
        _movement.OnMovementState -= Update_Animation;
        _movement.OnMovementDirection -= _animation.Update_Flip;

        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileHover -= Toggle_HealthBar;
        tilesController.OnTileHover -= Toggle_AlertIcon;
        tilesController.OnTileHover -= Toggle_MovementRangeTiles;

        TileTracker playerTracker = manager.player.movement.tileTracker;

        playerTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Collect_TrailMark);
        playerTracker.UnRegister(ActionUpdateBus.AwakeUpdate, Run_AgroActions);

        playerTracker.UnRegister(ActionUpdateBus.AwakeUpdate, HealthBar_Toggle);
        playerTracker.UnRegister(ActionUpdateBus.AwakeUpdate, AlertIcon_Toggle);

        Time_Manager time = manager.time;

        time.UnRegister(ActionUpdateBus.AwakeUpdate, Run_TimeCountActions);

        time.UnRegister(ActionUpdateBus.AwakeUpdate, HealthBar_Toggle);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, AlertIcon_Toggle);
    }


    // IDamageable
    public bool IsDamageable()
    {
        return _data.isOnSight && Deceased() == false;
    }

    public int InflictDamage(int damageValue)
    {
        if (_data.isOnSight == false) return 0;

        int actualDamageValue = Mathf.Min(damageValue, _data.health);

        _data.Update_Health(_data.health - actualDamageValue);
        _healthFillBar.Update_CurrentBarFill(_data.animalScrObj.maxHealth, _data.health);

        if (_data.health <= 0)
        {
            Update_DeceasedState();
            return actualDamageValue;
        }

        _animation.Play(2);
        Run_AgroActions(null);

        return actualDamageValue;
    }


    // Data
    public void Set_Data()
    {
        _movement.OnMovementState += Update_Animation;
        _movement.OnMovementDirection += _animation.Update_Flip;

        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        tilesController.OnTileHover += Toggle_HealthBar;
        tilesController.OnTileHover += Toggle_AlertIcon;
        tilesController.OnTileHover += Toggle_MovementRangeTiles;

        TileTracker playerTracker = manager.player.movement.tileTracker;

        playerTracker.Register(ActionUpdateBus.AwakeUpdate, Collect_TrailMark);
        playerTracker.Register(ActionUpdateBus.AwakeUpdate, Run_AgroActions);

        playerTracker.Register(ActionUpdateBus.AwakeUpdate, HealthBar_Toggle);
        playerTracker.Register(ActionUpdateBus.AwakeUpdate, AlertIcon_Toggle);

        Time_Manager time = manager.time;

        time.Register(ActionUpdateBus.AwakeUpdate, Run_TimeCountActions);

        time.Register(ActionUpdateBus.AwakeUpdate, HealthBar_Toggle);
        time.Register(ActionUpdateBus.AwakeUpdate, AlertIcon_Toggle);
    }
    public void Set_Data(AnimalScrObj setAnimal)
    {
        Transform currentTilePos = movement.tileTracker.data.CurrentTile().transform;
        Transform playerTilePos = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile().transform;

        int health = _data == null ? setAnimal.maxHealth : _data.health;

        int distanceFromPlayer = Utility.Chebyshev_Distance(currentTilePos.position, playerTilePos.position);
        int randCollectCount = UnityEngine.Random.Range(1, distanceFromPlayer + 1);

        _data = new(setAnimal, health, randCollectCount);

        _healthFillBar.Refresh_CurrentFillBar();
        _movement.Stop();
    }

    public void Update_Animation(bool isMoving)
    {
        if (_data.isOnSight == false)
        {
            _animation.Play(0);
            return;
        }

        if (isMoving)
        {
            _animation.Play(1);
            return;
        }
        _animation.Stop();
    }


    private Animals_Manager AnimalManager()
    {
        GameObject eventsPrefab = InGame_Manager.instance.worldMapGenerator.currentMapEventsPrefab;

        if (eventsPrefab.TryGetComponent(out Animals_Manager manager) == false) return null;
        return manager;
    }

    public List<Tile> MoveDistance_RangeTiles(int maxDistance)
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        Tile currentTile = _movement.tileTracker.data.CurrentTile();
        maxDistance = Mathf.Clamp(maxDistance, 1, _data.animalScrObj.moveDistance);

        return TilePatterns_Utility.PivotDistanced_Tiles(currentTile, maxDistance);
    }
    public List<Tile> MoveDistance_RangeTiles()
    {
        return MoveDistance_RangeTiles(_data.animalScrObj.moveDistance);
    }

    public Tile MoveDistanceRange_RandomTile(bool excludePlayerTile)
    {
        List<Tile> rangedTiles = MoveDistance_RangeTiles();
        rangedTiles.Remove(_movement.tileTracker.data.CurrentTile());

        if (excludePlayerTile) rangedTiles.Remove(InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile());
        return rangedTiles[UnityEngine.Random.Range(0, rangedTiles.Count)];
    }

    public List<Tile> AgroRange_Tiles()
    {
        Tile currentTile = _movement.tileTracker.data.CurrentTile();
        return TilePatterns_Utility.PivotDistanced_Tiles(currentTile, _data.animalScrObj.agroRange);
    }
    public bool Player_InAgroRange()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        return _movement.tileTracker.data.CurrentTile().DistanceTo_TargetTile(playerTile) <= _data.animalScrObj.agroRange;
    }

    public bool Deceased()
    {
        return _data.health <= 0 || AnimalManager().spawnedAnimals.Contains(this) == false;
    }


    // Visuals
    private void Toggle_HealthBar(Tile hoveringTile)
    {
        bool toggle = hoveringTile == _movement.tileTracker.data.CurrentTile();
        _healthFillBar.Toggle(toggle);

        if (toggle == false) return;
        _healthFillBar.Update_CurrentBarFill(_data.animalScrObj.maxHealth, _data.health);
    }

    private void HealthBar_Toggle()
    {
        Toggle_HealthBar(InGame_Manager.instance.cursor.pointingTile);
    }
    private void HealthBar_Toggle(Tile _)
    {
        HealthBar_Toggle();
    }


    private void Toggle_AlertIcon(bool toggle)
    {
        InGame_Manager manager = InGame_Manager.instance;

        Tile currentTile = _movement.tileTracker.data.CurrentTile();
        List<Tile> alertTiles = TilePatterns_Utility.PivotDistanced_Tiles(currentTile, _data.animalScrObj.agroRange + 1);

        bool playerAlerted = alertTiles.Contains(manager.player.movement.tileTracker.data.CurrentTile());
        _alertIcon.SetActive(toggle && _data.isOnSight && _data.health > 0 && playerAlerted);
    }
    private void Toggle_AlertIcon(Tile hoveringTile)
    {
        Toggle_AlertIcon(hoveringTile == _movement.tileTracker.data.CurrentTile());
    }

    private void AlertIcon_Toggle()
    {
        Toggle_AlertIcon(InGame_Manager.instance.cursor.pointingTile);
    }
    private void AlertIcon_Toggle(Tile _)
    {
        AlertIcon_Toggle();
    }


    private void Update_MovementRangeTiles()
    {
        _tileIndicator.Clear_CurrentIndicators();

        List<Tile> movementTiles = MoveDistance_RangeTiles();
        Tile currentTile = _movement.tileTracker.data.CurrentTile();

        foreach (Tile tile in movementTiles)
        {
            if (tile == currentTile) continue;
            _tileIndicator.Set_Indicator(tile);
        }
        Toggle_MovementRangeTiles(InGame_Manager.instance.cursor.pointingTile);
    }
    private void Toggle_MovementRangeTiles(Tile hoveringTile)
    {
        bool toggle = hoveringTile == _movement.tileTracker.data.CurrentTile();
        _tileIndicator.Toggle_CurrentIndicators(toggle);
    }


    // State Updates
    private Tile TrailMark_CollectTile()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        List<Tile> rangeTiles = MoveDistance_RangeTiles(_data.animalScrObj.agroRange + 1);

        for (int i = rangeTiles.Count - 1; i >= 0; i--)
        {
            Tile checkTile = rangeTiles[i];
            if (checkTile == playerTile)
            {
                rangeTiles.RemoveAt(i);
                continue;
            }

            List<GameObject> prefabs = checkTile.All_CurrentPrefabs();
            for (int j = 0; j < prefabs.Count; j++)
            {
                if (prefabs[j].TryGetComponent(out Animal animal) == false) continue;
                if (animal.data.isOnSight) continue;

                rangeTiles.RemoveAt(i);
                break;
            }
        }
        return rangeTiles[UnityEngine.Random.Range(0, rangeTiles.Count)];
    }

    private void Collect_TrailMark(Tile collectTile)
    {
        bool isOnSight = _data.isOnSight;
        if (isOnSight) return;

        if (collectTile != _movement.tileTracker.data.CurrentTile()) return;
        _data.Decrease_TrailMarkCount(1);

        Tile updateTile = TrailMark_CollectTile();
        if (updateTile == null) return;

        transform.position = isOnSight ? updateTile.Random_BoundPoint() : updateTile.transform.position;
        _movement.tileTracker.TrackUpdate_CurrentTile(updateTile);

        Update_OnSight();
    }
    private void Update_OnSight()
    {
        if (_data.isOnSight == false) return;

        Update_Animation(false);

        _healthFillBar.Set_FillBar(transform);
        _healthFillBar.Toggle(false);

        Toggle_AlertIcon(InGame_Manager.instance.cursor.pointingTile);
        Update_MovementRangeTiles();
    }


    private void Reset_ActionsUpdate()
    {
        if (_runActionCoroutine == null) return;

        StopCoroutine(_runActionCoroutine);
        _runActionCoroutine = null;
    }

    private void Run_TimeCountActions()
    {
        if (Deceased()) return;
        if (_data.isOnSight == false) return;

        Reset_ActionsUpdate();
        _tileIndicator.Clear_CurrentIndicators();

        _runActionCoroutine = StartCoroutine(TimeCountActions_Update());
    }
    private IEnumerator TimeCountActions_Update()
    {
        while (_animation.Animation_Playing()) yield return null;

        foreach (AnimalAction animalAction in _onTimeCountActions)
        {
            animalAction.Run_Action();
            while (animalAction.actionRunning) yield return null;
        }

        HealthBar_Toggle();
        AlertIcon_Toggle();
        Update_MovementRangeTiles();

        Reset_ActionsUpdate();
    }

    private void Run_AgroActions(Tile _)
    {
        if (Deceased()) return;
        if (_data.isOnSight == false) return;
        if (Player_InAgroRange() == false) return;

        Reset_ActionsUpdate();
        _tileIndicator.Clear_CurrentIndicators();

        _runActionCoroutine = StartCoroutine(AgroActions_Update());
    }
    private IEnumerator AgroActions_Update()
    {
        while (_animation.Animation_Playing()) yield return null;

        foreach (AnimalAction animalAction in _onAgroActions)
        {
            animalAction.Run_Action();
            while (animalAction.actionRunning) yield return null;
        }

        HealthBar_Toggle();
        AlertIcon_Toggle();
        Update_MovementRangeTiles();

        Reset_ActionsUpdate();
    }

    public void Update_DeceasedState()
    {
        if (_data.health > 0) return;

        Reset_ActionsUpdate();

        _healthFillBar.Refresh_CurrentFillBar();
        _tileIndicator.Clear_CurrentIndicators();

        Toggle_AlertIcon(false);
        _runActionCoroutine = StartCoroutine(DeceasedState_Update());
    }
    private IEnumerator DeceasedState_Update()
    {
        _animation.Play(_deceasedAnimationClip);
        while (_animation.Animation_Playing(_deceasedAnimationClip)) yield return null;

        Tile currentTile = _movement.tileTracker.data.CurrentTile();
        foreach (ItemData itemData in _deceasedDropItems)
        {
            currentTile.SetPreserve_Item(new(itemData.itemScrObj, itemData.amount));
        }
        
        _runActionCoroutine = null;

        AnimalManager().spawnedAnimals.Remove(this);
        Destroy(gameObject);
    }
}