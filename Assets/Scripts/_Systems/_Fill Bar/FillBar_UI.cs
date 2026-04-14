using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillBar_UI : MonoBehaviour
{
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
    public void Update_Fill(int maxValue, int currentValue)
    {
        if (_fillImage == null) return;
        if (maxValue <= 0) return;

        RectTransform rect = _fillImage.rectTransform;

        float fillSize = Mathf.Clamp01((float)currentValue / maxValue);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _maxFillWidth * fillSize);
    }
}
