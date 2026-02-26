using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;

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
    private AnimationClipScrObj AnimationClip(int clipIndexNum)
    {
        int clipIndex = Mathf.Clamp(clipIndexNum, 0, _animationClips.Length - 1);

        return _animationClips[clipIndex];
    }


    // Main
    public void Stop()
    {
        if (_playCoroutine == null) return;

        StopCoroutine(_playCoroutine);
        _playCoroutine = null;
    }

    public void Play(int clipIndexNum)
    {
        if (_animationClips == null) return;
        AnimationClipScrObj playClip = AnimationClip(clipIndexNum);

        if (playClip == null) return;
        ClipSpriteData[] spriteDatas = playClip.clipSpriteDatas;

        Stop();

        if (spriteDatas.Length <= 1)
        {
            for (int i = 0; i < spriteDatas.Length; i++)
            {
                if (spriteDatas[i].clipSprite == null) continue;

                _spriteRenderer.sprite = spriteDatas[i].clipSprite;
                return;
            }

            _spriteRenderer.sprite = playClip.defaultSprite;
            return;
        }

        _playCoroutine = StartCoroutine(Play_AnimationClip(playClip));
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

                _spriteRenderer.sprite = spriteDatas[i].clipSprite;
                return;
            }

            _spriteRenderer.sprite = playClip.defaultSprite;
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
                _spriteRenderer.sprite = clipSprite;

                yield return new WaitForSeconds(spriteDatas[i].DurationTime());
            }
        }
        while (playClip.loop);

        yield break;
    }
}