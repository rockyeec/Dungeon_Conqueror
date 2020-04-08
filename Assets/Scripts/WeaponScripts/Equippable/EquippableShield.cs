using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableShield : MonoBehaviour, IEquippable
{
    public ShieldScript shieldScript;
    [HideInInspector] public AnimatorEventManager wielder;

    public void Equip(AnimatorEventManager wielder)
    {
        this.wielder = wielder;

        transform.parent = wielder.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
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
