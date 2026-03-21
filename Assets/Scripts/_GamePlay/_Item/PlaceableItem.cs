using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaceableItem_DurabilityData
{
    private PlaceableItem _placeableItem;
    public PlaceableItem placeableItem => _placeableItem;

    private int _durabilityCount;
    public int durabilityCount => _durabilityCount;

    public PlaceableItem_DurabilityData(PlaceableItem item, int durabilityCount)
    {
        _placeableItem = item;
        _durabilityCount = durabilityCount;
    }

    public int Update_DurabilityCount(int updateCount)
    {
        _durabilityCount = Mathf.Max(0, updateCount);
        return _durabilityCount;
    }
}

public class PlaceableItem : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private AnimationPlayer _animPlayer;
    public AnimationPlayer animPlayer => _animPlayer;

    [SerializeField] private AnimationClipScrObj _removeAnimationClip;


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


    // Animation Delay Remove
    public void AnimationDelay_Remove()
    {
        _currentTile.Remove_PlacedItemData(this);

        if (_removeAnimationClip == null)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(AnimationDelay_RemoveUpdate());
    }
    private IEnumerator AnimationDelay_RemoveUpdate()
    {
        _animPlayer.Play(_removeAnimationClip);

        while (_animPlayer.Animation_Playing()) yield return null;
        Destroy(gameObject);
    }
}
