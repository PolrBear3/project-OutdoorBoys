using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSynergy_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemSynergy_ScrObj[] _placeTriggerSynergies;
    [SerializeField] private ItemSynergy_ScrObj[] _useTriggerSynergies;
    [SerializeField] private ItemSynergy_ScrObj[] _timeCountTriggerSynergies;


    // Triggers
    private void Trigger_onItemPlace(Tile itemPlacedTile)
    {
        
    }

    private void Trigger_onItemUse(Tile itemUseTile)
    {
        
    }

    private void Trigger_onTimeCount()
    {
        
    }
}