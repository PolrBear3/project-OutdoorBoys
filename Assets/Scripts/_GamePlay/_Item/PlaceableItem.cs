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

    [Space(10)]
    [SerializeField] private AnimationClipScrObj _placeAnimationClip;
    [SerializeField] private AnimationClipScrObj _removeAnimationClip;


    private Tile _placedTile;
    public Tile placedTile => _placedTile;

    private ItemData _data;
    public ItemData data => _data;


    // Data
    public void Set_Data(ItemData setData)
    {
        if (setData == null)
        {
            _animPlayer.Stop();
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
        _placedTile = setTile;
    }


    // Animations
    public void Play_PlaceAnimation()
    {
        if (_placeAnimationClip == null) return;

        _animPlayer.defaultData.Update_ClipSprite(_data.itemScrObj.PlacedSprite());
        _animPlayer.Play(_placeAnimationClip);
    }

    public void AnimationDelay_Remove(AnimationClipScrObj removeAnimationClip)
    {
        _placedTile.Remove_PlacedItemData(this);

        if (removeAnimationClip == null)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(AnimationDelay_RemoveUpdate(removeAnimationClip));
    }
    public void AnimationDelay_Remove()
    {
        AnimationDelay_Remove(_removeAnimationClip);
    }

    private IEnumerator AnimationDelay_RemoveUpdate(AnimationClipScrObj removeAnimationClip)
    {
        _animPlayer.Play(removeAnimationClip);

        while (_animPlayer.Animation_Playing()) yield return null;
        Destroy(gameObject);
    }
}
