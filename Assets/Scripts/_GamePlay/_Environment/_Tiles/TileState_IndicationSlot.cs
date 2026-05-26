using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TileState_IndicationSlot : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private Image _stateIcon;
    public Image stateIcon => _stateIcon;

    [SerializeField] private TextMeshProUGUI _timeCountText;
    public TextMeshProUGUI timeCountText => _timeCountText;
}
