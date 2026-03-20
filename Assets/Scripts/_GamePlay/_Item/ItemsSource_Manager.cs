using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

public interface IItemsSource
{
    IEnumerable<ItemData> ItemDatas();
}

public interface IItemsSourceAdd
{
    /// <returns>
    /// Total Add Amount
    /// </returns>
    int AddItem(Item_ScrObj updateItem, int addAmount);
}

public interface IItemsSourceRemove
{
    /// <returns>
    /// Total Removed Amount
    /// </returns>
    int RemoveItem(Item_ScrObj updateItem, int removeAmount);
}

public class ItemsSource_Manager : MonoBehaviour
{
    [Space(10)]
    [SerializeField] private MonoBehaviour[] _itemsSourcesComponents;

    private List<IItemsSource> _itemsSources = new();
    public List<IItemsSource> itemsSources => _itemsSources;

    private List<IItemsSourceAdd> _itemsAddSources = new();
    public List<IItemsSourceAdd> itemsAddSources => _itemsAddSources;

    private List<IItemsSourceRemove> _itemsRemoveSources = new();
    public List<IItemsSourceRemove> itemsRemoveSources => _itemsRemoveSources;


    // MonoBehaviour
    private void Awake()
    {
        Load_ItemSources();
    }


    // Load from _itemsSourcesComponents
    private void Load_ItemSources()
    {
        for (int i = 0; i < _itemsSourcesComponents.Length; i++)
        {
            MonoBehaviour component = _itemsSourcesComponents[i];

            if (component is IItemsSource source) _itemsSources.Add(source);
            if (component is IItemsSourceAdd addSource) _itemsAddSources.Add(addSource);
            if (component is IItemsSourceRemove removeSource) _itemsRemoveSources.Add(removeSource);
        }
    }


    // IItemsSource
    public int ItemData_Count(Item_ScrObj targetItem)
    {
        List<ItemData> itemDatas = ItemDatas();

        for (int i = 0; i < itemDatas.Count; i++)
        {
            ItemData data = itemDatas[i];

            if (targetItem != data.itemScrObj) continue;
            return data.amount;
        }
        return 0;
    }
    
    public List<ItemData> ItemDatas(List<IItemsSource> itemSources)
    {
        List<ItemData> sourcesDatas = new();

        foreach (IItemsSource source in itemSources)
        {
            IEnumerable<ItemData> datas = source.ItemDatas();
            if (datas == null) continue;

            foreach (ItemData data in datas)
            {
                if (data == null) continue;

                Item_ScrObj item = data.itemScrObj;
                int amount = data.amount;

                bool amountUpdate = false;

                for (int i = 0; i < sourcesDatas.Count; i++)
                {
                    ItemData currentData = sourcesDatas[i];
                    if (item != currentData.itemScrObj) continue;

                    amountUpdate = true;
                    currentData.Update_CurrentAmount(currentData.amount + amount);

                    break;
                }

                if (amountUpdate) continue;
                sourcesDatas.Add(new(item, amount));
            }
        }
        return sourcesDatas;
    }
    public List<ItemData> ItemDatas()
    {
        return ItemDatas(_itemsSources);
    }

    /// <returns>
    /// Leftover amount
    /// </returns>
    public int RemoveItem(List<IItemsSourceRemove> itemSources, Item_ScrObj removeItem, int removeAmount)
    {
        for (int i = 0; i < itemSources.Count; i++)
        {
            removeAmount -= itemSources[i].RemoveItem(removeItem, removeAmount);
            if (removeAmount <= 0) break;
        }
        return removeAmount;
    }
    /// <returns>
    /// Leftover amount
    /// </returns>
    public int RemoveItem(Item_ScrObj removeItem, int removeAmount)
    {
        return RemoveItem(_itemsRemoveSources, removeItem, removeAmount);
    }

    /// <returns>
    /// Amount overflow leftover amount
    /// </returns>
    public int AddItem(List<IItemsSourceAdd> itemSources, Item_ScrObj addItem, int addAmount)
    {
        for (int i = 0; i < itemSources.Count; i++)
        {
            addAmount -= itemSources[i].AddItem(addItem, addAmount);
            if (addAmount <= 0) break;
        }
        return addAmount;
    }
    /// <returns>
    /// Amount overflow leftover amount
    /// </returns>
    public int AddItem(Item_ScrObj addItem, int addAmount)
    {
        return AddItem(_itemsAddSources, addItem, addAmount);
    }
}
