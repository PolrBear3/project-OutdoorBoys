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

    private bool _onSightFlag;


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

        return distance <= _data.animalScrObj.moveDistanceRange;
    }


    private void Collect_TrailMark()
    {
        if (_data.isOnSight) return;
        if (InGame_Manager.instance.player.movement.currentTile != _movement.currentTile) return;

        _data.Decrease_TrailMarkCount(1);

        _movement.Update_Offset(_data.isOnSight ? _movement.offset : Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        if (_data.isOnSight == false) return;
        _onSightFlag = true;

        _movement.Update_MoveDurationValue();
    }

    private void Update_OnSight()
    {
        if (_onSightFlag)
        {
            _onSightFlag = false;
            return;
        }
        if (_data.isOnSight == false) return;

        _data.Update_OnSightTimeCount(1);
        OnSightActions?.Invoke();
    }


    // Default Actions
    public void RunOff_Sight()
    {
        if (Player_InRange() == false) return;

        _movement.Update_MoveDurationValue(0);
        _movement.Update_Offset(Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile(true));

        Set_Data(_data.animalScrObj);
        Update_Animation();
    }

    public void RunOff_fromPlayer()
    {
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
    }
    public void RunOff_fromPlayer(int maxRunOffCount)
    {
        if (_data.onSightTimeCount > maxRunOffCount) return;

        RunOff_fromPlayer();
    }

    public void Escape(int delayCount)
    {
        if (Player_InRange() == false) return;
        if (_data.onSightTimeCount <= delayCount) return;

        InGame_Manager.instance.animals.spawnedAnimals.Remove(this);
        Destroy(gameObject);
    }

    public void Follow(int maxFollowCount)
    {
        int onSightTimeCount = _data.onSightTimeCount;

        if (onSightTimeCount <= 1) return;
        if (onSightTimeCount > maxFollowCount + 1) return;

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
    }

    public void Attack()
    {
        if (InGame_Manager.instance.player.movement.currentTile != _movement.currentTile) return;

        Debug.Log("Game Over by Bear Attack");
    }
}