using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Projectile_LaunchWeaponType { reuseable, consumable }

public class Projectile_LaunchWeapon : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;

    [Space(20)]
    [SerializeField] private Projectile_Launcher _projectileLauncher;

    [Space(20)]
    [SerializeField] private Projectile_LaunchWeaponType _weaponType;
    [SerializeField][Range(0, 50)] private int _maxLaunchDistance;
    [SerializeField][Range(0, 10)] private int _damageValue;

    [Space(20)]
    [SerializeField] private Item_ScrObj[] _obstacleItems;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Launch;
        _projectileLauncher.OnLaunchComplete += Damage;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Launch;
        _projectileLauncher.OnLaunchComplete -= Damage;
    }


    // Use
    private bool Obstacle_Blocked(Tile checkTile)
    {
        for (int i = 0; i < _obstacleItems.Length; i++)
        {
            if (checkTile.Placed_ItemCount(_obstacleItems[i]) > 0) return true;
        }
        return false;
    }
    private bool Has_Damageables(Tile checkTile)
    {
        List<GameObject> tilePrefabs = checkTile.All_CurrentPrefabs();

        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            if (tilePrefabs[i].TryGetComponent(out IDamageable damageable) == false) continue;
            if (damageable.IsDamageable() == false) continue;

            return true;
        }
        return false;
    }

    public Tile Launch_DestinationTile(Tile directionalTile)
    {
        Tile startTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        if (startTile == null || directionalTile == null) return null;

        Vector2 directionValue = Utility.Grid_Direction(startTile.transform.position, directionalTile.transform.position);
        if (directionValue == Vector2.zero) return startTile;

        List<Tile> directionTiles = TilePatterns_Utility.Directional_Tiles(startTile, directionValue);
        int distanceTraveled = 1;

        for (int i = 0; i < directionTiles.Count; i++)
        {
            Tile directionTile = directionTiles[i];
            if (directionTile == startTile) continue;

            if (Obstacle_Blocked(directionTile) || Has_Damageables(directionTile)) return directionTile;
            if (distanceTraveled >= _maxLaunchDistance) return directionTile;

            distanceTraveled++;
        }
        return startTile;
    }
    private void Launch(Tile useTile)
    {
        if (_projectileLauncher.launchCoroutine != null) return;

        _projectileLauncher.Launch_Projectile(Launch_DestinationTile(useTile), _useableItem.data.itemScrObj.inventorySprite);
    }

    private bool InflictDamage(Tile damageTile)
    {
        if (damageTile == null) return false;

        Tile startTile = InGame_Manager.instance.player.movement.tileTracker.data.CurrentTile();
        if (damageTile == startTile) return false;

        bool damageInflicted = false;
        List<GameObject> tilePrefabs = damageTile.All_CurrentPrefabs();

        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            if (Obstacle_Blocked(damageTile)) break;
            if (tilePrefabs[i].TryGetComponent(out IDamageable damageable) == false) continue;
            if (damageable.InflictDamage(_damageValue) <= 0) continue;

            damageInflicted = true;
            break;
        }
        return damageInflicted;
    }
    private void Damage(Tile damageTile)
    {
        InGame_Manager manager = InGame_Manager.instance;
        ItemCursor itemCursor = manager.cursor.itemCursor;

        bool damageInflicted = InflictDamage(damageTile);

        int useAmountDecrease = _weaponType == Projectile_LaunchWeaponType.consumable ? 1 : damageInflicted ? 1 : 0;
        _useableItem.Update_UseAmount(useAmountDecrease);

        if (_weaponType == Projectile_LaunchWeaponType.reuseable)
        {
            damageTile.SetPreserve_Item(_useableItem.data);
            itemCursor.Set_Data(null);
        }
        itemCursor.Update_Visuals();
    }
}