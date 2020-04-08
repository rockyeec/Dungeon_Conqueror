using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableBow : Pickuppable, IEquippable
{
    [HideInInspector] public AnimatorEventManager wielder;
    public BowAnimatorScript bowArt;
    public GameObject fakeArrow;

    

    public void Equip(AnimatorEventManager wielder)
    {
        this.wielder = wielder;
        LimitLeftHand(true);
        bowArt.Equip(this.wielder);

        // i dunno keep myself in yer pockets
        transform.parent = wielder.transform;

        // put arrow in right hand
        fakeArrow.transform.parent = this.wielder.animator.GetBoneTransform(HumanBodyBones.RightHand);
        fakeArrow.transform.localPosition = Vector3.zero;
        fakeArrow.transform.localRotation = Quaternion.identity;
        fakeArrow.SetActive(false);

        // put bow in left hand
        bowArt.transform.parent.parent = this.wielder.animator.GetBoneTransform(HumanBodyBones.LeftHand);
        bowArt.transform.parent.localPosition = Vector3.zero;
        bowArt.transform.parent.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        transform.parent = null;
        fakeArrow.transform.parent = transform;
        fakeArrow.SetActive(false);

        bowArt.transform.parent.parent = transform;
        bowArt.Unequip();

        LimitLeftHand(false);
        wielder = null;
    }


    void LimitLeftHand(bool what)
    {
        Debug.Log("Waited all the frames bruh");
        wielder.animator.SetBool("limitLeftHand", what);
    }
}
