using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeatherUI_Icon : MonoBehaviour
{
    private Weather_Manager _manager;
    
    [Space(20)]
    [SerializeField] private EventPointer _eventPointer;
    public EventPointer eventPointer => _eventPointer;

    [SerializeField] private Image _image;

    private WeatherEvent_Data _data;
    public WeatherEvent_Data data => _data;


    // MonoBehaviour
    private void Awake()
    {
        _eventPointer.OnPointerState += Update_OnHover;
    }
    
    private void OnDestroy()
    {
        _eventPointer.OnPointerState -= Update_OnHover;
    }


    // Data
    public void Load_Manager(Weather_Manager loadManager)
    {
        _manager = loadManager;
    }

    public void Set_Data(WeatherEvent_Data data)
    {
        _data = data;
    }


    // EventPointer
    public void Update_OnHover(bool hovering)
    {
        _manager.OnIconHover?.Invoke(hovering ? this : null);
    }


    // Visuals
    public void Update_Visuals()
    {
        _image.sprite = _data.weather.iconSprite;
    }

    public void Update_ActivateAnimation(float animateDuration)
    {
        StartCoroutine(ActivateAnimation_Update(animateDuration));
    }
    private IEnumerator ActivateAnimation_Update(float animateDuration)
    {
        animateDuration /= 2;
        LeanTweenType tweenType = LeanTweenType.easeOutElastic;

        LeanTween.scale(gameObject, new(1.5f, 1.5f), animateDuration).setEase(tweenType);
        
        yield return new WaitForSeconds(animateDuration);
        LeanTween.scale(gameObject, new(1f, 1f), animateDuration).setEase(tweenType);
    }
}