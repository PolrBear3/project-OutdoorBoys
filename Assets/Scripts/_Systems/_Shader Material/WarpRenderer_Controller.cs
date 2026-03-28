using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WarpRenderer_Data
{
    [Space(10)]
    [SerializeField][Range(0, 50)] private float _pixelSize;
    public float pixelSize => _pixelSize;

    [SerializeField][Range(0, 10)] private float _animationSpeed;
    public float animationSpeed => _animationSpeed;

    [Space(10)]
    [SerializeField] private Color _colorA;
    public Color colorA => _colorA;

    [SerializeField] private Color _colorB;
    public Color colorB => _colorB;

    [SerializeField] private Color _colorC;
    public Color colorC => _colorC;
}

public class WarpRenderer_Controller : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;

    [Space(20)]
    [SerializeField] private WarpRenderer_Data _defaultData;

    private MaterialPropertyBlock _materialBlock;


    // MonoBehaviour
    private void Awake()
    {
        Load_Renderer();
    }


    // Load
    private void Load_Renderer(WarpRenderer_Data loadData)
    {
        if (loadData == null) return;

        _renderer.GetPropertyBlock(_materialBlock);

        _materialBlock.SetFloat("_PixelSize", loadData.pixelSize);
        _materialBlock.SetFloat("_Speed", loadData.animationSpeed);

        _materialBlock.SetColor("_ColorA", loadData.colorA);
        _materialBlock.SetColor("_ColorB", loadData.colorB);
        _materialBlock.SetColor("_ColorC", loadData.colorC);

        _renderer.SetPropertyBlock(_materialBlock);
    }

    private void Load_Renderer()
    {
        _materialBlock = new MaterialPropertyBlock();

        Load_Renderer(_defaultData);
    }
}
