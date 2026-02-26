using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _itemImage;
    public Image itemImage => _itemImage;


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
}
