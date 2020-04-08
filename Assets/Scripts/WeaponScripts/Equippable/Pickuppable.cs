using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickuppable : MonoBehaviour
{
    protected BoxCollider boxCollider;
    [HideInInspector] public Rigidbody rigidBody;

    protected virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        rigidBody = GetComponent<Rigidbody>();
    }

    public virtual void PickUp(StatesManager states)
    {

    }
    
}
