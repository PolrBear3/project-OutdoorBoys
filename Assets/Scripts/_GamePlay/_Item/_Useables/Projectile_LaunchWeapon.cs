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
    private void Launch(Tile useTile)
    {
        /*
        if (_projectileLauncher.launchCoroutine != null) return;

        if (useTile == InGame_Manager.instance.player.movement.tileTrackerData.CurrentTile())
        {
            Damage(useTile);
            return;
        }
        _projectileLauncher.Launch_Projectile(useTile, _useableItem.data.itemScrObj.inventorySprite);
        */
    }


    private bool Obstacle_Blocked(Tile useTile)
    {
        for (int i = 0; i < _obstacleItems.Length; i++)
        {
            if (useTile.Placed_ItemCount(_obstacleItems[i]) > 0) return true;
        }
        return false;
    }

    private void Damage(Tile useTile)
    {
        InGame_Manager manager = InGame_Manager.instance;
        ItemCursor itemCursor = manager.cursor.itemCursor;

        List<GameObject> tilePrefabs = useTile.All_CurrentPrefabs();
        bool damageSuccessful = false;

        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            if (Obstacle_Blocked(useTile)) break;
            if (tilePrefabs[i].TryGetComponent(out IDamageable damageable) == false) continue;

            damageable.InflictDamage(_damageValue);
            damageSuccessful = true;

            break;
        }
        
        if (_weaponType == Projectile_LaunchWeaponType.consumable)
        {
            _useableItem.Update_UseAmount(1);
            itemCursor.Update_Visuals();
            return;
        }
        
        _useableItem.Update_UseAmount(damageSuccessful ? 1 : 0);
        useTile.SetPreserve_Item(_useableItem.data);

        itemCursor.Set_Data(null);
        itemCursor.Update_Visuals();
    }
}