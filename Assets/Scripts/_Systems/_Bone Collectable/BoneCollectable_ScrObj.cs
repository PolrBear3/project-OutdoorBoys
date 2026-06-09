using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New ScriptableObject/ New Item/ New Bone Collectable")]
public class BoneCollectable_ScrObj : Item_ScrObj
{
    [Space(40)]
    [SerializeField] private Sprite _customSlotSprite;
    public Sprite customSlotSprite => _customSlotSprite;
}
