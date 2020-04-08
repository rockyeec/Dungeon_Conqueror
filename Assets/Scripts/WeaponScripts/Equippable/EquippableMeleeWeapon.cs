using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableMeleeWeapon : Pickuppable, IEquippable
{
    public MeleeWeaponManager weapon;
    AnimatorEventManager wielder;
    protected override void Awake()
    {
        base.Awake();
        weapon.gameObject.SetActive(false);
    }

    
    public void Equip(AnimatorEventManager wielder)
    {
        this.wielder = wielder;
        LimitRightHand(true);
        weapon.wielder = wielder;

        transform.parent = wielder.animator.GetBoneTransform(HumanBodyBones.RightHand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        transform.parent = null;

        weapon.wielder = null;
        LimitRightHand(false);
        wielder = null;
    }

   
   void LimitRightHand(bool what)
    {
        wielder.animator.SetBool("limitRightHand", what);
    }
}
