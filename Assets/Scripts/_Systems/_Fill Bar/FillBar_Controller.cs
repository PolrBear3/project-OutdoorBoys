using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillBar_Controller : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private GameObject _fillBarPrefab;
    [SerializeField] private Vector2 _setOffSet;

    [Space(10)]
    [SerializeField] private Color _fillColor;


    private FillBar _currentFllBar;
    

    // MonoBehaviour
    private void OnDestroy()
    {
        Refresh_CurrentFillBar();
    }
    

    // Main
    public void Refresh_CurrentFillBar()
    {
        if (_currentFllBar == null) return;
        
        Destroy(_currentFllBar.gameObject);
        _currentFllBar = null;
    }

    public void Set_FillBar(Transform setTransform)
    {
        Refresh_CurrentFillBar();

        GameObject setFillBar = Instantiate(_fillBarPrefab, setTransform);
        if (setFillBar.TryGetComponent(out FillBar trackFillBar) == false) return;

        _currentFllBar = trackFillBar;

        setFillBar.transform.localPosition = _setOffSet;
        trackFillBar.fillRenderer.color = _fillColor;
    }

    public void Update_CurrentBarFill(int maxValue, int currentValue)
    {
        if (_currentFllBar == null) return;

        _currentFllBar.Update_Fill(maxValue, currentValue);
    }

    public void Toggle(bool toggle)
    {
        if (_currentFllBar == null) return;
        
        _currentFllBar.gameObject.SetActive(toggle);
    }
}