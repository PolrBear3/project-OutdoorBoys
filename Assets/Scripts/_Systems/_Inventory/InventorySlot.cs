using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _itemImage;
    public Image itemImage => _itemImage;

    [SerializeField] private TextMeshProUGUI _amountText;


    private ItemData _data;
    public ItemData data => _data;


    // Data
    public void Set_Data(ItemData setData)
    {
        _data = setData;
    }


    // UI
    public void Update_ItemImage()
    {
        bool spriteAvailable = _data != null;
        _itemImage.gameObject.SetActive(spriteAvailable);

        if (spriteAvailable == false) return;
        _itemImage.sprite = _data.itemScrObj.inventorySprite;
    }

    public void Update_AmountText()
    {
        bool toggle = _data != null && _data.amount > 0;
        _amountText.gameObject.SetActive(toggle);

        if (toggle == false) return;
        _amountText.text = _data.amount.ToString();
    }
}
