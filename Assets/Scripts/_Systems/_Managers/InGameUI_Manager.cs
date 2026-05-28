using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InGameUI_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private TextMeshProUGUI _timeText;
    public TextMeshProUGUI timeText => _timeText;

    [SerializeField] private TextMeshProUGUI _dayText;

    [Space(20)]
    [SerializeField] private FillBar_UI _healthBar;
    [SerializeField] private FillBar_UI _temperatureBar;

    [Space(10)]
    [SerializeField] private FillBar_UI _staminaBar;
    [SerializeField] private FillBar_UI _staminaValueBar;


    [Space(20)]
    [SerializeField][Range(0, 10)] private float _textAnimateDuration;

    private Dictionary<TextMeshProUGUI, Coroutine> _textAnimationDatas = new();


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

        time.UnRegister(ActionUpdateBus.AwakeUpdate, Update_TimeText);
        time.OnDayCount -= Update_DayText;

        Player_Controller player = manager.player;

        player.OnHealthUpdate -= Update_HealthBar;
        player.OnTemperatureUpdate -= Update_TemperatureBar;

        player.OnStaminaUpdate -= Update_StaminaBar;
        manager.cursor.itemCursor.OnSetData -= Update_StaminaBar;
    }


    // Data
    private void Set_Data()
    {
        InGame_Manager manager = InGame_Manager.instance;

        Time_Manager time = manager.time;
        TimeData timeData = time.data;

        Update_TimeText(timeData.timeCount);
        time.Register(ActionUpdateBus.AwakeUpdate, Update_TimeText);

        Update_DayText(timeData.dayCount);
        time.OnDayCount += Update_DayText;

        Player_Controller player = manager.player;
        PlayerData playerData = player.data;

        Update_HealthBar(playerData.health);
        Update_TemperatureBar(playerData.temperature);
        Update_StaminaBar(playerData.stamina);

        player.OnHealthUpdate += Update_HealthBar;
        player.OnTemperatureUpdate += Update_TemperatureBar;

        player.OnStaminaUpdate += Update_StaminaBar;
        manager.cursor.itemCursor.OnSetData += Update_StaminaBar;
    }


    // Time Text
    private void Update_TimeText(int timeCount)
    {
        int rewardTargetTime = InGame_Manager.instance.time.data.rewardTargetTime;

        _timeText.text = "<sprite=0> " + timeCount + " (" + "<sprite=2> " + rewardTargetTime + ")".ToString();
        Update_TextAnimation(_timeText);
    }
    private void Update_TimeText()
    {
        Update_TimeText(InGame_Manager.instance.time.data.timeCount);
    }

    private void Update_DayText(int dayCount)
    {
        _dayText.text = "Day " + dayCount.ToString();
        Update_TextAnimation(_dayText);
    }


    // Player Data Text
    private void Update_HealthBar(int currentValue)
    {
        _healthBar.Update_Visuals(InGame_Manager.instance.player.maxData.health, currentValue, 0);
    }

    private void Update_TemperatureBar(int currentValue)
    {
        _temperatureBar.Update_Visuals(InGame_Manager.instance.player.maxData.temperature, currentValue, 0);
    }

    private void Update_StaminaBar(int currentValue)
    {
        Player_Controller player = InGame_Manager.instance.player;
        Player_Interaction interaction = player.interaction;

        int maxStamina = player.maxData.stamina;
        int barUpdateValue = interaction.Has_Stamina() || currentValue <= 1 ? 0 : maxStamina;

        _staminaBar.Update_Visuals(maxStamina, currentValue, barUpdateValue);
        _staminaValueBar.Update_Visuals(maxStamina, Mathf.Min(currentValue, interaction.Current_StaminaValue()));
    }
    private void Update_StaminaBar()
    {
        Update_StaminaBar(InGame_Manager.instance.player.data.currentStamina);
    }


    // Visual
    private void Update_TextAnimation(TextMeshProUGUI animateText)
    {
        if (_textAnimationDatas.ContainsKey(animateText))
        {
            StopCoroutine(_textAnimationDatas[animateText]);
            _textAnimationDatas.Remove(animateText);
        }
        _textAnimationDatas[animateText] = StartCoroutine(AnimateText_Update(animateText));
    }
    private IEnumerator AnimateText_Update(TextMeshProUGUI targetText)
    {
        GameObject animateText = targetText.gameObject;
        LeanTweenType tweenType = LeanTweenType.easeOutElastic;

        LeanTween.scale(animateText, new(1.5f, 1.5f), _textAnimateDuration).setEase(tweenType);
        yield return new WaitForSeconds(_textAnimateDuration);

        LeanTween.scale(animateText, new(1f, 1f), _textAnimateDuration).setEase(tweenType);
        yield return new WaitForSeconds(_textAnimateDuration);

        _textAnimationDatas.Remove(targetText);
    }
}