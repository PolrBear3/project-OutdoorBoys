using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeatherEvent : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Tile_Indicator _tileIndicator;
    public Tile_Indicator tileIndicator => _tileIndicator;
    
    private const float _indicatorToggleDelayTime = 1;
    private Coroutine _toggleDelayCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        InGame_Manager.instance.ingameUI.mainHoverPanelPointer.OnPointerState += Toggle_TileIndicator;
    }
    
    private void OnDestroy()
    {
        InGame_Manager.instance.ingameUI.mainHoverPanelPointer.OnPointerState -= Toggle_TileIndicator;
    }
    

    // Main
    public abstract void Update_EventPreview();
    public abstract void Activate_Event();

    public void Toggle_TileIndicator(bool toggle)
    {
        if (_toggleDelayCoroutine != null)
        {
            StopCoroutine(_toggleDelayCoroutine);
            _toggleDelayCoroutine = null;
        }

        if (toggle || InGame_Manager.instance.ingameUI.mainHoverPanelPointer.pointerDetected)
        {
            _tileIndicator.Toggle_CurrentIndicators(true);
            return;
        }
        _toggleDelayCoroutine = StartCoroutine(IndicatorToggle_DelayUpdate());
    }
    private IEnumerator IndicatorToggle_DelayUpdate()
    {
        yield return new WaitForSeconds(_indicatorToggleDelayTime);
        _tileIndicator.Toggle_CurrentIndicators(false);

        _toggleDelayCoroutine = null;
    }
}