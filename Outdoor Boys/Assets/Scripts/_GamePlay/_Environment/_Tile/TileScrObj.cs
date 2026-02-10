using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grouped_TileSprites
{
    [SerializeField] private Sprite[] _sprites;
    public Sprite[] sprites => _sprites;
}

[CreateAssetMenu(menuName = "New ScriptableObject/ New Tile")]
public class TileScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private TileType _type;
    public TileType type => _type;

    [SerializeField] private GameObject _prefab;
    public GameObject prefab => _prefab;

    [Space(20)]
    [SerializeField] private Grouped_TileSprites[] _groupedSprites;
    public Grouped_TileSprites[] groupedSprites => _groupedSprites;


    public Sprite[] GroupedSprites()
    {
        if (_groupedSprites.Length <= 0) return null;
        
        int randIndex = Random.Range(0, _groupedSprites.Length);
        return _groupedSprites[randIndex].sprites;
    }
}
