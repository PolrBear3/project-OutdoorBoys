using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Animation Clip")]
public class AnimationClipScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private string _clipName;
    public string clipName => _clipName;

    [Space(20)]
    [SerializeField] private bool _loop;
    public bool loop => _loop;

    [Space(20)]
    [SerializeField] private Sprite _defaultSprite;
    public Sprite defaultSprite => _defaultSprite;

    [Space(10)]
    [SerializeField] private ClipSpriteData[] _clipSpriteDatas;
    public ClipSpriteData[] clipSpriteDatas => _clipSpriteDatas;
}