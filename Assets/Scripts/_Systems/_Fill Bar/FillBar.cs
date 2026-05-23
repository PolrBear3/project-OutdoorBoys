using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillBar : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private SpriteRenderer _barRenderer;
    [SerializeField] private Sprite[] _barSprites;
    
    [Space(20)]
    [SerializeField] private SpriteRenderer _fillRenderer;
    public SpriteRenderer fillRenderer => _fillRenderer;


    // Main
    public void Update_Visuals(int maxValue, int currentValue)
    {
        if (_fillRenderer == null) return;
        if (maxValue <= 0) return;

        _barRenderer.sprite = _barSprites[currentValue > 1 ? 0 : 1];

        Vector3 currentScale = _fillRenderer.transform.localScale;
        float fillSize = currentValue > 1 ? Mathf.Max(0.1f, (float)currentValue / maxValue) : 0f;

        currentScale.x = Mathf.Clamp01(fillSize);
        _fillRenderer.transform.localScale = currentScale;
    }
}