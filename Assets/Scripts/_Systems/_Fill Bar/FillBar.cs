using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _fillRenderer;
    public SpriteRenderer fillRenderer => _fillRenderer;


    // Main
    public void Update_Fill(int maxValue, int currentValue)
    {
        if (_fillRenderer == null) return; 
        if (maxValue <= 0) return;
        
        Vector3 currentScale = _fillRenderer.transform.localScale;

        float fillSize = (float)currentValue / maxValue;
        currentScale.x = Mathf.Clamp01(fillSize);

        _fillRenderer.transform.localScale = currentScale;
    }
}