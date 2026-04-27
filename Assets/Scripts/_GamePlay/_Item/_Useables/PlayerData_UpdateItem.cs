using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData_UpdateItem : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;

    [Space(20)]
    [SerializeField][Range(-10, 10)] private int _healthUpdateValue;
    [SerializeField][Range(-10, 10)] private int _tempUpdateValue;


    // MonoBehaviour
    private void Awake()
    {
        _useableItem.OnUse += Consume;
    }

    private void OnDestroy()
    {
        _useableItem.OnUse -= Consume;
    }


    // Use
    private void Consume(Tile useTile)
    {
        Player_Controller player = InGame_Manager.instance.player;
        if (useTile != player.movement.tileTracker.data.CurrentTile()) return;

        PlayerData data = player.data;

        player.Update_Health(data.health + _healthUpdateValue);
        player.Update_Temperature(data.temperature + _tempUpdateValue);

        _useableItem.Update_UseAmount(1);
    }
}
