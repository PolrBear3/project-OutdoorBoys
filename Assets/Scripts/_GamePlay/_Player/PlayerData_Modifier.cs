using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData_Modifier : MonoBehaviour
{
    [Space(20)]
    [SerializeField][Range(-100, 100)] private int _healthUpdateValue;
    public int healthUpdateValue => _healthUpdateValue;

    [SerializeField][Range(-100, 100)] private int _hungerUpdateValue;
    public int hungerUpdateValue => _hungerUpdateValue;

    [SerializeField][Range(-100, 100)] private int _temperatureUpdateValue;
    public int temperatureUpdateValue => _temperatureUpdateValue;

    [SerializeField][Range(-100, 100)] private int _staminaUpdateValue;
    public int staminaUpdateValue => _staminaUpdateValue;


    public void Update_Data()
    {
        Player_Controller player = InGame_Manager.instance.player;
        PlayerData data = player.data;

        player.Update_Health(data.health + _healthUpdateValue);
        player.Update_Hunger(data.hunger + _hungerUpdateValue);
        player.Update_Temperature(data.temperature + _temperatureUpdateValue);
        player.Update_Stamina(data.stamina + _staminaUpdateValue);
    }
}
