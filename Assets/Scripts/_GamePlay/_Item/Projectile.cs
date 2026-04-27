using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animPlayer;
    public AnimationPlayer animPlayer => _animPlayer;

    [SerializeField] private Movement_Controller _movement;
    public Movement_Controller movement => _movement;


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
        if (targetTile == null) return;

        _movement.Move(targetTile);
    }
}