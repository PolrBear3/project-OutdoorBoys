using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCrafting_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private ItemSlot_Manager _slotManager;
    [SerializeField] private Image _togglePanel;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }
    
    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);
    }


    // Component
    private void Set_Data()
    {

    }
}
