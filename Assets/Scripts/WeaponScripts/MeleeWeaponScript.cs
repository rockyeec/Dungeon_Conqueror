using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponScript : MonoBehaviour, IEquippable
{
    public WeaponManager weapon;
    AnimatorEventManager wielder;
    private void Awake()
    {
        weapon = GetComponentInChildren<WeaponManager>();
        weapon.gameObject.SetActive(false);
    }

    public void Equip(AnimatorEventManager wielder)
    {
        this.wielder = wielder;
        weapon.wielder = wielder;

        transform.parent = wielder.animator.GetBoneTransform(HumanBodyBones.RightHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        transform.parent = null;

        weapon.wielder = null;
        wielder = null;
    }

   
}
