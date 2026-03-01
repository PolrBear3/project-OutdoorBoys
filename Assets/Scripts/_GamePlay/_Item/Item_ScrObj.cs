using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { place, use }

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item")]
public class Item_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private Sprite _inventorySprite;
    public Sprite inventorySprite => _inventorySprite;
    
    [SerializeField] private Sprite _microSprite;
    public Sprite microSprite => _microSprite;

    [Space(20)]
    [SerializeField] private string _itemName;
    public string itemName => _itemName;

    [SerializeField][Multiline] private string _description;
    public string description => _description;

    [Space(20)]
    [SerializeField] private ItemType _itemType;
    public ItemType itemType => _itemType;

    [Space(20)]
    [SerializeField] private GameObject _itemPrefab;
    public GameObject itemPrefab => _itemPrefab;

    [SerializeField] private Vector2 _offsetPosition;
    public Vector2 offsetPosition => _offsetPosition;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _maxAmount;
    public int maxAmount => _maxAmount;

    [SerializeField][Range(0, 100)] private int _itemWeight;
    public int itemWeight => _itemWeight;

    [Space(20)]
    [SerializeField] private bool _stackable;
    public bool stackable => _stackable;

    [SerializeField][Range(0, 10)]  private int _triggerRange;
    public int triggerRange => _triggerRange;
}
