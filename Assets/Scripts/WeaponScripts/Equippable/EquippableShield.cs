using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableShield : Pickuppable, IEquippable
{
    public ShieldScript shieldScript;
    [HideInInspector] public AnimatorEventManager wielder;


    public void Equip(AnimatorEventManager wielder)
    {
        // physics
        gameObject.SetActive(true);
        SetRigidbodyActivity(false);

        // who's boss
        this.wielder = wielder;
        wielder.states.rpg.currentShield = this;

        // put on arm
        transform.parent = wielder.leftArm;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        // physics
        gameObject.SetActive(true);
        SetRigidbodyActivity(true);

        // take out from arm
        transform.parent = null;

        // de-boss
        wielder.states.rpg.currentShield = null;
        wielder = null;
    }

    private void Update()
    {
        if (wielder == null)
            return;

        if (shieldScript == null)
            return;

        shieldScript.gameObject.SetActive(wielder.animator.GetBool("block"));
        //shieldScript.enabled = wielder.animator.GetBool("block");
        //Debug.Log(shieldScript.enabled);
    }
}
