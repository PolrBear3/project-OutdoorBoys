using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Launcher : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;

    [Space(20)]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private AnimationClipScrObj _launchAnimClip;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Launch_Projectile;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Launch_Projectile;
    }


    // Launch
    private void Launch_Projectile(Tile useTile)
    {
        Vector2 launchStartPos = InGame_Manager.instance.player.movement.currentTile.transform.position;
        GameObject spawnedProjectile = Instantiate(_projectilePrefab, launchStartPos, Quaternion.identity);

        if (spawnedProjectile.TryGetComponent(out Projectile projectile) == false)
        {
            Destroy(spawnedProjectile);
            return;
        }
        projectile.LaunchTo_Tile(useTile);
    }
}
