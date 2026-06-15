using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillBar_UI : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _barImage;
    [SerializeField] private Sprite[] _barSprites;

    [Space(20)]
    [SerializeField] private Image _fillImage;
    public Image fillImage => _fillImage;


    private bool _isHorizontalBar;

    private float _maxFillSize;
    public float maxFillSize => _maxFillSize;


    // MonoBehaviour
    private void Awake()
    {
        if (_fillImage == null) return;

        Rect fillRect = _fillImage.rectTransform.rect;

        _isHorizontalBar = fillRect.width >= fillRect.height;
        _maxFillSize = _isHorizontalBar ? fillRect.width : fillRect.height;
    }


    // Main
    public void Update_BarVisual(int spriteIndexNum)
    {
        if (_barSprites.Length <= 0) return;
        
        _barImage.sprite = _barSprites[Mathf.Clamp(spriteIndexNum, 0, _barSprites.Length - 1)];
    }
    private void Update_BarVisual(int currentValue, int barSpriteUpdateValue)
    {
        if (_barImage == null) return;
        if (_barImage.sprite == null) return;

        Update_BarVisual(currentValue > barSpriteUpdateValue ? 0 : 1);
    }

    public void Update_Visuals(int maxValue, int currentValue, int barSpriteUpdateValue)
    {
        if (_fillImage == null) return;
        if (maxValue <= 0) return;

        Update_BarVisual(currentValue, barSpriteUpdateValue);

        RectTransform rect = _fillImage.rectTransform;
        float fillSize = currentValue > 0 ? Mathf.Clamp01((float)currentValue / maxValue) : 0;

        rect.SetSizeWithCurrentAnchors(_isHorizontalBar ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical, _maxFillSize * fillSize);
    }
    public void Update_Visuals(int maxValue, int currentValue)
    {
        Update_Visuals(maxValue, currentValue, 1);
    }
}