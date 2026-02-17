using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    private Item_ScrObj _itemScrObj;
    public Item_ScrObj itemScrObj => _itemScrObj;
    
    private int _count;
    public int count => _count;


    // Constructor
    public ItemData(Item_ScrObj setItem, int setCount)
    {
        _itemScrObj = setItem;
        _count = Mathf.Max(0, setCount);
    }


    // Current Data
    public void Update_CurrentCount(int updateCount)
    {
        _count = Mathf.Max(0, updateCount);
    }
}