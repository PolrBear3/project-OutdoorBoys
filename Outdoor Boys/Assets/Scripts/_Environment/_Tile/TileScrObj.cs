using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Tile")]
public class TileScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private TileType _type;
    public TileType type => _type;

    [SerializeField] private GameObject _prefab;
    public GameObject prefab => _prefab;

    [Space(20)]
    [SerializeField] private Sprite[] _sprites;
    public Sprite[] sprites => _sprites;
}
