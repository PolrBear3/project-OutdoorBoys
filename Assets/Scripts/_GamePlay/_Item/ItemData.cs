using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    private Item_ScrObj _itemScrObj;
    public Item_ScrObj itemScrObj => _itemScrObj;
    
    private int _amount;
    public int amount => _amount;


    // Constructor
    public ItemData(Item_ScrObj setItem, int setAmount)
    {
        _itemScrObj = setItem;
        _amount = Mathf.Max(0, setAmount);
    }


    // Data
    public void Update_CurrentAmount(int updateAmount)
    {
        _amount = Mathf.Max(0, updateAmount);
    }


    public int Item_Weight()
    {
        int singleWeight = _itemScrObj.itemWeight;

        if (_itemScrObj.itemType != ItemType.place) return singleWeight;
        return singleWeight * _amount;
    }
}