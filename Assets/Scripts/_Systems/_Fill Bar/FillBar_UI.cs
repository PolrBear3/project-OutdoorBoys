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


    private float _maxFillWidth;


    // MonoBehaviour
    private void Awake()
    {
        if (_fillImage == null) return;

        _maxFillWidth = _fillImage.rectTransform.rect.width;
    }


    // Main
    public void Update_Visuals(int maxValue, int currentValue)
    {
        if (_fillImage == null) return;
        if (maxValue <= 0) return;

        _barImage.sprite = _barSprites[currentValue > 1 ? 0 : 1];

        RectTransform rect = _fillImage.rectTransform;

        float fillSize = currentValue > 1 ? Mathf.Clamp01((float)currentValue / maxValue) : 0;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _maxFillWidth * fillSize);
    }
}
