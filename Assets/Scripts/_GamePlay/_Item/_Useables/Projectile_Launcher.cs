using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Launcher : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField][Range(0, 360)] private float _projectTileLaunchAngle;

    [Space(10)]
    [SerializeField][Range(0, 10)] private float _projectileLaunchSpeed;


    private Coroutine _launchCoroutine;
    public Coroutine launchCoroutine => _launchCoroutine;

    public Action<Tile> OnLaunchComplete;


    // Launch
    public void Launch_Projectile(Tile useTile, Sprite projectTileSprite)
    {
        if (_launchCoroutine != null) return;

        InGame_Manager manager = InGame_Manager.instance;
        manager.time.timeUpdateActions.Add(this);

        Tile playerTile = manager.player.movement.tileTracker.data.CurrentTile();
        GameObject spawnedProjectile = Instantiate(_projectilePrefab, playerTile.setPosition);
        
        if (spawnedProjectile.TryGetComponent(out Projectile projectile) == false)
        {
            Destroy(spawnedProjectile);
            return;
        }

        projectile.animPlayer.spriteRenderer.sprite = projectTileSprite;
        projectile.Update_RotateDirection(useTile.transform.position - playerTile.transform.position, _projectTileLaunchAngle);

        projectile.movement.Update_CurrentSpeed(_projectileLaunchSpeed);
        projectile.LaunchTo_Tile(useTile);

        _launchCoroutine = StartCoroutine(LaunchComplete_Delay(useTile, projectile));
    }

    private IEnumerator LaunchComplete_Delay(Tile useTile, Projectile launchedProjectile)
    {
        while (launchedProjectile.movement.At_Destination() == false) yield return null;
        
        OnLaunchComplete?.Invoke(useTile);
        Destroy(launchedProjectile.gameObject);

        InGame_Manager.instance.time.timeUpdateActions.Remove(this);
        
        _launchCoroutine = null;
        yield break;
    }
}