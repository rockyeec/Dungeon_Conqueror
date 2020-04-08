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
    

    void PutAt(Transform who, Transform where)
    {
        who.parent = where;
        who.localPosition = Vector3.zero;
        who.localRotation = Quaternion.identity;
    }

    public void Equip(AnimatorEventManager wielder)
    {
        // physics
        gameObject.SetActive(true);
        SetRigidbodyActivity(false);
        
        // who's boss
        this.wielder = wielder;
        wielder.states.rpg.currentWeapon = this;

        // animation modification
        LimitLeftHand(true);

        // i dunno keep myself in yer pockets
        PutAt(transform, wielder.leftHand);

        // put arrow in right hand
        PutAt(fakeArrow.transform, wielder.rightHand);
        fakeArrow.SetActive(false);

        // put bow in left hand
        PutAt(bowArt.transform.parent, wielder.leftHand);
        bowArt.Equip(this.wielder);
    }

    public void Unequip()
    {
        // physics
        gameObject.SetActive(true);
        SetRigidbodyActivity(true);

        // take out arrow
        PutAt(fakeArrow.transform, transform);
        fakeArrow.SetActive(false);

        // take out bow
        PutAt(bowArt.transform.parent, transform);
        bowArt.Unequip();

        // take out weapons from hands
        transform.parent = null;

        // modify animation
        LimitLeftHand(false);

        // de-boss yourself
        wielder.states.rpg.currentWeapon = null;
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
