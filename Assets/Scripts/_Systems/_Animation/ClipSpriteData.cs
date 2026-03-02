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
    [SerializeField] private Vector2 _offSetPosition;
    public Vector2 offSetPosition => _offSetPosition;

    [SerializeField] private float _rotationValue;
    public float rotationValue => _rotationValue;

    [SerializeField][Range(0, 60)] private float _transformDurationTime;


    public float DurationTime()
    {
        return Mathf.Min(0.1f, _durationTime);
    }

    public float Transform_DurationTime()
    {
        return Mathf.Clamp(_transformDurationTime, 0f, DurationTime());
    }
}
