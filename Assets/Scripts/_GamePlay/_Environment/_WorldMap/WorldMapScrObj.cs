using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New World Map")]
public class WorldMapScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private Vector2 _generateSize;
    public Vector2 generateSize => _generateSize;

    [SerializeField][Range(0, 100)] private float _harshGroundDensity;
    public float harshGroundDensity => _harshGroundDensity;

    [Space(10)]
    [SerializeField] private Tile_PresetDatas[] _presetTileDatas;
    public Tile_PresetDatas[] presetTileDatas => _presetTileDatas;

    [Space(20)]
    [SerializeField] private GameObject _worldMapEventsPrefab;
    public GameObject worldMapEventsPrefab => _worldMapEventsPrefab;
}