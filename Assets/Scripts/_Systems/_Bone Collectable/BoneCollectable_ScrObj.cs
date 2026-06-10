using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item/ New Bone Collectable")]
public class BoneCollectable_ScrObj : Item_ScrObj
{
    [Space(40)]
    [SerializeField][Multiline] private string _collectedDescription;
    public string collectedDescription => _collectedDescription;
}
