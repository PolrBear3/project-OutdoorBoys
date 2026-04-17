using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animPlayer;
    public AnimationPlayer animPlayer => _animPlayer;


    // Visual
    public void Update_RotateDirection(Vector2 direction, float launchAngle)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += launchAngle;

        _animPlayer.spriteRenderer.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    // Main
    public void LaunchTo_Tile(Tile targetTile)
    {
        /*
        if (targetTile == null) return;

        InGame_Manager manager = InGame_Manager.instance;
        Tile launchStartTile = manager.player.movement.tileTrackerData.CurrentTile();

        _movement.MoveTo_Tile(launchStartTile);
        _movement.MoveTo_Tile(targetTile);
        */
    }
}