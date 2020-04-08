using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableMeleeWeapon : Pickuppable, IAttackable
{
    public MeleeWeaponManager weapon;
    AnimatorEventManager wielder;

    public StatesManager.MoveSet moveSet;
    public StatesManager.MoveSet MoveSet
    {
        get { return moveSet; }
    }

    protected override void Awake()
    {
        base.Awake();
        weapon.gameObject.SetActive(false);
    }

    
    public void Equip(AnimatorEventManager wielder)
    {
        gameObject.SetActive(true);
        SetRigidbodyActivity(false);

        // who's boss
        this.wielder = wielder;
        wielder.states.rpg.currentWeapon = this;

        // animation modification
        LimitRightHand(true);
        weapon.wielder = wielder;

        // put weapon in right hand
        transform.parent = wielder.rightHand;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        // physics
        gameObject.SetActive(true);
        SetRigidbodyActivity(true);

        // take out from hand
        transform.parent = null;
        weapon.wielder = null;

        // modify animations
        LimitRightHand(false);

        // de-boss.. Merdeka! You answer to NO ONE now!!
        wielder.states.rpg.currentWeapon = null;
        wielder = null;
    }

    public void Attack()
    {
        weapon.gameObject.SetActive(true);
        weapon.isHit = false;
        weapon.friendlyLayer = wielder.friendlyLayer;
        weapon.ownerForward = transform.forward;
        weapon.damage = wielder.states.rpg.GetDamage() * wielder.states.damageMultiplier;
    }

    public void EndAttack()
    {
        weapon.gameObject.SetActive(false);
    }

    public void LaunchArrow()
    {
       
    }

    public void LaunchMultipleArrowsVertically()
    {
        
    }
    public void LaunchMultipleArrowsHorizontally()
    {
       
    }


    void LimitRightHand(bool what)
    {
        wielder.animator.SetBool("limitRightHand", what);
    }
}
