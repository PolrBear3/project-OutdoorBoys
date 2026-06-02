using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData_UpdateItem : MonoBehaviour
{
    [SerializeField] private UseableItem _useableItem;

    [Space(20)]
    [SerializeField] private PlayerData_Modifier _playerDataModifier;


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
        _useableItem.Update_UseAmount(1);
    }
}
