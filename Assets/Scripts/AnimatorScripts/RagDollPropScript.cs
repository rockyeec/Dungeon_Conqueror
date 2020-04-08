using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollPropScript : MonoBehaviour
{
    Transform parent;
    Rigidbody rigidBody;
    BoxCollider boxCollider;

    public void Init()
    {
        parent = transform.parent;
        rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public void ActivateProp()
    {
        transform.parent = null;
        SetRigidbodyActivity(true);
        rigidBody.AddForce(transform.up * 50, ForceMode.Impulse);
    }

    public void DeactivateProp()
    {
        SetRigidbodyActivity(false);
        transform.parent = parent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    void SetRigidbodyActivity(bool to)
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
