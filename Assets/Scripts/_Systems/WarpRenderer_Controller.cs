using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WarpRenderer_Data
{
    [SerializeField][Range(0, 10)] private float _loadDuration;
    public float loadDuration => _loadDuration;

    [SerializeField][Range(0, 50)] private float _pixelSize;
    public float pixelSize => _pixelSize;

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


    private WarpRenderer_Data _currentData;
    private const float _loadAnimationSpeed = 0.4f;

    private MaterialPropertyBlock _materialBlock;
    private Coroutine _loadCoroutine;


    // MonoBehaviour
    private void Awake()
    {
        Load_Renderer();

        _materialBlock.SetFloat("_Speed", _loadAnimationSpeed);
        _renderer.SetPropertyBlock(_materialBlock);
    }


    // Load
    public void Load_Renderer(WarpRenderer_Data loadData)
    {
        if (loadData == null || loadData == _currentData) return;

        if (loadData.loadDuration <= 0)
        {
            _currentData = loadData;

            _materialBlock.SetFloat("_PixelSize", loadData.pixelSize);
            _materialBlock.SetColor("_ColorA", loadData.colorA);
            _materialBlock.SetColor("_ColorB", loadData.colorB);
            _materialBlock.SetColor("_ColorC", loadData.colorC);

            _renderer.SetPropertyBlock(_materialBlock);

            return;
        }

        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
        _loadCoroutine = StartCoroutine(Renderer_LoadUpdate(loadData));
    }
    private IEnumerator Renderer_LoadUpdate(WarpRenderer_Data loadData)
    {
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime / loadData.loadDuration;

            float pixelSize = Mathf.Lerp(_currentData.pixelSize, loadData.pixelSize, time);
            Color colorA = Color.Lerp(_currentData.colorA, loadData.colorA, time);
            Color colorB = Color.Lerp(_currentData.colorB, loadData.colorB, time);
            Color colorC = Color.Lerp(_currentData.colorC, loadData.colorC, time);

            _materialBlock.SetFloat("_PixelSize", pixelSize);
            _materialBlock.SetColor("_ColorA", colorA);
            _materialBlock.SetColor("_ColorB", colorB);
            _materialBlock.SetColor("_ColorC", colorC);

            _renderer.SetPropertyBlock(_materialBlock);

            yield return null;
        }
        
        _currentData = loadData;
        _loadCoroutine = null;
    }

    private void Load_Renderer()
    {
        _materialBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_materialBlock);

        Load_Renderer(_defaultData);
    }
}