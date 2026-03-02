using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationPlayerUI : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _image;
    public Image image => _image;

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

                _image.sprite = spriteDatas[i].clipSprite;
                return;
            }

            _image.sprite = clip.defaultSprite;
            return;
        }

        _playCoroutine = StartCoroutine(Play_AnimationClip(clip));
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

    private IEnumerator Play_AnimationClip(AnimationClipScrObj playClip)
    {
        ClipSpriteData[] spriteDatas = playClip.clipSpriteDatas;

        do
        {
            for (int i = 0; i < spriteDatas.Length; i++)
            {
                Sprite clipSprite = spriteDatas[i].clipSprite;

                if (clipSprite == null) continue;
                _image.sprite = clipSprite;

                yield return new WaitForSeconds(spriteDatas[i].DurationTime());
            }
        }
        while (playClip.loop);

        yield break;
    }
}
