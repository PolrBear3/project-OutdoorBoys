using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemInfo_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private PanelToggle_AnimationController _togglePanelController;
    [SerializeField] private ItemSlot _itemImageSlot;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemDescriptionText;


    // Item Info
    public void Toggle_ItemInfoPanel(ItemSlot targetItemSlot)
    {
        bool toggle = targetItemSlot != null && targetItemSlot.data != null;
        _togglePanelController.Toggle(toggle);

        if (toggle == false) return;

        Vector2 panelPos = _togglePanelController.togglePanel.transform.position;
        panelPos.x = targetItemSlot.itemImage.transform.position.x;

        _togglePanelController.togglePanel.transform.position = panelPos;
    }

    public void Update_HoveringItemInfo(ItemSlot targetItemSlot, string descriptionText)
    {
        if (targetItemSlot == null) return;

        Item_ScrObj hoveringItem = targetItemSlot.data?.itemScrObj;
        if (hoveringItem == null) return;

        _itemImageSlot.Set_Data(new ItemData(hoveringItem, 1));
        _itemImageSlot.Update_ItemImage();
        _itemImageSlot.Update_AmountText();

        int hoverAmount = targetItemSlot.data.amount;
        _itemNameText.text = hoveringItem.itemName + " [" + hoverAmount + "/" + hoveringItem.maxAmount + "]";

        _itemDescriptionText.text = descriptionText;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_togglePanelController.togglePanel);
    }
    public void Update_HoveringItemInfo(ItemSlot targetItemSlot)
    {
        if (targetItemSlot == null) return;

        Item_ScrObj hoveringItem = targetItemSlot.data?.itemScrObj;
        
        if (hoveringItem == null) return;
        Update_HoveringItemInfo(targetItemSlot, hoveringItem.description);
    }
}
