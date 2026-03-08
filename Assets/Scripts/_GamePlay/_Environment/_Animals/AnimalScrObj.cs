using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Animal")]
public class AnimalScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private GameObject _prefab;
    public GameObject prefab => _prefab;

    [Space(10)]
    [SerializeField] private TileScrObj[] _spawnTiles;
    public TileScrObj[] spawnTiles => _spawnTiles;

    [Space(20)]
    [SerializeField][Range(0, 100)] private int _maxHealth;
    public int maxHealth => _maxHealth;

    [SerializeField][Range(0, 10)] private int _maxMovementDistance;
    public int maxMovementDistance => _maxMovementDistance;
}
