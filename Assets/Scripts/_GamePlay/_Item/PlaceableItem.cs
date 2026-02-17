using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableItem : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animPlayer;
    public AnimationPlayer animPlayer => _animPlayer;

    private Tile _currentTile;
    public Tile currentTile => _currentTile;


    // Data
    public void Set_CurrentTile(Tile setTile)
    {
        if (setTile == null)
        {
            Destroy(gameObject);
            return;
        }
        _currentTile = setTile;
    }
}
