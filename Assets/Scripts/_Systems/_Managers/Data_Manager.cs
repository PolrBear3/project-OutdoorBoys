using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionUpdateBus
{
    AwakeUpdate = 0,
    StartUpdate = 1,
    SubUpdate = 2
}

public interface IDamageable
{
    bool IsDamageable();
    
    /// <returns>
    /// Actual Inflicted Damage Value
    /// </returns>
    int InflictDamage(int damageValue);
}

public class Data_Manager : MonoBehaviour
{
    public static Data_Manager instance;


    [Space(20)]
    [SerializeField] private TileScrObj[] _tileScrObjs;
    public TileScrObj[] tileScrObjs => _tileScrObjs;

    [Space(20)]
    [SerializeField] private Item_ScrObj[] _allItems;
    public Item_ScrObj[] allItems => _allItems;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;
    }


    // _itemScrObjs
    public Item_ScrObj Item(string itemName)
    {
        for (int i = 0; i < _allItems.Length; i++)
        {
            if (itemName != _allItems[i].itemName) continue;
            return _allItems[i];
        }
        return null;
    }
}