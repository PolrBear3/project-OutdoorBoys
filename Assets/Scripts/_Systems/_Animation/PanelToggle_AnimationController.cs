using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelToggle_AnimationController : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private RectTransform _togglePanel;
    public RectTransform togglePanel => _togglePanel;

    [Space(20)]
    [SerializeField] private LeanTweenType _tweenType;
    [SerializeField][Range(0, 10)] private float _duration;

    [Space(10)]
    [SerializeField] private Vector2 _startingScale;
    [SerializeField] private Vector2[] _toggleScales;

    private Coroutine _animationCoroutine;


    // Main
    public void Toggle(bool toggle)
    {
        if (toggle == _togglePanel.gameObject.activeSelf) return;
        
        if (toggle == false)
        {
            _togglePanel.gameObject.SetActive(false);
            return;
        }
        
        if (_animationCoroutine != null)
        {
            LeanTween.cancel(_togglePanel.gameObject);
            _togglePanel.localScale = new(1, 1);
            
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
        _animationCoroutine = StartCoroutine(ToggleAnimation_Update());
    }

    public void Update_ToggleAnimation()
    {
        if (_animationCoroutine != null)
        {
            LeanTween.cancel(_togglePanel.gameObject);
            _togglePanel.localScale = new(1, 1);

            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
        _animationCoroutine = StartCoroutine(ToggleAnimation_Update());
    }
    private IEnumerator ToggleAnimation_Update()
    {
        float stepDelayTime = _duration / _toggleScales.Length;

        _togglePanel.localScale = _startingScale;
        _togglePanel.gameObject.SetActive(true);

        for (int i = 0; i < _toggleScales.Length; i++)
        {
            LeanTween.scale(_togglePanel, _toggleScales[i], stepDelayTime).setEase(_tweenType);
            yield return new WaitForSeconds(stepDelayTime);
        }

        _togglePanel.localScale = new(1, 1);
        _animationCoroutine = null;
    }
}
