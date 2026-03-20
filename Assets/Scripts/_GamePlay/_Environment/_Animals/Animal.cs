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
    public UnityEvent OnSightActions;


    private AnimalData _data;
    public AnimalData data => _data;


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
        int randCollectCount = UnityEngine.Random.Range(1, distanceFromPlayer);

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

        _animation.Play(isMoving ? 2 : 1);
    }


    // Trail Mark Collecting
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

        return distance < _data.animalScrObj.moveDistanceRange;
    }


    private void Collect_TrailMark()
    {
        if (_data.isOnSight) return;
        if (InGame_Manager.instance.player.movement.currentTile != _movement.currentTile) return;

        _data.Decrease_TrailMarkCount(1);

        _movement.Update_Offset(_data.isOnSight ? _movement.offset : Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        if (_data.isOnSight == false) return;
        _movement.Update_MoveDurationValue();
    }

    private void Update_OnSight()
    {
        if (_data.isOnSight == false) return;
        _data.Update_OnSightTimeCount(1);

        if (_data.onSightTimeCount <= 1) return;
        OnSightActions?.Invoke();
    }


    // Default Actions
    public void RunOff()
    {
        if (Player_InRange() == false) return;

        _movement.Update_MoveDurationValue(0);
        _movement.Update_Offset(Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        Set_Data(_data.animalScrObj);
        Update_Animation();
    }

    public void Escape()
    {
        if (Player_InRange() == false) return;

        InGame_Manager manager = InGame_Manager.instance;

        // escape
        if (_movement.currentMoveDuration <= 0)
        {
            manager.animals.spawnedAnimals.Remove(this);
            Destroy(gameObject);

            return;
        }

        // run off
        List<Tile> rangedTiles = MoveDistance_RangeTiles();
        float escapeDistance = UnityEngine.Random.Range(1, _data.animalScrObj.moveDistanceRange) - 1;

        Tile farTile = null;
        float farDistance = float.MinValue;

        for (int i = 0; i < rangedTiles.Count; i++)
        {
            Tile rangedTile = rangedTiles[i];
            Vector2 rangedTilePos = rangedTile.transform.position;

            float rangedTileDistance = Utility.Chebyshev_Distance(_movement.transform.position, rangedTilePos);
            if (rangedTileDistance != escapeDistance) continue;

            Vector2 playerTilePos = manager.player.movement.currentTile.transform.position;
            float distanceFromPlayer = Utility.Chebyshev_Distance(rangedTilePos, playerTilePos);

            if (distanceFromPlayer <= farDistance) continue;

            farDistance = distanceFromPlayer;
            farTile = rangedTile;
        }

        _movement.MoveTo_Tile(farTile);
        _movement.Update_MoveDurationValue(0);
    }

    public void Follow(int maxFollowCount)
    {
        int onSightTimeCount = _data.onSightTimeCount;
        if (onSightTimeCount <= 2) return;

        int actualFollowCount = maxFollowCount + 3;

        // escape
        if (onSightTimeCount == actualFollowCount)
        {
            _movement.MoveTo_Tile(MoveDistance_RangeTile(true));
            return;
        }
        if (onSightTimeCount >= actualFollowCount + 1)
        {
            InGame_Manager.instance.animals.spawnedAnimals.Remove(this);
            Destroy(gameObject);
            return;
        }

        // follow
        Tile playerTile = InGame_Manager.instance.player.movement.currentTile;
        List<Tile> rangedTiles = MoveDistance_RangeTiles();

        float closestDistance = float.MaxValue;
        Tile closestTile = null;

        for (int i = 0; i < rangedTiles.Count; i++)
        {
            Tile rangedTile = rangedTiles[i];

            float distance = Utility.Chebyshev_Distance(rangedTile.transform.position, playerTile.transform.position);
            if (distance > closestDistance) continue;

            closestDistance = distance;
            closestTile = rangedTile;
        }
        _movement.MoveTo_Tile(closestTile);
    }

    public void Attack()
    {
        if (InGame_Manager.instance.player.movement.currentTile != _movement.currentTile) return;

        Debug.Log("Game Over by Bear Attack");
    }
}