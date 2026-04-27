using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Animal : MonoBehaviour, IDamageable
{
    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;

    [Space(20)]
    [SerializeField] private EventPointer _eventPointer;
    [SerializeField] private Tile_Indicator _tileIndicator;

    [Space(20)]
    [SerializeField] private AnimationPlayer _animation;
    [SerializeField] private FillBar_Controller _healthFillBar;

    [Space(20)]
    [SerializeField] private AnimationClipScrObj _deceasedAnimationClip;
    [SerializeField] private ItemData[] _deceasedDropItems;

    [Space(20)]
    [SerializeField] private AnimalAction[] _onTimeCountActions;
    [SerializeField] private AnimalAction[] _onAgroActions;
    [SerializeField] private AnimalAction[] _onDamageInflictedActions;


    private AnimalData _data;
    public AnimalData data => _data;


    // MonoBehaviour
    private void OnDestroy()
    {
        _movement.OnMovementState -= Update_Animation;
        _movement.OnMovementDirection -= _animation.Update_Flip;

        _eventPointer.OnPointerState -= Toggle_HealthBar;
        // _eventPointer.OnPointerState -= _tileIndicator.Toggle_CurrentIndicators;

        InGame_Manager manager = InGame_Manager.instance;
        TileTracker playerTracker = InGame_Manager.instance.player.movement.tileTracker;

        playerTracker.OnTileTrackUpdate -= Collect_TrailMark;
        playerTracker.OnTileTrackUpdate -= Run_AgroActions;

        InGame_Manager.instance.time.UnRegister(TimeUpdateBus.AwakeUpdate, Run_TimeCountActions);
    }


    // IDamageable
    public int InflictDamage(int damageValue)
    {
        if (_data.isOnSight == false) return _data.health;

        _data.Update_Health(_data.health - damageValue);
        Update_DeceasedState();

        if (_data.health <= 0) return _data.health;

        foreach (AnimalAction animalAction in _onDamageInflictedActions)
        {
            if (animalAction.RunAction()) break;
        }
        return _data.health;
    }


    // Data
    public void Set_Data()
    {
        _movement.OnMovementState += Update_Animation;
        _movement.OnMovementDirection += _animation.Update_Flip;

        _eventPointer.OnPointerState += Toggle_HealthBar;
        // _eventPointer.OnPointerState += _tileIndicator.Toggle_CurrentIndicators;

        InGame_Manager manager = InGame_Manager.instance;
        TileTracker playerTracker = InGame_Manager.instance.player.movement.tileTracker;

        playerTracker.OnTileTrackUpdate += Collect_TrailMark;
        playerTracker.OnTileTrackUpdate += Run_AgroActions;

        InGame_Manager.instance.time.Register(TimeUpdateBus.AwakeUpdate, Run_TimeCountActions);
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

    public List<Tile> MoveDistance_RangeTiles()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        Tile currentTile = _movement.tileTracker.data.CurrentTile();
        int distanceRange = _data.animalScrObj.moveDistance;

        List<Tile> rangedTiles = tilesController.Current_Tiles(currentTile, distanceRange);
        rangedTiles.Remove(distanceRange > 0 ? currentTile : null);

        return rangedTiles;
    }
    public Tile MoveDistance_RangeTile(bool excludePlayerTile)
    {
        List<Tile> rangedTiles = MoveDistance_RangeTiles();
        if (excludePlayerTile) rangedTiles.Remove(InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile());

        return rangedTiles[UnityEngine.Random.Range(0, rangedTiles.Count)];
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
    private void Toggle_HealthBar(bool toggle)
    {
        _healthFillBar.Toggle(toggle);

        if (toggle == false) return;
        _healthFillBar.Update_CurrentBarFill(_data.animalScrObj.maxHealth, _data.health);
    }


    // State Updates
    private void Collect_TrailMark(Tile collectTile)
    {
        if (_data.isOnSight) return;
        if (collectTile != _movement.tileTracker.data.CurrentTile()) return;

        _data.Decrease_TrailMarkCount(1);

        Tile nextTile = MoveDistance_RangeTile(true);
        if (nextTile == null) return;

        bool isOnSight = _data.isOnSight;

        transform.position = isOnSight ? nextTile.Random_BoundPoint() : nextTile.transform.position;
        _movement.tileTracker.data.TrackTile(nextTile);

        if (isOnSight == false) return;

        Update_Animation(false);

        _healthFillBar.Set_FillBar(transform);
        _healthFillBar.Toggle(false);

        List<Tile> nextMoveTiles = MoveDistance_RangeTiles();
        foreach (Tile tile in nextMoveTiles)
        {
            _tileIndicator.Set_Indicator(tile);
        }
        _tileIndicator.Toggle_CurrentIndicators(_eventPointer.pointerDetected);
    }

    private void Run_TimeCountActions()
    {
        if (_data.isOnSight == false) return;

        InGame_Manager.instance.time.timeUpdateActions.Add(this);
        StartCoroutine(TimeCountActions_Update());
    }
    private IEnumerator TimeCountActions_Update()
    {
        if (Player_InAgroRange())
        {
            StartCoroutine(AgroActions_Update());
            yield break;
        }

        foreach (AnimalAction animalAction in _onTimeCountActions)
        {
            if (animalAction.RunAction() == false) continue;
            while (animalAction.actionRunning) yield return null;
        }

        if (Player_InAgroRange() == false)
        {
            InGame_Manager.instance.time.timeUpdateActions.Remove(this);
            yield break;
        }
        StartCoroutine(AgroActions_Update());
    }

    private void Run_AgroActions(Tile _)
    {
        if (_data.isOnSight == false) return;
        if (Player_InAgroRange() == false) return;

        InGame_Manager.instance.time.timeUpdateActions.Add(this);
        StartCoroutine(AgroActions_Update());
    }
    private IEnumerator AgroActions_Update()
    {
        foreach (AnimalAction animalAction in _onAgroActions)
        {
            if (animalAction.RunAction() == false) continue;
            while (animalAction.actionRunning) yield return null;
        }

        InGame_Manager.instance.time.timeUpdateActions.Remove(this);
        yield break;
    }

    public void Update_DeceasedState()
    {

    }
    private IEnumerator DeceasedState_Update()
    {
        yield break;
    }
}