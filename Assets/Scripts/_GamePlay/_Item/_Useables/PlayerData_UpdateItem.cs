using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData_UpdateItem : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;

    [Space(20)]
    [SerializeField] private PlayerData_Modifier _playerDataModifier;

    [Space(10)]
    [SerializeField][Range(0, 10)] private float _coolTimeDecreaseValue;
    [SerializeField][Range(0, 100)] private int _timeCountDuration;


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

        _playerDataModifier.Update_Data();
        player.data.Update_CoolTimeDecrease(new(_useableItem.data.itemScrObj, _timeCountDuration), _coolTimeDecreaseValue);

        _useableItem.Update_UseAmount(1);
    }
}
