using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Weather_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private PanelToggle_AnimationController _togglePanel;
    [SerializeField] private Tile_Indicator _tileIndicator;

    [Space(20)]
    [SerializeField][Range(0, 10)] private float _updateDelayTime;
    public float updateDelayTime => _updateDelayTime;

    [Space(20)]
    [SerializeField] private WeatherUI_Icon[] _icons;
    [SerializeField] private PanelToggle_AnimationController _descriptionToggleController;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _upcomingUpdateCoolTime;


    private Coroutine _weathersUpdateCoroutine;

    private Dictionary<Weather_ScrObj, WeatherEvent> _currentWeathers = new();
    public Dictionary<Weather_ScrObj, WeatherEvent> currentWeathers => _currentWeathers;


    private int _currentCooltime;
    private List<WeatherEvent_Data> _upcomingWeatherDatas = new();

    private const float _indicatorToggleDelayTime = 1;
    private Coroutine _toggleDelayCoroutine;

    public Action<WeatherUI_Icon> OnIconHover;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_Data);

        Time_Manager time = InGame_Manager.instance.time;

        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_UpcomingWeathers);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_Visuals);

        OnIconHover -= Toggle_Description;
        OnIconHover -= Toggle_TileIndicator;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.Register(ActionUpdateBus.AwakeUpdate, Update_UpcomingWeathers);
        time.Register(ActionUpdateBus.AwakeUpdate, Update_Visuals);

        Weather_ScrObj[] currentMapWeathers = manager.worldMapGenerator.currentWorldMap.weathers;

        if (currentMapWeathers == null)
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < currentMapWeathers.Length; i++)
        {
            Weather_ScrObj weather = currentMapWeathers[i];

            GameObject prefabToSpawn = weather.activePrefab;
            if (prefabToSpawn == null) continue;

            GameObject eventPrefab = Instantiate(prefabToSpawn, transform);
            if (eventPrefab.TryGetComponent(out WeatherEvent weatherEvent) == false) continue;

            _currentWeathers.Add(weather, weatherEvent);
            weatherEvent.Set_Manager(this);
        }

        for (int i = 0; i < _icons.Length; i++)
        {
            _icons[i].Load_Manager(this);
        }
        Toggle_WeatherPanel();

        OnIconHover += Toggle_Description;
        OnIconHover += Toggle_TileIndicator;
    }


    // Update
    public WeatherEvent Current_WeatherEvent(Weather_ScrObj weather)
    {
        foreach (var currentWeather in _currentWeathers)
        {
            if (currentWeather.Key != weather) continue;
            return currentWeather.Value;
        }
        return null;
    }
    public Weather_ScrObj TargetEvent_Weather(WeatherEvent weatherEvent)
    {
        foreach (var currentWeather in _currentWeathers)
        {
            if (currentWeather.Value != weatherEvent) continue;
            return currentWeather.Key;
        }
        return null;
    }

    private WeatherEvent_Data Upcoming_EventData(WeatherEvent_Data searchData)
    {
        for (int i = 0; i < _upcomingWeatherDatas.Count; i++)
        {
            WeatherEvent_Data data = _upcomingWeatherDatas[i];

            if (data != searchData) continue;
            return data;
        }
        return null;
    }


    private bool Update_CoolTime()
    {
        if (EmptyIcons().Count <= 0) return false;

        _currentCooltime++;
        if (_currentCooltime < _upcomingUpdateCoolTime) return false;

        _currentCooltime = 0;
        return true;
    }

    private Weather_ScrObj RandomWeight_WeatherScrObj()
    {
        List<Weather_ScrObj> availableWeathers = new();

        foreach (var activeEvent in _currentWeathers)
        {
            Weather_ScrObj weatherToAdd = activeEvent.Key;
            bool upcomingFound = false;

            for (int i = 0; i < _upcomingWeatherDatas.Count; i++)
            {
                if (weatherToAdd != _upcomingWeatherDatas[i].weather) continue;

                upcomingFound = true;
                break;
            }
            if (upcomingFound) continue;

            TimeRange_Data timeRangeData = weatherToAdd.timeRangeData;

            if (timeRangeData.Is_ActiveDay() == false) continue;
            if (timeRangeData.Is_ActiveTime() == false || timeRangeData.Is_RestrictTime()) continue;

            availableWeathers.Add(weatherToAdd);
        }

        if (availableWeathers.Count <= 0) return null;

        int totalWeight = 0;
        for (int i = 0; i < availableWeathers.Count; i++)
        {
            totalWeight += Mathf.Max(0, availableWeathers[i].rateValue);
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < availableWeathers.Count; i++)
        {
            Weather_ScrObj weather = availableWeathers[i];
            int weight = Mathf.Max(0, weather.rateValue);

            if (randomValue < weight) return weather;
            randomValue -= weight;
        }

        return availableWeathers[UnityEngine.Random.Range(0, availableWeathers.Count)];
    }

    private void Update_UpcomingWeathers()
    {
        InGame_Manager.instance.time.timeUpdateActions.Add(this);
        _weathersUpdateCoroutine = StartCoroutine(UpcomingWeathers_Update());
    }
    private IEnumerator UpcomingWeathers_Update()
    {
        for (int i = _upcomingWeatherDatas.Count - 1; i >= 0; i--)
        {
            WeatherEvent_Data eventData = _upcomingWeatherDatas[i];
            if (eventData == null) continue;

            if (eventData.timeCount <= 1)
            {
                Weather_ScrObj dataWeather = eventData.weather;
                WeatherEvent weatherEvent = Current_WeatherEvent(dataWeather);

                // visuals
                Icon(eventData).Update_ActivateAnimation(_updateDelayTime);
                Update_WarpRenderer(dataWeather);
                ActivateBlink_TileIndicator(weatherEvent);

                // activation
                weatherEvent.Activate_Event();
                weatherEvent.reservedActivationTiles.Clear();

                _upcomingWeatherDatas.RemoveAt(i);

                yield return new WaitForSeconds(_updateDelayTime);
                continue;
            }
            eventData.Update_TimeCount(eventData.timeCount - 1);
        }

        InGame_Manager.instance.time.timeUpdateActions.Remove(this);

        Weather_ScrObj updateEvent = RandomWeight_WeatherScrObj();
        if (updateEvent == null || Update_CoolTime() == false)
        {
            _weathersUpdateCoroutine = null;
            yield break;
        }

        _upcomingWeatherDatas.Add(new(updateEvent, updateEvent.timeRangeData.Random_TimeCount()));
        Current_WeatherEvent(updateEvent).Reserve_ActivationTiles();

        _weathersUpdateCoroutine = null;
    }


    // Visuals
    private List<WeatherUI_Icon> EmptyIcons()
    {
        List<WeatherUI_Icon> emptyIcons = new();

        for (int i = 0; i < _icons.Length; i++)
        {
            WeatherUI_Icon icon = _icons[i];

            if (icon.data != null) continue;
            emptyIcons.Add(icon);
        }
        return emptyIcons;
    }

    private WeatherUI_Icon Icon(WeatherEvent_Data data)
    {
        for (int i = 0; i < _icons.Length; i++)
        {
            WeatherUI_Icon icon = _icons[i];
            WeatherEvent_Data iconData = icon.data;

            if (iconData == null || iconData != data) continue;
            return icon;
        }
        return null;
    }
    private WeatherUI_Icon Hovering_Icon()
    {
        for (int i = 0; i < _icons.Length; i++)
        {
            WeatherUI_Icon icon = _icons[i];

            if (icon.eventPointer.pointerDetected == false) continue;
            return icon;
        }
        return null;
    }


    private void Update_Visuals()
    {
        StartCoroutine(VisualsUpdate_Delay());
    }
    private IEnumerator VisualsUpdate_Delay()
    {
        while (_weathersUpdateCoroutine != null) yield return null;

        Update_Icons();
        Toggle_Description();
        Toggle_TileIndicator();
    }


    private void Update_Icons()
    {
        List<WeatherEvent_Data> newDatas = new();

        for (int i = 0; i < _upcomingWeatherDatas.Count; i++)
        {
            WeatherEvent_Data upcomingData = _upcomingWeatherDatas[i];

            if (Icon(upcomingData) != null) continue;
            newDatas.Add(upcomingData);
        }

        List<WeatherUI_Icon> emptyIcons = EmptyIcons();

        for (int i = 0; i < emptyIcons.Count; i++)
        {
            if (i >= newDatas.Count) break;
            emptyIcons[i].Set_Data(newDatas[i]);
        }

        Refresh_Icons();
    }
    private void Refresh_Icons()
    {
        List<WeatherEvent_Data> currentDatas = new();

        for (int i = 0; i < _icons.Length; i++)
        {
            WeatherUI_Icon icon = _icons[i];

            WeatherEvent_Data data = icon.data;
            if (data == null) continue;

            if (_upcomingWeatherDatas.Contains(data) == false)
            {
                icon.Set_Data(null);
                continue;
            }
            currentDatas.Add(data);
        }

        for (int i = 0; i < _icons.Length; i++)
        {
            WeatherUI_Icon icon = _icons[i];
            WeatherEvent_Data setData = i < currentDatas.Count ? currentDatas[i] : null;

            icon.Set_Data(setData);
            icon.gameObject.SetActive(setData != null);

            if (setData == null) continue;
            icon.Update_Visuals();
        }

        Toggle_WeatherPanel();
    }

    private void Toggle_WeatherPanel()
    {
        _togglePanel.Toggle(EmptyIcons().Count < _icons.Length);
    }

    private void Toggle_Description(WeatherUI_Icon hoveringIcon)
    {
        bool toggle = hoveringIcon != null && hoveringIcon.data != null;
        _descriptionToggleController.Toggle(toggle);

        if (toggle == false) return;

        WeatherEvent_Data hoveringData = Upcoming_EventData(hoveringIcon.data);
        if (hoveringData == null) return;

        Weather_ScrObj weather = hoveringData.weather;
        _descriptionText.text = weather.UpcomingInfo(hoveringData.timeCount) + "\n\n" + Current_WeatherEvent(weather).Description();
    }
    private void Toggle_Description()
    {
        Toggle_Description(Hovering_Icon());
    }


    private void Update_WarpRenderer(Weather_ScrObj updateWeather)
    {
        WarpRenderer_Controller warpRenderer = InGame_Manager.instance.environmentVisuals.backgroundRenderer;
    }

    private void Update_TileIndicator(WeatherEvent targetEvent)
    {
        List<Tile> activationTiles = targetEvent.reservedActivationTiles;

        _tileIndicator.Clear_CurrentIndicators();
        if (activationTiles == null) return;

        for (int i = 0; i < activationTiles.Count; i++)
        {
            _tileIndicator.Set_Indicator(activationTiles[i]);
        }

        _tileIndicator.Update_CurrentVisualDatas(TargetEvent_Weather(targetEvent).activateTileVisuals);
    }

    private void Toggle_TileIndicator(WeatherUI_Icon hoveringIcon)
    {
        if (_weathersUpdateCoroutine != null) return;

        Cancel_IndicatorToggleDelay();

        if (hoveringIcon != null)
        {
            Update_TileIndicator(Current_WeatherEvent(hoveringIcon.data.weather));
            _tileIndicator.Toggle_CurrentIndicators(true);

            return;
        }
        _toggleDelayCoroutine = StartCoroutine(IndicatorToggle_DelayUpdate());
    }
    private void Toggle_TileIndicator()
    {
        Toggle_TileIndicator(Hovering_Icon());
    }

    private IEnumerator IndicatorToggle_DelayUpdate()
    {
        yield return new WaitForSeconds(_indicatorToggleDelayTime);

        _tileIndicator.Clear_CurrentIndicators();
        _toggleDelayCoroutine = null;
    }
    private void Cancel_IndicatorToggleDelay()
    {
        if (_toggleDelayCoroutine == null) return;

        StopCoroutine(_toggleDelayCoroutine);
        _toggleDelayCoroutine = null;
    }

    private void ActivateBlink_TileIndicator(WeatherEvent activateEvent)
    {
        Cancel_IndicatorToggleDelay();
        Update_TileIndicator(activateEvent);

        StartCoroutine(ActivateBlink_Update());
    }
    private IEnumerator ActivateBlink_Update()
    {
        const int blinkCount = 3;
        float blinkDuration = _updateDelayTime / (blinkCount * 2f);

        for (int i = 0; i < blinkCount; i++)
        {
            _tileIndicator.Toggle_CurrentIndicators(true);
            yield return new WaitForSeconds(blinkDuration);

            _tileIndicator.Toggle_CurrentIndicators(false);
            yield return new WaitForSeconds(blinkDuration);
        }
    }
}