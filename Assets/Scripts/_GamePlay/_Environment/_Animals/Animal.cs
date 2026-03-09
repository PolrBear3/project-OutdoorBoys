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
        _movement.OnMovementStated -= Update_Animation;

        Movement_Controller playerMovement = InGame_Manager.instance.player.movement;

        playerMovement.OnMovement -= Collect_TrailMark;
        playerMovement.OnMovement -= Update_OnSight;
    }


    // Data
    public void Set_Data()
    {
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
        bool isOnSight = _data.isOnSight;

        _animation.spriteRenderer.sortingLayerName = isOnSight ? "Default" : "Behind Player";

        if (_data.isOnSight) return;
        _animation.Play(0);
    }
    public void Update_Animation(bool isMoving)
    {
        if (_data.isOnSight == false) return;
        _animation.Play(isMoving ? 2 : 1);
    }


    // Trail Mark Collecting
    private Tile MoveDistance_RangeTile()
    {
        Tiles_Controller tilesController = InGame_Manager.instance.tilesController;

        Tile currentTile = _movement.currentTile;
        int distanceRange = _data.animalScrObj.moveDistanceRange;

        List<Tile> rangedTiles = tilesController.Current_Tiles(currentTile, distanceRange);
        rangedTiles.Remove(distanceRange > 0 ? currentTile : null);

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

        _movement.Update_Offset(Vector2.zero);
        _movement.MoveTo_Tile(MoveDistance_RangeTile());

        _data.Decrease_TrailMarkCount(1);
        Update_Animation();
    }

    private void Update_OnSight()
    {
        if (_data.isOnSight == false) return;

        _movement.Update_MoveDurationValue();
        _movement.Update_Offset();

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
        _movement.MoveTo_Tile(MoveDistance_RangeTile());

        Set_Data(_data.animalScrObj);
        Update_Animation();
    }

    public void Escape()
    {
        if (Player_InRange() == false) return;

        if (_data.onSightTimeCount <= 2)
        {
            _movement.MoveTo_Tile(MoveDistance_RangeTile());
            return;
        }

        InGame_Manager.instance.animals.spawnedAnimals.Remove(this);
        Destroy(gameObject);
    }
}