using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField][Range(0, 100)] private int _upcomingUpdateCoolTime;

    [Space(10)]
    [SerializeField] private Weather_ScrObj[] _mapWeatherEvents;

    private Dictionary<Weather_ScrObj, WeatherEvent> _activeEvents = new();
    public Dictionary<Weather_ScrObj, WeatherEvent> activeEvents => _activeEvents;

    private const int _maxUpcomingEventCount = 3;
    private int _currentCooltime;

    [Space(40)]
    [SerializeField] private List<WeatherEvent_Data> _upcomingEventDatas = new();
    [SerializeField] private List<WeatherEvent_Data> _runningEventDatas = new();


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_ActivePrefabs);

        Time_Manager time = InGame_Manager.instance.time;

        time.Register(ActionUpdateBus.AwakeUpdate, Update_RunningWeathers);

        time.Register(ActionUpdateBus.AwakeUpdate, Update_UpcomingWeathers);
        time.Register(ActionUpdateBus.AwakeUpdate, Update_UpcomingWeatherInfo);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_ActivePrefabs);

        Time_Manager time = InGame_Manager.instance.time;

        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_RunningWeathers);

        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_UpcomingWeathers);
        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_UpcomingWeatherInfo);
    }


    // Data
    private void Set_ActivePrefabs()
    {
        for (int i = 0; i < _mapWeatherEvents.Length; i++)
        {
            Weather_ScrObj eventScrObj = _mapWeatherEvents[i];

            GameObject prefabToSpawn = eventScrObj.activePrefab;
            if (prefabToSpawn == null) continue;

            GameObject eventPrefab = Instantiate(prefabToSpawn, transform);
            if (eventPrefab.TryGetComponent(out WeatherEvent weatherEvent) == false) continue;

            _activeEvents.Add(eventScrObj, weatherEvent);
        }
    }


    // Update
    private bool Update_CoolTime()
    {
        if (_upcomingEventDatas.Count >= _maxUpcomingEventCount) return false;

        _currentCooltime++;
        if (_currentCooltime < _upcomingUpdateCoolTime) return false;

        _currentCooltime = 0;
        return true;
    }

    private Weather_ScrObj RandomWeight_WeatherScrObj()
    {
        Time_Manager time = InGame_Manager.instance.time;

        int currentDayCount = time.data.dayCount;
        int currentTimeCount = time.data.timeCount;

        List<Weather_ScrObj> availableWeathers = new();

        foreach (var activeEvent in _activeEvents)
        {
            Weather_ScrObj weatherToAdd = activeEvent.Key;
            if (Running_WeatherEventData(weatherToAdd) != null) continue;

            bool upcomingFound = false;

            for (int i = 0; i < _upcomingEventDatas.Count; i++)
            {
                if (weatherToAdd != _upcomingEventDatas[i].weather) continue;

                upcomingFound = true;
                break;
            }
            if (upcomingFound) continue;

            if (currentDayCount < weatherToAdd.activeDayCount) continue;
            if (currentTimeCount < weatherToAdd.activeTimeCount) continue;

            availableWeathers.Add(weatherToAdd);
        }

        if (availableWeathers.Count <= 0) return null;

        int totalWeight = 0;
        for (int i = 0; i < availableWeathers.Count; i++)
        {
            totalWeight += Mathf.Max(0, availableWeathers[i].randomWeightValue);
        }

        int randomValue = Random.Range(0, totalWeight);
        for (int i = 0; i < availableWeathers.Count; i++)
        {
            Weather_ScrObj weather = availableWeathers[i];
            int weight = Mathf.Max(0, weather.randomWeightValue);

            if (randomValue < weight) return weather;
            randomValue -= weight;
        }

        return availableWeathers[Random.Range(0, availableWeathers.Count)];
    }
    private void Update_UpcomingWeathers()
    {
        for (int i = _upcomingEventDatas.Count - 1; i >= 0; i--)
        {
            WeatherEvent_Data eventData = _upcomingEventDatas[i];
            Weather_ScrObj weather = eventData.weather;

            if (eventData.timeCount <= 1)
            {
                _runningEventDatas.Add(new(weather, weather.Random_ActiveTime()));
                _upcomingEventDatas.RemoveAt(i);
                continue;
            }
            eventData.Update_TimeCount(eventData.timeCount - 1);
        }

        Weather_ScrObj updateEvent = RandomWeight_WeatherScrObj();

        if (updateEvent == null) return;
        if (Update_CoolTime() == false) return;

        _upcomingEventDatas.Add(new(updateEvent, updateEvent.Random_ActiveTime()));
    }

    private void Update_UpcomingWeatherInfo()
    {
        InGameUI_Manager uiManager = InGame_Manager.instance.ingameUI;

        if (_upcomingEventDatas.Count <= 0)
        {
            uiManager.Toggle_MainHoverPanel(false);
            return;
        }

        WeatherEvent_Data nearestUpcomingData = null;
        int nearestUpcomingTime = int.MaxValue;

        for (int i = 0; i < _upcomingEventDatas.Count; i++)
        {
            WeatherEvent_Data data = _upcomingEventDatas[i];
            int remainingTime = data.timeCount;

            if (remainingTime >= nearestUpcomingTime) continue;

            nearestUpcomingData = data;
            nearestUpcomingTime = remainingTime;
        }

        if (nearestUpcomingData == null) return;

        Weather_ScrObj weather = nearestUpcomingData.weather;
        _activeEvents[weather].Update_EventPreview();

        string weatherInfo = weather.upcomingInfoText + ": <sprite=0> " + nearestUpcomingTime;

        uiManager.Update_MainHoverPanelText(weatherInfo);
        uiManager.Update_HoverInfoText(weather.runningInfoText);
    }

    private WeatherEvent_Data Running_WeatherEventData(Weather_ScrObj checkWeather)
    {
        for (int i = 0; i < _runningEventDatas.Count; i++)
        {
            WeatherEvent_Data data = _runningEventDatas[i];

            if (checkWeather != data.weather) continue;
            return data;
        }
        return null;
    }
    private void Update_RunningWeathers()
    {
        for (int i = _runningEventDatas.Count - 1; i >= 0; i--)
        {
            WeatherEvent_Data eventData = _runningEventDatas[i];
            _activeEvents[eventData.weather].Activate_Event();

            if (eventData.timeCount <= 1)
            {
                _runningEventDatas.RemoveAt(i);
                continue;
            }
            eventData.Update_TimeCount(eventData.timeCount - 1);
        }
    }
}