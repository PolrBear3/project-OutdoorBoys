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
    [SerializeField] private TextMeshProUGUI _staminaText;


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

        time.UnRegister(TimeUpdateBus.AwakeUpdate, Update_TimeText);
        time.OnDayCount -= Update_DayText;

        Player_Controller player = manager.player;

        player.OnHungerUpdate -= Update_HungerText;
        player.OnTemperatureUpdate -= Update_TemperatureText;
        
        player.OnStaminaUpdate -= Update_StaminaText;
        player.OnStaminaUpdate -= Update_HungerText;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnSetData -= Update_HungerText;
        itemCursor.OnSetData -= Update_StaminaText;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;

        Time_Manager time = manager.time;
        TimeData timeData = time.data;

        Update_TimeText(timeData.timeCount);
        time.Register(TimeUpdateBus.AwakeUpdate, Update_TimeText);

        Update_DayText(timeData.dayCount);
        time.OnDayCount += Update_DayText;

        Player_Controller player = manager.player;
        PlayerData playerData = player.data;

        Update_HungerText(playerData.hunger);
        Update_TemperatureText(playerData.temperature);
        Update_StaminaText(playerData.maxStamina, playerData.currentStamina);

        player.OnHungerUpdate += Update_HungerText;
        player.OnTemperatureUpdate += Update_TemperatureText;

        player.OnStaminaUpdate += Update_StaminaText;
        player.OnStaminaUpdate += Update_HungerText;

        ItemCursor itemCursor = manager.cursor.itemCursor;

        itemCursor.OnSetData += Update_HungerText;
        itemCursor.OnSetData += Update_StaminaText;
    }


    // Text
    private void Update_TimeText(int timeCount)
    {
        int rewardTargetTime = InGame_Manager.instance.time.data.rewardTargetTime;
        _timeText.text = timeCount + " (" + "<sprite=0> " + rewardTargetTime + ")".ToString();
    }
    private void Update_TimeText()
    {
        Update_TimeText(InGame_Manager.instance.time.data.timeCount);
    }
    
    private void Update_DayText(int dayCount)
    {
        _dayText.text = "Day " + dayCount.ToString();
    }

    private void Update_HungerText(int currentValue)
    {
        Player_Controller player = InGame_Manager.instance.player;

        int decreaseValue = player.data.currentStamina - player.interaction.Movement_StaminaValue();
        string decreaseString = decreaseValue < 0 ? "\n(" + decreaseValue + ")" : null;

        _hungerText.text = currentValue + decreaseString;
    }
    private void Update_HungerText(int _, int __)
    {
        Update_HungerText();
    }
    private void Update_HungerText()
    {
        Update_HungerText(InGame_Manager.instance.player.data.hunger);
    }

    private void Update_TemperatureText(int currentValue) => _temperatureText.text = currentValue.ToString();

    private void Update_StaminaText(int maxValue, int currentValue)
    {
        Player_Controller player = InGame_Manager.instance.player;

        int decreaseValue = Mathf.Min(player.data.currentStamina, player.interaction.Movement_StaminaValue());
        string decreaseString = decreaseValue > 0 ? "\n(-" + decreaseValue + ")" : null;

        _staminaText.text = currentValue + "/" + maxValue + decreaseString;
    }
    private void Update_StaminaText()
    {
        PlayerData playerData = InGame_Manager.instance.player.data;
        Update_StaminaText(playerData.maxStamina, playerData.currentStamina);
    }
}
