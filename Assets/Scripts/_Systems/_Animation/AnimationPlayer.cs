using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _renderer;

    [Space(10)]
    [SerializeField] private AnimationClipScrObj[] _animationClips;

    private Coroutine _playCoroutine;


    // Data
    private AnimationClipScrObj AnimationClip(string clipName)
    {
        for (int i = 0; i < _animationClips.Length; i++)
        {
            if (clipName != _animationClips[i].clipName) continue;
            return _animationClips[i];
        }
        return null;
    }


    // Main
    public void Stop()
    {
        if (_playCoroutine == null) return;

        StopCoroutine(_playCoroutine);
        _playCoroutine = null;
    }

    public void Play(string clipName)
    {
        if (_animationClips == null) return;
        AnimationClipScrObj playClip = AnimationClip(clipName);
        
        if (playClip == null) return;
        Sprite[] clipSprites = playClip.clipSprites;

        Stop();

        if (_animationClips.Length <= 1)
        {
            for (int i = 0; i < clipSprites.Length; i++)
            {
                if (clipSprites[i] == null) continue;

                _renderer.sprite = clipSprites[i];
                return;
            }
            _renderer.sprite = playClip.defaultSprite;
        }

        _playCoroutine = StartCoroutine(Play_AnimationClip(playClip));
    }

    private IEnumerator Play_AnimationClip(AnimationClipScrObj playClip)
    {
        float playSpeed = Mathf.Min(0.1f, playClip.playSpeed);

        Sprite[] clipSprites = playClip.clipSprites;
        int spriteIndex = 0;

        while (spriteIndex < clipSprites.Length)
        {
            Sprite clipSprite = clipSprites[spriteIndex];
            Sprite setSprite = clipSprite != null ? clipSprite : _renderer.sprite;
            
            _renderer.sprite = setSprite;
            spriteIndex ++;

            yield return new WaitForSeconds(playSpeed);

            if (playClip.loop == false) continue;
            if (spriteIndex < clipSprites.Length) continue;

            spriteIndex = 0;
        }
        
        yield break;
    }
}