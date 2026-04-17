using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Launcher : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField][Range(0, 360)] private float _projectTileLaunchAngle;

    private Coroutine _launchCoroutine;
    public Coroutine launchCoroutine => _launchCoroutine;

    public Action<Tile> OnLaunchComplete;


    // Launch
    public void Launch_Projectile(Tile useTile, Sprite projectTileSprite)
    {
        /*
        if (_launchCoroutine != null) return;
        
        Tile playerTile = InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile();
        GameObject spawnedProjectile = Instantiate(_projectilePrefab, playerTile.setPosition);
        
        if (spawnedProjectile.TryGetComponent(out Projectile projectile) == false)
        {
            Destroy(spawnedProjectile);
            return;
        }

        projectile.animPlayer.spriteRenderer.sprite = projectTileSprite;
        projectile.Update_RotateDirection(useTile.transform.position - playerTile.transform.position, _projectTileLaunchAngle);
        projectile.LaunchTo_Tile(useTile);

        _launchCoroutine = StartCoroutine(LaunchComplete_Delay(useTile, projectile.gameObject));
        */
    }

    private IEnumerator LaunchComplete_Delay(Tile useTile, GameObject launchedProjectile)
    {
        MovementControllers_Manager movementsManager = InGame_Manager.instance.movements;
        while (movementsManager.AllMovements_Complete() == false) yield return null;

        OnLaunchComplete?.Invoke(useTile);
        Destroy(launchedProjectile);

        _launchCoroutine= null;
    }
}