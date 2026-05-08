using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weather_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Tile_Indicator _tileIndicator;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Set_TestTiles);
        // InGame_Manager.instance.time.Register(ActionUpdateBus.AwakeUpdate, );
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Set_TestTiles);
        // InGame_Manager.instance.time.UnRegister(ActionUpdateBus.AwakeUpdate, );
    }


    // Visual
    private void Set_TestTiles()
    {

    }
}
