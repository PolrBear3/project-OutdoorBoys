using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Scheme!")]
public class ControlScheme_ScrObj : ScriptableObject
{
    [Space(20)]
    [SerializeField] private string _schemeName;
    public string schemeName => _schemeName;

    [Space(20)]
    [SerializeField] private TMP_SpriteAsset _emojiAsset;
    public TMP_SpriteAsset emojiAsset => _emojiAsset;
    
    [SerializeField] private ActionKey_Data[] _actionKeyDatas;
    public ActionKey_Data[] actionKeyDatas => _actionKeyDatas;
}
