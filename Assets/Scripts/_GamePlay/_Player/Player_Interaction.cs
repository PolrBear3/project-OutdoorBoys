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
    [SerializeField][Range(0, 10)] private float _heavyCarryMovementDuration;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _stayTimeCost;


    private GameObject _currentItemPrefab;
    public GameObject currentItemPrefab => _currentItemPrefab;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        Input_Controller input = Input_Controller.instance;
        Movement_Controller playerMovement = _controller.movement;

        input.OnMovement -= playerMovement.MoveTo_Tile;
        input.OnInteract -= StayOn_Tile;

        playerMovement.OnMovementDistanced -= UpdateStatus_OnMovement;
        playerMovement.OnMovementStated -= Update_MovementAnimation;
    }


    // Data
    private void Set_Data()
    {
        Update_IndicationIcon(null);

        Input_Controller input = Input_Controller.instance;
        Movement_Controller playerMovement = _controller.movement;

        input.OnMovement += playerMovement.MoveTo_Tile;
        input.OnInteract += StayOn_Tile;

        playerMovement.OnMovementDistanced += UpdateStatus_OnMovement;
        playerMovement.OnMovementStated += Update_MovementAnimation;
    }

    public void Update_IndicationIcon(Sprite iconSprite)
    {
        bool updateAvailable = iconSprite != null;
        _indicationIcon.gameObject.SetActive(updateAvailable);

        if (updateAvailable == false) return;
        _indicationIcon.sprite = iconSprite;
    }


    // Actions
    public void Load_ItemPrefab(GameObject itemPrefab)
    {
        if (itemPrefab == null && _currentItemPrefab == null) return;

        if (itemPrefab == null)
        {
            Destroy(_currentItemPrefab);
            _currentItemPrefab = null;

            return;
        }
        _currentItemPrefab = Instantiate(itemPrefab, transform);
    }

    private void StayOn_Tile()
    {
        InGame_Manager.instance.time.Update_Data(_stayTimeCost);
    }


    private void Update_MovementAnimation(bool isMoving)
    {
        AnimationPlayer animPlayer = _controller.animationPlayer;

        int animIndexNum = isMoving ? 1 : 0;
        animPlayer.Play(animIndexNum);
    }

    private void UpdateStatus_OnMovement(int moveDistance)
    {
        InGame_Manager manager = InGame_Manager.instance;

        ItemData currentItem = manager.cursor.itemCursor.data;
        bool hasInventoryBagpack = currentItem != null && currentItem.itemScrObj == _controller.inventoryBagpack;

        int currentInventoryWeight = hasInventoryBagpack ? manager.inventory.slotManager.Total_ItemWeight() : 0;
        int currentItemWeight = currentItem != null ? currentItem.Item_Weight() + currentInventoryWeight : 0;

        manager.time.Update_Data(moveDistance + currentItemWeight * moveDistance);
        
        Movement_Controller playerMovement = _controller.movement;
        bool isHeavyMovement = currentItemWeight > _controller.data.maxItemCarryWeight / 2;

        playerMovement.Update_MoveDurationValue(isHeavyMovement ? _heavyCarryMovementDuration : 0);
    }
}
