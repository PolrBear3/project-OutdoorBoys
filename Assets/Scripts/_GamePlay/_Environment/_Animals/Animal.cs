using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Animal : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animation;

    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;

    [Space(20)]
    [SerializeField] private AnimationClipScrObj _deceasedAnimationClip;

    [Space(20)]
    [SerializeField] private ItemData[] _dropItems;
    [SerializeField] private Item_ScrObj[] _followItems;

    [Space(20)]
    [SerializeField][Range(0, 10)] private int _actionDelayTime;

    [Space(10)]
    public UnityEvent OnSightActions;


    private AnimalData _data;
    public AnimalData data => _data;

    private float _movementFlag = -1;
    private float _onSightFlag = -1;


    // MonoBehaviour
    private void OnDestroy()
    {
        _movement.OnMovementDirection -= _animation.Update_Flip;
        _movement.OnMovementStated -= Update_Animation;

        Movement_Controller playerMovement = InGame_Manager.instance.player.movement;

        playerMovement.OnMovement -= Collect_TrailMark;
        playerMovement.OnMovement -= Update_OnSight;
    }


    // Data
    public void Set_Data()
    {
        _movement.OnMovementDirection += _animation.Update_Flip;
        _movement.OnMovementStated += Update_Animation;

        Movement_Controller playerMovement = InGame_Manager.instance.player.movement;

        playerMovement.OnMovement += Collect_TrailMark;
        playerMovement.OnMovement += Update_OnSight;
    }

    public void Set_Data(AnimalScrObj setAnimal)
    {
        Transform currentTilePos = _movement.currentTile.transform;
        Transform playerTilePos = InGame_Manager.instance.player.movement.currentTile.transform;

        int health = _data == null ? setAnimal.maxHealth : _data.health;

        int distanceFromPlayer = Utility.Chebyshev_Distance(currentTilePos.position, playerTilePos.position);
        int randCollectCount = UnityEngine.Random.Range(1, distanceFromPlayer + 1);

        _data = new(setAnimal, health, randCollectCount);
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

        Tile currentTile = _movement.currentTile;
        int distanceRange = _data.animalScrObj.moveDistanceRange;

        List<Tile> rangedTiles = tilesController.Current_Tiles(currentTile, distanceRange);
        rangedTiles.Remove(distanceRange > 0 ? currentTile : null);

        return rangedTiles;
    }
    private Tile MoveDistance_RangeTile(bool excludePlayerTile)
    {
        List<Tile> rangedTiles = MoveDistance_RangeTiles();
        if (excludePlayerTile) rangedTiles.Remove(InGame_Manager.instance.player.movement.currentTile);

        return rangedTiles[UnityEngine.Random.Range(0, rangedTiles.Count)];
    }

    private bool Player_InRange()
    {
        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
        float distance = playerTile.DistanceTo_TargetTile(_movement.currentTile);

        return distance <= _data.animalScrObj.moveDistanceRange;
    }


    // State Updates
    private void Collect_TrailMark()
    {
        if (_data.isOnSight) return;
        if (InGame_Manager.instance.player.movement.currentTile != _movement.currentTile) return;

        _data.Decrease_TrailMarkCount(1);

        _movement.Update_Offset(_data.isOnSight ? _movement.offset : Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        if (_data.isOnSight == false) return;
        _onSightFlag = Time.frameCount;

        _movement.Update_MoveDurationValue();
    }

    private void Update_OnSight()
    {
        if (_data.isOnSight == false) return;
        if (_onSightFlag == Time.frameCount) return;

        _data.Update_OnSightTimeCount(1);
        OnSightActions?.Invoke();
    }

    public void Update_DeceasedState()
    {
        if (_data.health > 0) return;

        Animals_Manager manager = AnimalManager();
        if (manager == null) return;

        manager.spawnedAnimals.Remove(this);

        Tile currentTile = _movement.currentTile;
        for (int i = 0; i < _dropItems.Length; i++)
        {
            currentTile.Set_Item(_dropItems[i]);
        }

        StartCoroutine(DeceasedState_Update());
    }
    private IEnumerator DeceasedState_Update()
    {
        _animation.Play(_deceasedAnimationClip);
        while (_animation.Animation_Playing()) yield return null;

        Destroy(gameObject);
        yield break;
    }


    // Default Actions
    public void RunOff()
    {
        if (_movementFlag == Time.frameCount) return;
        if (_data.health <= 0) return;
        if (Player_InRange() == false) return;

        List<Tile> rangedTiles = MoveDistance_RangeTiles();

        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
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
    public void RunOff(int maxRunOffCount)
    {
        if (_data.onSightTimeCount > maxRunOffCount) return;
        RunOff();
    }

    public void RunOff_Sight()
    {
        if (_data.health <= 0) return;
        if (Player_InRange() == false) return;

        _movement.Update_MoveDurationValue(0);
        _movement.Update_Offset(Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        Set_Data(_data.animalScrObj);
        Update_Animation();
    }


    public void Escape(int delayCount)
    {
        if (_data.health <= 0) return;
        if (_data.onSightTimeCount <= delayCount) return;

        Animals_Manager manager = AnimalManager();
        if (manager == null) return;

        manager.spawnedAnimals.Remove(this);
        Destroy(gameObject);
    }

    public void Follow(int maxFollowCount)
    {
        if (_movementFlag == Time.frameCount) return;
        if (_data.health <= 0) return;

        int onSightTimeCount = _data.onSightTimeCount;

        if (onSightTimeCount <= _actionDelayTime) return;
        if (onSightTimeCount > maxFollowCount + _actionDelayTime) return;

        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
        if (playerTile == _movement.currentTile) return;

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
    }
    

    private Tile FollowItem_Tile()
    {
        Tile currentTile = _movement.currentTile;

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
        if (_data.onSightTimeCount <= _actionDelayTime) return;

        Tile currentTile = _movement.currentTile;
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
        if (_data.onSightTimeCount > maxFollowCount + _actionDelayTime) return;
        Follow_Item();
    }


    public void Attack()
    {
        if (_data.health <= 0) return;
        if (InGame_Manager.instance.player.movement.currentTile != _movement.currentTile) return;

        Debug.Log("Game Over by Bear Attack");
    }
}