using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickuppable : MonoBehaviour
{
    protected BoxCollider boxCollider;
    [HideInInspector] public Rigidbody rigidBody;
    StatesManager owner;

    protected virtual void Awake()
    {
        gameObject.layer = 13;
        rigidBody = gameObject.AddComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public virtual void PickUp(StatesManager states)
    {
        Debug.Log("pickUpLaDey");
        owner = states;
        SetRigidbodyActivity(false);
        states.rpg.generalStuff.Add(this);
        gameObject.SetActive(false);
    }

    public virtual void Drop()
    {
        gameObject.SetActive(true);
        transform.position = owner.aem.body.position;
        SetRigidbodyActivity(true);

        if (owner.rpg.generalStuff.Contains(this))
            owner.rpg.generalStuff.Remove(this);

        owner = null;        
    }

    protected void SetRigidbodyActivity(bool to)
    {
        boxCollider.enabled = to;
        rigidBody.useGravity = to;

        if (to)
        {
            rigidBody.drag = 0;
            rigidBody.constraints = RigidbodyConstraints.None;
        }
        else
        {
            rigidBody.drag = 1000;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
