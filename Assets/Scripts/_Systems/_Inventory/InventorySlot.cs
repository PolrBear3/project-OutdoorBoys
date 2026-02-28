using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Space(20)]
    [SerializeField] EventPointer _eventPointer;
    
    [Space(20)]
    [SerializeField] private Image _itemImage;
    public Image itemImage => _itemImage;

    [SerializeField] private TextMeshProUGUI _amountText;


    private ItemData _data;
    public ItemData data => _data;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.AwakeLoad, Set_Data);

        _eventPointer.OnEnter -= Update_HoveringState;
        _eventPointer.OnExit -= Update_HoveringState;
    }


    // Data
    private void Set_Data()
    {
        _eventPointer.OnEnter += Update_HoveringState;
        _eventPointer.OnExit += Update_HoveringState;
    }

    public void Set_Data(ItemData setData)
    {
        _data = setData;
    }


    public void Update_HoveringState()
    {
        Inventory_Manager inventory = InGame_Manager.instance.inventory;

        inventory.Track_HoveringSlot(_eventPointer.pointerDetected ? this : null);
    }


    // Visuals
    public void Update_Visuals()
    {
        _itemImage.gameObject.SetActive(_data != null);
        _itemImage.sprite = _data?.itemScrObj.inventorySprite;

        Update_AmountText();
    }

    public void Update_AmountText()
    {
        bool toggleText = _data != null && _data.amount > 0;
        _amountText.gameObject.SetActive(toggleText);

        if (toggleText == false) return;
        _amountText.text = _data.amount.ToString();
    }
}
