using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableItem : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animPlayer;
    public AnimationPlayer animPlayer => _animPlayer;


    private ItemData _data;
    public ItemData data => _data;

    private Tile _currentTile;
    public Tile currentTile => _currentTile;


    // Data
    public void Set_Data(ItemData setData)
    {
        if (setData == null)
        {
            Destroy(gameObject);
            return;
        }
        
        _data = setData;
        _animPlayer.Set_DefaultPosition(_data.itemScrObj.offsetPosition);
    }

    public void Track_CurrentTile(Tile setTile)
    {
        if (setTile == null)
        {
            Destroy(gameObject);
            return;
        }
        _currentTile = setTile;
    }
}
