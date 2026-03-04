using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [Space(20)]
    [SerializeField] EventPointer _eventPointer;

    [Space(20)]
    [SerializeField] private Image _itemImage;
    public Image itemImage => _itemImage;

    [SerializeField] private TextMeshProUGUI _amountText;


    private ItemSlot_Manager _slotManager;

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

        _eventPointer.OnEnter -= UpdateManager_HoveringSlot;
        _eventPointer.OnExit -= UpdateManager_HoveringSlot;
    }


    // Data
    private void Set_Data()
    {
        _eventPointer.OnEnter += UpdateManager_HoveringSlot;
        _eventPointer.OnExit += UpdateManager_HoveringSlot;
    }
    public void Set_Data(ItemSlot_Manager setManager)
    {
        _slotManager = setManager;
    }


    public void Set_Data(ItemData setData)
    {
        _data = setData;
    }

    public void UpdateManager_HoveringSlot()
    {
        bool isTracking = _slotManager.slots.Contains(this) && _eventPointer.pointerDetected;
        _slotManager.Update_HoveringSlot(isTracking ? this : null);
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