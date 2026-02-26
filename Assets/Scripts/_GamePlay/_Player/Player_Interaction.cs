using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Interaction : MonoBehaviour
{
    [SerializeField] private Player_Controller _controller;

    [Space(20)]
    [SerializeField] private SpriteRenderer _indicationIcon;
    public SpriteRenderer indicationIcon => _indicationIcon;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _skipTimeCost;


    // MonoBehaviour
    private void Awake()
    {
        Update_IndicationIcon(null);

        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller.instance.OnInteract -= StayOn_Tile;
    }


    // Data
    private void Set_Data()
    {
        Input_Controller.instance.OnInteract += StayOn_Tile;
    }

    public void Update_IndicationIcon(Sprite iconSprite)
    {
        bool updateAvailable = iconSprite != null;
        _indicationIcon.gameObject.SetActive(updateAvailable);

        if (updateAvailable == false) return;
        _indicationIcon.sprite = iconSprite;
    }


    // Actions
    private void StayOn_Tile()
    {
        InGame_Manager.instance.time.Update_Data(_skipTimeCost);
    }
}
