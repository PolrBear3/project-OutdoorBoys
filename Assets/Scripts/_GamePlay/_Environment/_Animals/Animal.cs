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
    [Space(20)]
    [SerializeField] private AnimationPlayer _animation;
    [SerializeField] private FillBar_Controller _healthFillBar;


    [Space(20)]
    [SerializeField] private AnimationClipScrObj _escapeAnimationClip;
    [SerializeField] private AnimationClipScrObj _deceasedAnimationClip;

    [Space(20)]
    [SerializeField] private ItemData[] _dropItems;
    [SerializeField] private Item_ScrObj[] _followItems;

    [Space(10)]
    public UnityEvent OnSightActions;


    private AnimalData _data;
    public AnimalData data => _data;

    // private float _movementFlag = -1;
    private float _onSightFlag = -1;

    private Dictionary<string, int> _actionCountDatas = new();


    // MonoBehaviour
    private void OnDestroy()
    {
        // _movement.OnMovementDirection -= _animation.Update_Flip;
        // _movement.OnMovementActive -= Update_Animation;

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
        // _movement.OnMovementDirection += _animation.Update_Flip;
        // _movement.OnMovementActive += Update_Animation;

        InGame_Manager manager = InGame_Manager.instance;

        manager.tilesController.OnTileHover += Toggle_FillBar;
        // _movement.OnMovement += Toggle_FillBar;

        Time_Manager time = manager.time;

        time.Register(TimeUpdateBus.AwakeUpdate, Collect_TrailMark);
        time.Register(TimeUpdateBus.AwakeUpdate, Update_OnSight);
    }

    public void Set_Data(AnimalScrObj setAnimal)
    {
        /*
        Transform currentTilePos = _movement.tileTrackerData.CurrentTile().transform;
        Transform playerTilePos = InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile().transform;

        int health = _data == null ? setAnimal.maxHealth : _data.health;

        int distanceFromPlayer = Utility.Chebyshev_Distance(currentTilePos.position, playerTilePos.position);
        int randCollectCount = UnityEngine.Random.Range(1, distanceFromPlayer + 1);

        _data = new(setAnimal, health, randCollectCount);

        _healthFillBar.Refresh_CurrentFillBar();
        */
    }


    public void Update_Animation()
    {
        if (_data.isOnSight) return;

        _animation.Play(0);
    }
    public void Update_Animation(bool isMoving)
    {
        if (_data.isOnSight == false) return;

        if (isMoving == false)
        {
            _animation.Stop();
            return;
        }
        _animation.Play(1);
    }


    private Animals_Manager AnimalManager()
    {
        GameObject eventsPrefab = InGame_Manager.instance.worldMapGenerator.currentMapEventsPrefab;

        if (eventsPrefab.TryGetComponent(out Animals_Manager manager) == false) return null;
        return manager;
    }

    private List<Tile> MoveDistance_RangeTiles()
    {
        /*
        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        Tile currentTile = _movement.tileTrackerData.CurrentTile();
        int distanceRange = _data.animalScrObj.moveDistanceRange;

        List<Tile> rangedTiles = tilesController.Current_Tiles(currentTile, distanceRange);
        rangedTiles.Remove(distanceRange > 0 ? currentTile : null);
        */

        return null; //rangedTiles;
    }
    private Tile MoveDistance_RangeTile(bool excludePlayerTile)
    {
        /*
        List<Tile> rangedTiles = MoveDistance_RangeTiles();
        if (excludePlayerTile) rangedTiles.Remove(InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile());
        */

        return null; // rangedTiles[UnityEngine.Random.Range(0, rangedTiles.Count)];
    }


    private bool Player_InRange()
    {
        Tile playerTile = InGame_Manager.instance.player.tileTracker.data.CurrentTile();
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
        bool toggle = true; //hoveringTile == _movement.tileTrackerData.CurrentTile();
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
        // if (InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile() != _movement.tileTrackerData.CurrentTile()) return;

        _data.Decrease_TrailMarkCount(1);

        // _movement.Update_Offset(_data.isOnSight ? _movement.offset : Vector2.zero);
        // _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        if (_data.isOnSight == false) return;
        _onSightFlag = Time.frameCount;

        // _movement.Update_MoveDurationValue();

        _healthFillBar.Set_FillBar(transform);
        _healthFillBar.Toggle(false);
    }

    private void Update_OnSight()
    {
        if (_data.isOnSight == false) return;
        if (_onSightFlag == Time.frameCount) return;

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
    public void Update_StunnedMovementState()
    {
        // int stunnedStateCount = _movement.CurrentState_Count(MovementState.stunned);

        // if (stunnedStateCount <= 0) return;
        // _movement.Update_CurrentState(MovementState.stunned, stunnedStateCount - 1);
    }


    public void RunOff()
    {

    }

    public void Roam()
    {

    }
    public void Roam(int coolTime)
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