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

    [SerializeField][Range(0, 10)] private float _playSpeed;
    public float playSpeed => _playSpeed;

    [Space(20)]
    [SerializeField] private Sprite _defaultSprite;
    public Sprite defaultSprite => _defaultSprite;

    [SerializeField] private Sprite[] _clipSprites;
    public Sprite[] clipSprites => _clipSprites;
}