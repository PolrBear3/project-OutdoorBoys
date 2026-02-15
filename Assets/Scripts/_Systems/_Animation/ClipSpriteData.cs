using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClipSpriteData
{
    [SerializeField] private Sprite _clipSprite;
    public Sprite clipSprite => _clipSprite;

    [SerializeField][Range(0, 60)] private float _durationTime;


    public float DurationTime()
    {
        return Mathf.Min(0.1f, _durationTime);
    }
}
