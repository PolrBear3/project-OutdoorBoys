using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Animal_ActionKeys
{
    public const string Roam = nameof(Roam);
    public const string Escape = nameof(Escape);
}

public class Animal : MonoBehaviour, IDamageable
{
    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;

    [Space(20)]
    [SerializeField] private AnimationPlayer _animation;
    [SerializeField] private FillBar_Controller _healthFillBar;

    [Space(20)]
    [SerializeField] private AnimationClipScrObj _escapeAnimationClip;
    [SerializeField] private AnimationClipScrObj _deceasedAnimationClip;

    [Space(20)]
    [SerializeField] private ItemData[] _dropItems;
    [SerializeField] private Item_ScrObj[] _followItems;


    private AnimalData _data;
    public AnimalData data => _data;

    private Coroutine _actionCoroutine;
    private Dictionary<string, int> _actionCountDatas = new();

    public UnityEvent OnSightActions;


    // MonoBehaviour
    private void OnDestroy()
    {
        _movement.OnMovementState += Update_Animation;
        _movement.OnMovementDirection += _animation.Update_Flip;

        InGame_Manager manager = InGame_Manager.instance;

        manager.tilesController.OnTileHover -= Toggle_FillBar;
        // _movement.OnMovement -= Toggle_FillBar;

        Time_Manager time = manager.time;

        time.UnRegister(TimeUpdateBus.AwakeUpdate, Collect_TrailMark);
        time.UnRegister(TimeUpdateBus.AwakeUpdate, Update_OnSight);
    }


    // IDamageable
    public int InflictDamage(int damageValue)
    {
        if (_data.isOnSight == false) return _data.health;

        _data.Update_Health(_data.health - damageValue);
        Update_DeceasedState();

        return _data.health;
    }


    // Data
    public void Set_Data()
    {
        _movement.OnMovementState += Update_Animation;
        _movement.OnMovementDirection += _animation.Update_Flip;

        InGame_Manager manager = InGame_Manager.instance;

        manager.tilesController.OnTileHover += Toggle_FillBar;
        // _movement.OnMovement += Toggle_FillBar;

        Time_Manager time = manager.time;

        time.Register(TimeUpdateBus.AwakeUpdate, Collect_TrailMark);
        time.Register(TimeUpdateBus.AwakeUpdate, Update_OnSight);
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

    private List<Tile> MoveDistance_RangeTiles()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        Tile currentTile = _movement.tileTracker.data.CurrentTile();
        int distanceRange = _data.animalScrObj.moveDistanceRange;

        List<Tile> rangedTiles = tilesController.Current_Tiles(currentTile, distanceRange);
        rangedTiles.Remove(distanceRange > 0 ? currentTile : null);

        return rangedTiles;
    }
    private Tile MoveDistance_RangeTile(bool excludePlayerTile)
    {
        List<Tile> rangedTiles = MoveDistance_RangeTiles();
        if (excludePlayerTile) rangedTiles.Remove(InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile());

        return rangedTiles[UnityEngine.Random.Range(0, rangedTiles.Count)];
    }


    private bool Player_InRange()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        float distance = 0f; // playerTile.DistanceTo_TargetTile(_movement.tileTrackerData.CurrentTile());

        return distance <= _data.animalScrObj.moveDistanceRange;
    }

    public bool Deceased()
    {
        return _data.health <= 0 || AnimalManager().spawnedAnimals.Contains(this) == false;
    }


    // Visuals
    private void Toggle_FillBar(Tile hoveringTile)
    {
        bool toggle = hoveringTile == _movement.tileTracker.data.CurrentTile();
        _healthFillBar.Toggle(toggle);

        if (toggle == false) return;
        _healthFillBar.Update_CurrentBarFill(_data.animalScrObj.maxHealth, _data.health);
    }
    private void Toggle_FillBar()
    {
        Toggle_FillBar(InGame_Manager.instance.cursor.pointingTile);
    }


    // State Updates
    private void Collect_TrailMark()
    {
        if (_data.isOnSight) return;

        Tile playerTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        if (playerTile != _movement.tileTracker.data.CurrentTile()) return;

        _data.Decrease_TrailMarkCount(1);

        Tile nextTile = MoveDistance_RangeTile(true);
        if (nextTile == null) return;

        bool isOnSight = _data.isOnSight;

        transform.position = isOnSight ? nextTile.Random_BoundPoint() : nextTile.transform.position; // update offset ?
        _movement.tileTracker.data.TrackTile(nextTile);

        if (isOnSight == false) return;

        _healthFillBar.Set_FillBar(transform);
        _healthFillBar.Toggle(false);
    }

    private void Update_OnSight()
    {
        if (_data.isOnSight == false) return;

        OnSightActions?.Invoke();
    }

    public void Update_DeceasedState()
    {

    }
    private IEnumerator DeceasedState_Update()
    {
        yield break;
    }


    // Default Actions
    public void Roam()
    {
        if (_actionCoroutine != null) return;
        _actionCoroutine = StartCoroutine(Roam_MovementUpdate());
    }
    private IEnumerator Roam_MovementUpdate()
    {
        _movement.Move(MoveDistance_RangeTile(true).Random_BoundPoint());

        while (_movement.moveCoroutine != null) yield return null;
        _actionCoroutine = null;
    }

    public void RunOff()
    {

    }

    public void Escape(int delayCount)
    {

    }
    private IEnumerator EscapeDelay()
    {
        yield break;
    }

    public void Follow(int agroRange)
    {

    }

    public void Attack()
    {

    }
}