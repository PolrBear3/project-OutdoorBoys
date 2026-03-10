using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;

    [Space(10)]
    [SerializeField] private AnimationClipScrObj[] _animationClips;

    private Vector2 _defaultPosition;
    private Coroutine _playCoroutine;


    // Data
    public void Set_DefaultPosition(Vector2 position)
    {
        _defaultPosition = position;
    }

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

        LeanTween.cancel(_spriteRenderer.gameObject);
        Transform transform = _spriteRenderer.transform;

        transform.localPosition = _defaultPosition;
        transform.rotation = Quaternion.identity;
    }


    public void Play(AnimationClipScrObj clip)
    {
        if (clip == null) return;
        ClipSpriteData[] spriteDatas = clip.clipSpriteDatas;

        Stop();

        if (spriteDatas.Length <= 1)
        {
            for (int i = 0; i < spriteDatas.Length; i++)
            {
                if (spriteDatas[i].clipSprite == null) continue;

                _spriteRenderer.sprite = spriteDatas[i].clipSprite;
                return;
            }

            _spriteRenderer.sprite = clip.defaultSprite;
            return;
        }

        _playCoroutine = StartCoroutine(Play_AnimationClip(clip));
    }
    private IEnumerator Play_AnimationClip(AnimationClipScrObj playClip)
    {
        ClipSpriteData[] spriteDatas = playClip.clipSpriteDatas;

        do
        {
            for (int i = 0; i < spriteDatas.Length; i++)
            {
                ClipSpriteData data = spriteDatas[i];

                Sprite dataSprite = data.clipSprite;
                _spriteRenderer.sprite = dataSprite != null ? dataSprite : _spriteRenderer.sprite;

                GameObject animObject = _spriteRenderer.gameObject;
                float transformDuration = data.Transform_DurationTime();

                LeanTween.moveLocal(animObject, _defaultPosition + data.offSetPosition, transformDuration);
                LeanTween.rotateLocal(animObject, new(0f, 0f, data.rotationValue), transformDuration);

                yield return new WaitForSeconds(spriteDatas[i].DurationTime());
            }
        }
        while (playClip.loop);

        Stop();
        yield break;
    }

    public void Play(int clipIndexNum)
    {
        if (_animationClips == null) return;

        AnimationClipScrObj playClip = AnimationClip(clipIndexNum);
        Play(playClip);
    }
    public void Play(string clipName)
    {
        if (_animationClips == null) return;

        AnimationClipScrObj playClip = AnimationClip(clipName);
        Play(playClip);
    }


    public void Update_Flip(Vector2 direction)
    {
        Debug.Log(direction.x);
        _spriteRenderer.flipX = direction.x < 0;
    }
}