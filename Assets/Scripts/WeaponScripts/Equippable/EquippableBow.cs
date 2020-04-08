using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippableBow : Pickuppable, IAttackable
{
    [HideInInspector] public AnimatorEventManager wielder;
    public BowAnimatorScript bowArt;
    public GameObject fakeArrow;

    public StatesManager.MoveSet moveSet;
    public StatesManager.MoveSet MoveSet
    {
        get { return moveSet; }
    }
    

    public void Equip(AnimatorEventManager wielder)
    {
        gameObject.SetActive(true);
        SetRigidbodyActivity(false);
        this.wielder = wielder;
        LimitLeftHand(true);
        bowArt.Equip(this.wielder);

        // i dunno keep myself in yer pockets
        transform.parent = wielder.transform;

        // put arrow in right hand
        fakeArrow.transform.parent = this.wielder.rightHand;
        fakeArrow.transform.localPosition = Vector3.zero;
        fakeArrow.transform.localRotation = Quaternion.identity;
        fakeArrow.SetActive(false);

        // put bow in left hand
        bowArt.transform.parent.parent = this.wielder.leftHand;
        bowArt.transform.parent.localPosition = Vector3.zero;
        bowArt.transform.parent.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        gameObject.SetActive(true);
        SetRigidbodyActivity(true);
        transform.parent = null;
        fakeArrow.transform.parent = transform;
        fakeArrow.SetActive(false);

        bowArt.transform.parent.parent = transform;
        bowArt.Unequip();

        LimitLeftHand(false);
        wielder = null;
    }

    public void Attack()
    {
        fakeArrow.SetActive(true);
    }

    public void EndAttack()
    {
        fakeArrow.SetActive(false);
    }

    public void LaunchArrow()
    {
        GameObject arrow = ObjectPool.Instance.GetObject("Arrow_Prefab(Clone)", fakeArrow.transform.position, wielder.head.rotation);
        AssignArrowVariables(arrow.GetComponent<ArrowManager>());
    }

    public void LaunchMultipleArrowsVertically()
    {
        LaunchMultipleArrows("vertically");
    }
    public void LaunchMultipleArrowsHorizontally()
    {
        LaunchMultipleArrows("horizontally");
    }

    public void LaunchMultipleArrows(string method)
    {
        int arrowCount = wielder.states.rpg.level + 2;
        float arcAngle = wielder.arcAngle;
        for (int i = 0; i < arrowCount; i++)
        {
            float yDir = -(arcAngle / 2) + i * (arcAngle / arrowCount);
            Vector3 offset = method == "horizontally" ? new Vector3(0, yDir) : new Vector3(yDir, 0);
            Quaternion rot = Quaternion.Euler(wielder.head.eulerAngles + offset);

            GameObject arrow = ObjectPool.Instance.GetObject("Arrow_Prefab(Clone)", fakeArrow.transform.position, rot);
            AssignArrowVariables(arrow.GetComponent<ArrowManager>());
        }

    }

    void AssignArrowVariables(ArrowManager aS)
    {
        Physics.IgnoreCollision(wielder.states.capsule, aS.boxCollider);
        aS.damage = wielder.states.rpg.GetDamage();
        aS.friendlyLayer = wielder.friendlyLayer;
        aS.rigidb.AddForce(aS.transform.forward * wielder.arrowLaunchForce);
        aS.ownerForward = transform.forward;
    }


    void LimitLeftHand(bool what)
    {
        Debug.Log("Waited all the frames bruh");
        wielder.animator.SetBool("limitLeftHand", what);
    }
}
