using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData_ModifierData
{
    [SerializeField][Range(-100, 100)] private int _healthUpdateValue;
    public int healthUpdateValue => _healthUpdateValue;

    [SerializeField][Range(-100, 100)] private int _hungerUpdateValue;
    public int hungerUpdateValue => _hungerUpdateValue;

    [SerializeField][Range(-100, 100)] private int _temperatureUpdateValue;
    public int temperatureUpdateValue => _temperatureUpdateValue;

    [SerializeField][Range(-100, 100)] private int _staminaUpdateValue;
    public int staminaUpdateValue => _staminaUpdateValue;
}

public class PlayerData_Modifier : MonoBehaviour
{
    [Space(20)] 
    [SerializeField] private PlayerData_ModifierData _modityData;
    public PlayerData_ModifierData modifyData => _modityData;

    public void Update_Data(PlayerData_ModifierData updateData)
    {
        Player_Controller player = InGame_Manager.instance.player;
        PlayerData data = player.data;

        player.Update_Health(data.health + updateData.healthUpdateValue);
        player.Update_Hunger(data.hunger + updateData.hungerUpdateValue);
        player.Update_Temperature(data.temperature + updateData.temperatureUpdateValue);
        player.Update_Stamina(data.stamina + updateData.staminaUpdateValue);
    }
    public void Update_Data()
    {
        Update_Data(_modityData);
    }
}
