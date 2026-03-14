using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;

    [SerializeField] private AnimationClipScrObj[] _animationClips;


    private ClipSpriteData _defaultData;
    private Coroutine _playCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        _defaultData = new(_spriteRenderer.sprite, _spriteRenderer.sortingLayerID, _spriteRenderer.color.a, transform.localPosition);
    }
    

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

    public bool Animation_Playing()
    {
        return _playCoroutine != null;
    }


    // Main
    public void Stop()
    {
        if (_playCoroutine == null) return;

        StopCoroutine(_playCoroutine);
        _playCoroutine = null;

        GameObject animObject = _spriteRenderer.gameObject;
        LeanTween.cancel(animObject);

        LeanTween.alpha(animObject, _defaultData.alphaUpdateValue, 0f);

        _spriteRenderer.sprite = _defaultData.clipSprite;
        _spriteRenderer.sortingOrder = _defaultData.sortingOrderUpdateValue;

        Transform transform = _spriteRenderer.transform;

        transform.localPosition = _defaultData.offSetPosition;
        transform.rotation = Quaternion.identity;
    }


    public void Play(AnimationClipScrObj clip)
    {
        Stop();

        if (clip == null) return;
        if (clip.clipSpriteDatas.Length <= 0) return;

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
                
                GameObject animObject = _spriteRenderer.gameObject;
                
                _spriteRenderer.sprite = dataSprite != null ? dataSprite : _spriteRenderer.sprite;
                _spriteRenderer.sortingOrder = _defaultData.sortingOrderUpdateValue + data.sortingOrderUpdateValue;
                
                LeanTween.alpha(animObject, _defaultData.alphaUpdateValue + data.alphaUpdateValue, data.Alpha_DurationTime());

                float transformDuration = data.Transform_DurationTime();

                LeanTween.moveLocal(animObject, _defaultData.offSetPosition + data.offSetPosition, transformDuration);
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
        _spriteRenderer.flipX = direction.x < 0;
    }
}