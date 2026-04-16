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

    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;

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

    private float _movementFlag = -1;
    private float _onSightFlag = -1;

    private Dictionary<string, int> _actionCountDatas = new();


    // MonoBehaviour
    private void OnDestroy()
    {
        _movement.OnMovementDirection -= _animation.Update_Flip;
        _movement.OnMovementActive -= Update_Animation;

        InGame_Manager manager = InGame_Manager.instance;

        manager.tilesController.OnTileHover -= Toggle_FillBar;
        _movement.OnMovement -= Toggle_FillBar;

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
        _movement.OnMovementDirection += _animation.Update_Flip;
        _movement.OnMovementActive += Update_Animation;

        InGame_Manager manager = InGame_Manager.instance;

        manager.tilesController.OnTileHover += Toggle_FillBar;
        _movement.OnMovement += Toggle_FillBar;

        Time_Manager time = manager.time;

        time.Register(TimeUpdateBus.AwakeUpdate, Collect_TrailMark);
        time.Register(TimeUpdateBus.AwakeUpdate, Update_OnSight);
    }

    public void Set_Data(AnimalScrObj setAnimal)
    {
        Transform currentTilePos = _movement.tileTrackerData.CurrentTile().transform;
        Transform playerTilePos = InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile().transform;

        int health = _data == null ? setAnimal.maxHealth : _data.health;

        int distanceFromPlayer = Utility.Chebyshev_Distance(currentTilePos.position, playerTilePos.position);
        int randCollectCount = UnityEngine.Random.Range(1, distanceFromPlayer + 1);

        _data = new(setAnimal, health, randCollectCount);

        _healthFillBar.Refresh_CurrentFillBar();
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
        InGame_Manager manager = InGame_Manager.instance;
        Tiles_Controller tilesController = manager.tilesController;

        Tile currentTile = _movement.tileTrackerData.CurrentTile();
        int distanceRange = _data.animalScrObj.moveDistanceRange;

        List<Tile> rangedTiles = tilesController.Current_Tiles(currentTile, distanceRange);
        rangedTiles.Remove(distanceRange > 0 ? currentTile : null);

        return rangedTiles;
    }
    private Tile MoveDistance_RangeTile(bool excludePlayerTile)
    {
        List<Tile> rangedTiles = MoveDistance_RangeTiles();
        if (excludePlayerTile) rangedTiles.Remove(InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile());

        return rangedTiles[UnityEngine.Random.Range(0, rangedTiles.Count)];
    }


    private bool Player_InRange()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile();
        float distance = playerTile.DistanceTo_TargetTile(_movement.tileTrackerData.CurrentTile());

        return distance <= _data.animalScrObj.moveDistanceRange;
    }

    public bool Deceased()
    {
        return _data.health <= 0 || AnimalManager().spawnedAnimals.Contains(this) == false;
    }


    // Visuals
    private void Toggle_FillBar(Tile hoveringTile)
    {
        bool toggle = hoveringTile == _movement.tileTrackerData.CurrentTile();
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
        if (InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile() != _movement.tileTrackerData.CurrentTile()) return;

        _data.Decrease_TrailMarkCount(1);

        _movement.Update_Offset(_data.isOnSight ? _movement.offset : Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        if (_data.isOnSight == false) return;
        _onSightFlag = Time.frameCount;

        _movement.Update_MoveDurationValue();

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
        if (_data.health > 0) return;

        Animals_Manager manager = AnimalManager();
        if (manager == null) return;

        manager.spawnedAnimals.Remove(this);

        Tile currentTile = _movement.tileTrackerData.CurrentTile();
        for (int i = 0; i < _dropItems.Length; i++)
        {
            currentTile.SetPreserve_Item(_dropItems[i]);
        }

        StartCoroutine(DeceasedState_Update());
    }
    private IEnumerator DeceasedState_Update()
    {
        MovementControllers_Manager movementsManager = InGame_Manager.instance.movements;
        while (movementsManager.AllMovements_Complete() == false) yield return null;

        _animation.Play(_deceasedAnimationClip);
        while (_animation.Animation_Playing()) yield return null;

        Destroy(gameObject);
        yield break;
    }


    // Default Actions
    public void Update_StunnedMovementState()
    {
        int stunnedStateCount = _movement.CurrentState_Count(MovementState.stunned);

        if (stunnedStateCount <= 0) return;
        _movement.Update_CurrentState(MovementState.stunned, stunnedStateCount - 1);

        _movementFlag = Time.frameCount;
    }


    public void RunOff()
    {
        if (_movementFlag == Time.frameCount) return;
        if (Player_InRange() == false) return;
        if (Deceased()) return;

        List<Tile> rangedTiles = MoveDistance_RangeTiles();

        Tile playerTile = InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile();
        float runOffDistance = UnityEngine.Random.Range(1, _data.animalScrObj.moveDistanceRange);

        Tile farTile = null;
        float farDistance = float.MinValue;

        for (int i = 0; i < rangedTiles.Count; i++)
        {
            Tile rangedTile = rangedTiles[i];
            if (rangedTile == playerTile) continue;

            Vector2 rangedTilePos = rangedTile.transform.position;

            float rangedTileDistance = Utility.Chebyshev_Distance(_movement.transform.position, rangedTilePos);
            if (rangedTileDistance > runOffDistance) continue;

            float distanceFromPlayer = Utility.Chebyshev_Distance(rangedTilePos, playerTile.transform.position);
            if (distanceFromPlayer <= farDistance) continue;

            farDistance = distanceFromPlayer;
            farTile = rangedTile;
        }

        _movement.MoveTo_Tile(farTile);
        _movementFlag = Time.frameCount;
    }

    public void Roam()
    {
        if (_movementFlag == Time.frameCount) return;
        if (Player_InRange()) return;
        if (Deceased()) return;

        Tile playerTile = InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile();
        List<Tile> rangedTiles = MoveDistance_RangeTiles();

        for (int i = rangedTiles.Count - 1; i >= 0; i--)
        {
            if (playerTile.DistanceTo_TargetTile(rangedTiles[i]) > _data.animalScrObj.moveDistanceRange) continue;
            rangedTiles.RemoveAt(i);
        }

        _movement.MoveTo_Tile(rangedTiles[UnityEngine.Random.Range(0, rangedTiles.Count)]);
        _movementFlag = Time.frameCount;
    }
    public void Roam(int coolTime)
    {
        string actionKey = nameof(Roam);
        _actionCountDatas[actionKey] = _actionCountDatas.ContainsKey(actionKey) ? _actionCountDatas[actionKey] : coolTime;

        if (_movementFlag == Time.frameCount) return;
        if (Player_InRange()) return;
        if (Deceased()) return;

        _actionCountDatas[actionKey]++;
        Debug.Log(actionKey + " " + (_actionCountDatas[actionKey] - 1) + "/" + coolTime);

        if (_actionCountDatas[actionKey] <= coolTime) return;

        _actionCountDatas[actionKey] = 0;
        Roam();
    }

    public void Escape(int delayCount)
    {
        if (Deceased()) return;

        string actionKey = nameof(Escape);
        _actionCountDatas[actionKey] = _actionCountDatas.ContainsKey(actionKey) ? _actionCountDatas[actionKey] : 0;

        if (Player_InRange() == false)
        {
            _actionCountDatas[actionKey] = Mathf.Max(0, _actionCountDatas[actionKey] - 1);
            return;
        }

        _actionCountDatas[actionKey] ++;
        Debug.Log(actionKey + " " + (_actionCountDatas[actionKey] - 1) + "/" + delayCount);

        if (_actionCountDatas[actionKey] <= delayCount) return;

        Animals_Manager manager = AnimalManager();
        if (manager == null) return;

        Vector2 currentTilePos = _movement.tileTrackerData.CurrentTile().transform.position;
        List<Tile> edgedTiles = InGame_Manager.instance.tilesController.Current_EdgedTiles();

        edgedTiles.Sort((a, b) =>
        {
            int distA = Utility.Chebyshev_Distance(currentTilePos, a.transform.position);
            int distB = Utility.Chebyshev_Distance(currentTilePos, b.transform.position);
            return distA.CompareTo(distB);
        });

        Tile escapeTile = edgedTiles.Count > 0 ? edgedTiles[0] : null;
        Vector2 escapetilePos = escapeTile.transform.position;

        List<Tile> rangedTiles = MoveDistance_RangeTiles();

        rangedTiles.Sort((a, b) =>
        {
            int distA = Utility.Chebyshev_Distance(escapetilePos, a.transform.position);
            int distB = Utility.Chebyshev_Distance(escapetilePos, b.transform.position);
            return distA.CompareTo(distB);
        });
        escapeTile = rangedTiles[0];

        _movement.MoveTo_Tile(escapeTile);
        _movementFlag = Time.frameCount;

        if (edgedTiles.Contains(escapeTile) == false) return;

        manager.spawnedAnimals.Remove(this);
        StartCoroutine(EscapeDelay());
    }
    private IEnumerator EscapeDelay()
    {
        InGame_Manager manager = InGame_Manager.instance;
        MovementControllers_Manager movementsManager = manager.movements;

        while (movementsManager.AllMovements_Complete() == false) yield return null;

        Tiles_Controller tilesController = manager.tilesController;
        List<Vector2> surroundingPositions = Utility.Surrounding_Positions(_movement.tileTrackerData.CurrentTile().transform.position);
        
        for (int i = 0; i < surroundingPositions.Count; i++)
        {
            Vector2 escapePos = surroundingPositions[i];
            if (tilesController.Current_Tile(escapePos) != null) continue;

            _movement.MoveTo_CustomPosition(escapePos);
            _animation.Play(_escapeAnimationClip);

            break;
        }

        while (LeanTween.isTweening(gameObject)) yield return null;
        Destroy(gameObject);
    }

    public void Follow(int agroRange)
    {
        if (_movementFlag == Time.frameCount) return;
        if (Deceased()) return;

        InGame_Manager manager = InGame_Manager.instance;

        Tile currentTile = _movement.tileTrackerData.CurrentTile();
        Tile playerTile = manager.player.movement.tileTrackerData.CurrentTile();

        if (playerTile == currentTile) return;
        if (playerTile.DistanceTo_TargetTile(currentTile) > agroRange) return;

        List<Tile> rangedTiles = MoveDistance_RangeTiles();

        Tile closestTile = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < rangedTiles.Count; i++)
        {
            Tile rangedTile = rangedTiles[i];

            float distance = Utility.Chebyshev_Distance(rangedTile.transform.position, playerTile.transform.position);
            if (distance > closestDistance) continue;

            closestDistance = distance;
            closestTile = rangedTile;
        }

        _movement.MoveTo_Tile(closestTile);
        _movementFlag = Time.frameCount;

        // manager.time.Stop_TimTik();
    }


    private Tile FollowItem_Tile()
    {
        Tile currentTile = _movement.tileTrackerData.CurrentTile();

        for (int i = 0; i < _followItems.Length; i++)
        {
            List<Tile> itemExsitTiles = new(InGame_Manager.instance.tilesController.Current_Tiles(_followItems[i]));
            if (itemExsitTiles.Count <= 0) continue;

            Tile closestTile = null;
            float closestDistance = float.MaxValue;

            for (int j = 0; j < itemExsitTiles.Count; j++)
            {
                float distanceToTile = Utility.Chebyshev_Distance(currentTile.transform.position, itemExsitTiles[i].transform.position);
                if (distanceToTile > closestDistance) continue;

                closestDistance = distanceToTile;
                closestTile = itemExsitTiles[i];
            }
            return closestTile;
        }
        return null;
    }

    public void Follow_Item()
    {
        if (_movementFlag == Time.frameCount) return;

        Tile currentTile = _movement.tileTrackerData.CurrentTile();
        for (int i = 0; i < _followItems.Length; i++)
        {
            if (currentTile.PlacedItem(_followItems[i]) == null) continue;

            _movementFlag = Time.frameCount;
            return;
        }

        Tile followItemTile = FollowItem_Tile();
        if (followItemTile == null) return;

        List<Tile> rangedTiles = new(MoveDistance_RangeTiles());
        float moveDistance = UnityEngine.Random.Range(1, _data.animalScrObj.moveDistanceRange);

        Tile closestTile = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < rangedTiles.Count; i++)
        {
            Tile rangedTile = rangedTiles[i];
            Vector2 rangedTilePos = rangedTile.transform.position;

            float distanceToTile = Utility.Chebyshev_Distance(currentTile.transform.position, rangedTilePos);
            if (distanceToTile > moveDistance) return;

            float distanceToItem = Utility.Chebyshev_Distance(rangedTilePos, followItemTile.transform.position);
            if (distanceToItem > closestDistance) continue;

            closestDistance = distanceToItem;
            closestTile = rangedTile;
        }

        _movement.MoveTo_Tile(closestTile);
        _movementFlag = Time.frameCount;
    }
    public void Follow_Item(int maxFollowCount)
    {
        Follow_Item();
    }


    public void Attack()
    {
        if (InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile() != _movement.tileTrackerData.CurrentTile()) return;

        StartCoroutine(AttackDelay());
    }
    private IEnumerator AttackDelay()
    {
        MovementControllers_Manager movementsManager = InGame_Manager.instance.movements;
        while (movementsManager.AllMovements_Complete() == false) yield return null;

        if (Deceased()) yield break;
        Debug.Log("Game Over by Bear Attack");
    }
}