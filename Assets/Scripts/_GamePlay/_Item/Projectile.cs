using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animPlayer;
    public AnimationPlayer animPlayer => _animPlayer;

    [SerializeField] private Movement_Controller _movement;

    private Coroutine _launchCoroutine;


    // Visual
    private void Update_RotateDirection()
    {
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    // Main
    private Vector2 LaunchDirection(Tile targetTile)
    {
        Vector2 currentTilePos = _movement.currentTile.transform.position;
        return Utility.Grid_Direction(currentTilePos, targetTile.transform.position);
    }

    public void LaunchTo_Tile(Tile targetTile)
    {
        if (_launchCoroutine != null) return;
        if (targetTile == null) return;

        _launchCoroutine = StartCoroutine(LaunchMovement_Update(targetTile));
    }
    private IEnumerator LaunchMovement_Update(Tile targetTile)
    {
        InGame_Manager manager = InGame_Manager.instance;

        Tiles_Controller tilesController = manager.tilesController;
        MovementControllers_Manager movements = manager.movements;

        _movement.MoveTo_Tile(manager.player.movement.currentTile);
        Vector2 launchDirection = LaunchDirection(targetTile);

        while (_movement.currentTile != targetTile)
        {
            if (tilesController.Current_EdgedTiles().Contains(_movement.currentTile)) break;

            _movement.MoveTo_Tile(launchDirection);

            while (movements.AllMovements_Complete() == false) yield return null;
            yield return new WaitForSeconds(_movement.moveDuration);
        }

        _launchCoroutine = null;
        yield break;
    }
}