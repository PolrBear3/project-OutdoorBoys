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
        ClipSpriteData[] spriteDatas = playClip.clipSpriteDatas;

        Stop();

        if (spriteDatas.Length <= 1)
        {
            for (int i = 0; i < spriteDatas.Length; i++)
            {
                if (spriteDatas[i].clipSprite == null) continue;

                _renderer.sprite = spriteDatas[i].clipSprite;
                return;
            }

            _renderer.sprite = playClip.defaultSprite;
            return;
        }

        _playCoroutine = StartCoroutine(Play_AnimationClip(playClip));
    }

    private IEnumerator Play_AnimationClip(AnimationClipScrObj playClip)
    {
        ClipSpriteData[] spriteDatas = playClip.clipSpriteDatas;

        do
        {
            for (int i = 0; i < spriteDatas.Length; i++)
            {
                Sprite clipSprite = spriteDatas[i].clipSprite;

                if (clipSprite == null) continue;
                _renderer.sprite = clipSprite;

                yield return new WaitForSeconds(spriteDatas[i].DurationTime());
            }
        }
        while (playClip.loop);

        yield break;
    }
}