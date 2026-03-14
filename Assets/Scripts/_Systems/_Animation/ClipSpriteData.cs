using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClipSpriteData
{
    [SerializeField] private string _clipName;
    

    [Space(20)]
    [SerializeField] private Sprite _clipSprite;
    public Sprite clipSprite => _clipSprite;

    [SerializeField][Range(0, 60)] private float _durationTime;


    [Space(20)]
    [SerializeField][Range(-10, 10)] private int _sortingOrderUpdateValue;
    public int sortingOrderUpdateValue => _sortingOrderUpdateValue;

    [SerializeField][Range(-1, 1)] private float _alphaUpdateValue;
    public float alphaUpdateValue => _alphaUpdateValue;

    [SerializeField][Range(0, 60)] private float _alphaDurationTime;


    [Space(20)]
    [SerializeField] private Vector2 _offSetPosition;
    public Vector2 offSetPosition => _offSetPosition;

    [SerializeField] private float _rotationValue;
    public float rotationValue => _rotationValue;

    [SerializeField][Range(0, 60)] private float _transformDurationTime;


    // Default Data Constructor
    public ClipSpriteData(Sprite sprite, int layerNum, float alphaValue, Vector2 position)
    {
        _clipSprite = sprite;
        _sortingOrderUpdateValue = layerNum;
        _alphaUpdateValue = alphaValue;
        _offSetPosition = position;
    }


    // Duration Time
    public float DurationTime()
    {
        return Mathf.Max(0.1f, _durationTime);
    }

    public float Transform_DurationTime()
    {
        return Mathf.Clamp(_transformDurationTime, 0f, DurationTime());
    }

    public float Alpha_DurationTime()
    {
        return Mathf.Clamp(_alphaDurationTime, 0f, DurationTime());
    }
}
