using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableShield : Pickuppable, IEquippable
{
    public ShieldScript shieldScript;
    [HideInInspector] public AnimatorEventManager wielder;


    public void Equip(AnimatorEventManager wielder)
    {
        SetRigidbodyActivity(false);
        this.wielder = wielder;

        transform.parent = wielder.leftArm;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        SetRigidbodyActivity(true);
        transform.parent = null;

        wielder = null;
    }

    private void Update()
    {
        if (wielder == null)
            return;

        if (shieldScript == null)
            return;

        shieldScript.gameObject.SetActive(wielder.animator.GetBool("block"));
    }
}
