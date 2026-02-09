using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data_Manager : MonoBehaviour
{
    public static Data_Manager instance;

    [Space(20)]
    [SerializeField] private TileScrObj[] _tileScrObjs;
    public TileScrObj[] tileScrObj => _tileScrObjs;


    // MonoBehaviour
    private void Awake()
    {
        instance = this;
    }
}