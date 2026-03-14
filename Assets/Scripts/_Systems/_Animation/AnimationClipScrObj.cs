using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Animation Clip")]
public class AnimationClipScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private string _clipName;
    public string clipName => _clipName;

    [SerializeField] private bool _loop;
    public bool loop => _loop;

    [Space(10)]
    [SerializeField] private ClipSpriteData[] _clipSpriteDatas;
    public ClipSpriteData[] clipSpriteDatas => _clipSpriteDatas;
}