using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[System.Serializable]
public struct TileGenerate_ResoulationData
{
    [SerializeField] private Vector2 _maxGenerateSize;
    public Vector2 maxGenerateSize => _maxGenerateSize;

    [SerializeField] private Vector2 _resolution;
    public Vector2 resolution => _resolution;
}

[System.Serializable]
public class BackgroundRenderer_Data
{
    [Space(10)]
    [SerializeField][Range(0, 10)] private float _animationSpeed;
    public float animationSpeed => _animationSpeed;

    [SerializeField] private Color _colorA;
    public Color colorA => _colorA;

    [SerializeField] private Color _colorB;
    public Color colorB => _colorB;

    [SerializeField] private Color _colorC;
    public Color colorC => _colorC;
}

public class EnvironmentVisuals_Manager : MonoBehaviour
{
    [Space(20)]
    [SerializeField] PixelPerfectCamera _pixelCamera;
    [SerializeField] TileGenerate_ResoulationData[] resolutionDatas;

    [Space(20)]
    [SerializeField] private WarpRenderer_Controller _backgroundRenderer;
    public WarpRenderer_Controller backgroundRenderer => _backgroundRenderer;

    [Space(20)]
    [SerializeField] private Transform _blackTileMapBorder;
    [SerializeField][Range(0, 10)] private float _blackTileMapBorderSize;

    [Space(10)]
    [SerializeField] private Transform _whiteTileMapBorder;
    [SerializeField][Range(0, 10)] private float _whiteTileMapBorderSize;

    [Space(20)]
    [SerializeField] private Transform _tileMapShadow;
    [SerializeField] private Vector3 _tileMapShadowOffset;


    private MaterialPropertyBlock _backgroundMaterialblock;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_Manager.Register(EventBus.StartLoad, Update_Resolution);

        EventBus_Manager.Register(EventBus.StartLoad, Load_TileMapBorders);
        EventBus_Manager.Register(EventBus.StartLoad, Load_TileMapShadow);
    }
    
    private void OnDestroy()
    {
        EventBus_Manager.UnRegister(EventBus.StartLoad, Update_Resolution);

        EventBus_Manager.UnRegister(EventBus.StartLoad, Load_TileMapBorders);
        EventBus_Manager.UnRegister(EventBus.StartLoad, Load_TileMapShadow);
    }
    

    // Camera
    private void Update_Resolution()
    {
        Vector2 generateSize = InGame_Manager.instance.worldMapGenerator.Converted_GenerateSize();

        for (int i = 0; i < resolutionDatas.Length; i++)
        {
            TileGenerate_ResoulationData resData = resolutionDatas[i];
            Vector2 maxSize = resData.maxGenerateSize;
            
            if (generateSize.x > maxSize.x || generateSize.y > maxSize.y) continue;

            _pixelCamera.refResolutionX = (int)resData.resolution.x;
            _pixelCamera.refResolutionY = (int)resData.resolution.y;

            return;
        }
    }


    // Load
    private void Load_TileMapBorders()
    {
        Vector2 generateSize = InGame_Manager.instance.worldMapGenerator.Converted_GenerateSize();

        Vector3 blackBorderScale = _blackTileMapBorder.localScale;
        Vector3 whiteBorderScale = _whiteTileMapBorder.localScale;

        blackBorderScale.x = generateSize.x + _blackTileMapBorderSize;
        blackBorderScale.y = generateSize.y + _blackTileMapBorderSize;

        whiteBorderScale.x = generateSize.x + _whiteTileMapBorderSize;
        whiteBorderScale.y = generateSize.y + _whiteTileMapBorderSize;

        _blackTileMapBorder.localScale = blackBorderScale;
        _whiteTileMapBorder.localScale = whiteBorderScale;
    }

    private void Load_TileMapShadow()
    {
        Vector2 generateSize = InGame_Manager.instance.worldMapGenerator.Converted_GenerateSize();
        Vector3 shadowScale = _tileMapShadow.localScale;

        shadowScale.x = generateSize.x;
        shadowScale.y = generateSize.y;

        _tileMapShadow.localScale = shadowScale;
        _tileMapShadow.position += _tileMapShadowOffset;
    }
}
