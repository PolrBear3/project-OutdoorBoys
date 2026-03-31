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
    private Coroutine _loadCoroutine;


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

    public void Load_Renderer(WarpRenderer_Data loadData, float loadDuration)
    {
        if (loadData == null) return;

        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
        _loadCoroutine = StartCoroutine(Renderer_LoadUpdate(loadData, loadDuration));
    }
    private IEnumerator Renderer_LoadUpdate(WarpRenderer_Data loadData, float loadDuration)
    {
        float startPixel = _materialBlock.GetFloat("_PixelSize");
        float startSpeed = _materialBlock.GetFloat("_Speed");

        Color startA = _materialBlock.GetColor("_ColorA");
        Color startB = _materialBlock.GetColor("_ColorB");
        Color startC = _materialBlock.GetColor("_ColorC");

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / loadDuration;

            float pixelSize = Mathf.Lerp(startPixel, loadData.pixelSize, time);
            float animSpeed = Mathf.Lerp(startSpeed, loadData.animationSpeed, time);

            Color colorA = Color.Lerp(startA, loadData.colorA, time);
            Color colorB = Color.Lerp(startB, loadData.colorB, time);
            Color colorC = Color.Lerp(startC, loadData.colorC, time);

            _renderer.GetPropertyBlock(_materialBlock);

            _materialBlock.SetFloat("_PixelSize", pixelSize);
            _materialBlock.SetFloat("_Speed", animSpeed);

            _materialBlock.SetColor("_ColorA", colorA);
            _materialBlock.SetColor("_ColorB", colorB);
            _materialBlock.SetColor("_ColorC", colorC);

            _renderer.SetPropertyBlock(_materialBlock);

            yield return null;
        }

        _loadCoroutine = null;
        yield break;
    }

    private void Load_Renderer()
    {
        _materialBlock = new MaterialPropertyBlock();

        Load_Renderer(_defaultData);
    }
}
