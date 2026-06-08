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

    [Space(10)]
    [SerializeField] private FillBar_UI _durabilityBar;


    private ItemSlot_Manager _slotManager;

    private ItemData _data;
    public ItemData data => _data;

    private const float _transparencyToggleValue = 0.1f;


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
        _data = setData != null && setData.amount > 0 ? setData : null;
    }
    public void Clear_Data()
    {
        _data = null;
    }

    public void UpdateManager_HoveringSlot()
    {
        if (_slotManager == null) return;

        bool isTracking = _slotManager.slots.Contains(this) && _eventPointer.pointerDetected;
        ItemSlot updateSlot = isTracking ? this : null;

        _slotManager.Update_HoveringSlot(updateSlot);
        _slotManager.OnSlotHover?.Invoke(updateSlot);
    }


    // Visuals
    public void Update_ItemImage()
    {
        _itemImage.gameObject.SetActive(_data != null);
        _itemImage.sprite = _data?.itemScrObj.inventorySprite;
    }

    public void Update_AmountText()
    {
        _durabilityBar.gameObject.SetActive(false);

        bool textToggle = _data != null && _data.amount > 1;
        _amountText.gameObject.SetActive(textToggle);

        if (textToggle == false) return;
        _amountText.text = _data.amount.ToString();
    }

    public void Update_AmountIndications()
    {
        _amountText.gameObject.SetActive(false);

        GameObject durabilityBar = _durabilityBar.gameObject;
        durabilityBar.SetActive(false);

        if (_data == null) return;
        Item_ScrObj currentItem = _data.itemScrObj;

        if (currentItem.itemType == ItemType.use)
        {
            _durabilityBar.gameObject.SetActive(true);
            _durabilityBar.Update_Visuals(currentItem.maxAmount, _data.amount);

            return;
        }
        Update_AmountText();
    }


    public void Toggle_Transparency(bool toggle)
    {
        float toggleValue = toggle ? _transparencyToggleValue : 1f;

        Color color = _itemImage.color;
        color.a = toggleValue;

        _itemImage.color = color;
    }
}