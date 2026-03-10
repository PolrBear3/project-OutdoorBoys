using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUI_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private TextMeshProUGUI _timeText;
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
        
        time.OnTimeCountUpdate -= Update_TimeText;

        Player_Controller player = manager.player;

        player.OnHungerUpdate -= Update_HungerText;
        player.OnTemperatureUpdate -= Update_TemperatureText;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;
        Time_Manager time = manager.time;

        Update_TimeText(time.data.timeCount);
        time.OnTimeCountUpdate += Update_TimeText;

        Player_Controller player = manager.player;
        PlayerData playerData = player.data;

        Update_HungerText(playerData.hunger);
        Update_TemperatureText(playerData.temperature);

        player.OnHungerUpdate += Update_HungerText;
        player.OnTemperatureUpdate += Update_TemperatureText;
    }


    // Text
    private void Update_TimeText(int timeCount) => _timeText.text = timeCount.ToString();

    private void Update_HungerText(int hungerValue) => _hungerText.text = hungerValue.ToString();
    private void Update_TemperatureText(int tempValue) => _temperatureText.text = tempValue.ToString();
}
