using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUI_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _dayText;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _hungerText;
    [SerializeField] private TextMeshProUGUI _temperatureText;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        time.OnTimeCount -= Update_TimeText;
        time.OnTimeCountDataUpdate -= Update_TimeText;
        time.OnDayCount -= Update_DayText;

        Player_Controller player = manager.player;

        player.OnHungerUpdate -= Update_HungerText;
        player.OnTemperatureUpdate -= Update_TemperatureText;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;

        Time_Manager time = manager.time;
        TimeData timeData = time.data;

        Update_TimeText(timeData.timeCount);
        time.OnTimeCount += Update_TimeText;
        time.OnTimeCountDataUpdate += Update_TimeText;

        Update_DayText(timeData.dayCount);
        time.OnDayCount += Update_DayText;

        Player_Controller player = manager.player;
        PlayerData playerData = player.data;

        Update_HungerText(playerData.hunger);
        Update_TemperatureText(playerData.temperature);

        player.OnHungerUpdate += Update_HungerText;
        player.OnTemperatureUpdate += Update_TemperatureText;
    }


    // Text
    private void Update_TimeText(int timeCount)
    {
        int timeCountUpdateValue = InGame_Manager.instance.time.Total_TimeCountSum();
        _timeText.text = timeCount + " (" + "+" + timeCountUpdateValue + ")".ToString();
    }
    private void Update_TimeText()
    {
        Update_TimeText(InGame_Manager.instance.time.data.timeCount);
    }

    private void Update_DayText(int dayCount) => _dayText.text = "Day " + dayCount.ToString();

    private void Update_HungerText(int hungerValue) => _hungerText.text = hungerValue.ToString();
    private void Update_TemperatureText(int tempValue) => _temperatureText.text = tempValue.ToString();
}
