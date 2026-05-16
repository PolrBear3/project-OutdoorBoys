using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Weather_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Tile_Indicator _tileIndicator;
    [SerializeField] private RectTransform _weatherPanel;

    [Space(10)]
    [SerializeField] private WeatherUI_Icon[] _icons;

    [Space(10)]
    [SerializeField] private RectTransform _descriptionPanel;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _upcomingUpdateCoolTime;


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
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_Icons);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Toggle_Description);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Toggle_TileIndicator);

        OnIconHover -= Toggle_Description;
        OnIconHover -= Toggle_TileIndicator;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.Register(ActionUpdateBus.AwakeUpdate, Update_UpcomingWeathers);
        time.Register(ActionUpdateBus.AwakeUpdate, Update_Icons);
        time.Register(ActionUpdateBus.AwakeUpdate, Toggle_Description);
        time.Register(ActionUpdateBus.AwakeUpdate, Toggle_TileIndicator);

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
    private bool Update_CoolTime()
    {
        if (EmptyIcons().Count <= 0) return false;

        _currentCooltime++;
        if (_currentCooltime < _upcomingUpdateCoolTime) return false;

        _currentCooltime = 0;
        return true;
    }


    public WeatherEvent Current_WeatherEvent(Weather_ScrObj weather)
    {
        foreach (var currentWeather in _currentWeathers)
        {
            if (currentWeather.Key != weather) continue;
            return currentWeather.Value;
        }
        return null;
    }

    private Weather_ScrObj RandomWeight_WeatherScrObj()
    {
        Time_Manager time = InGame_Manager.instance.time;

        int currentDayCount = time.data.dayCount;
        int currentTimeCount = time.data.timeCount;

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

            if (currentDayCount < weatherToAdd.activeDayPoint) continue;
            if (currentTimeCount < weatherToAdd.Active_TimePoint()) continue;
            if (currentTimeCount >= weatherToAdd.Restrict_TimePoint()) continue;

            availableWeathers.Add(weatherToAdd);
        }

        if (availableWeathers.Count <= 0) return null;

        int totalWeight = 0;
        for (int i = 0; i < availableWeathers.Count; i++)
        {
            totalWeight += Mathf.Max(0, availableWeathers[i].randomWeightValue);
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < availableWeathers.Count; i++)
        {
            Weather_ScrObj weather = availableWeathers[i];
            int weight = Mathf.Max(0, weather.randomWeightValue);

            if (randomValue < weight) return weather;
            randomValue -= weight;
        }

        return availableWeathers[UnityEngine.Random.Range(0, availableWeathers.Count)];
    }
    private void Update_UpcomingWeathers()
    {
        for (int i = _upcomingWeatherDatas.Count - 1; i >= 0; i--)
        {
            WeatherEvent_Data eventData = _upcomingWeatherDatas[i];

            if (eventData.timeCount <= 1)
            {
                WeatherEvent weatherEvent = Current_WeatherEvent(eventData.weather);

                weatherEvent.Activate_Event();
                weatherEvent.reservedActivationTiles.Clear();

                _upcomingWeatherDatas.RemoveAt(i);
                continue;
            }
            eventData.Update_TimeCount(eventData.timeCount - 1);
        }

        Weather_ScrObj updateEvent = RandomWeight_WeatherScrObj();

        if (updateEvent == null) return;
        if (Update_CoolTime() == false) return;

        _upcomingWeatherDatas.Add(new(updateEvent, updateEvent.Random_ActiveTime()));
        Current_WeatherEvent(updateEvent).Reserve_ActivationTiles();
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


    // UI
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
        _weatherPanel.gameObject.SetActive(EmptyIcons().Count < _icons.Length);
    }

    private void Toggle_Description(WeatherUI_Icon hoveringIcon)
    {
        bool toggle = hoveringIcon != null && hoveringIcon.data != null;
        _descriptionPanel.gameObject.SetActive(toggle);

        if (toggle == false) return;

        WeatherEvent_Data hoveringData = Upcoming_EventData(hoveringIcon.data);
        Weather_ScrObj weather = hoveringData.weather;

        string upcomingDescription = weather.upcomingInfoText + ": <sprite=0> " + hoveringData.timeCount;
        string weatherDescription = "\n\n" + weather.descriptionText;

        _descriptionText.text = upcomingDescription + weatherDescription;
    }
    private void Toggle_Description()
    {
        Toggle_Description(Hovering_Icon());
    }

    private void Toggle_TileIndicator(WeatherUI_Icon hoveringIcon)
    {
        if (_toggleDelayCoroutine != null)
        {
            StopCoroutine(_toggleDelayCoroutine);
            _toggleDelayCoroutine = null;
        }

        if (hoveringIcon != null)
        {
            WeatherEvent hoveringWeatherEvent = Current_WeatherEvent(hoveringIcon.data.weather);
            List<Tile> activationTiles = hoveringWeatherEvent.reservedActivationTiles;

            _tileIndicator.Clear_CurrentIndicators();
            if (activationTiles == null) return;

            for (int i = 0; i < activationTiles.Count; i++)
            {
                _tileIndicator.Set_Indicator(activationTiles[i]);
            }

            _tileIndicator.Update_CurrentVisualDatas(hoveringWeatherEvent.activateTileVisuals);
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
}