using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSript : WeaponManager
{
    bool isAlreadyAttacked = false;

    [HideInInspector]
    public Rigidbody rigidb;
    [HideInInspector]
    public BoxCollider boxCollider;

    private void Awake()
    {
        rigidb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        isAlreadyAttacked = false;
        StartCoroutine(DisableSelf());
    }

    IEnumerator DisableSelf()
    {
        yield return new WaitForSeconds(1.45f);

        StartCoroutine(StopRigidbody());
    }

    IEnumerator StopRigidbody()
    {
        rigidb.isKinematic = true;
        // kiasu wait 3 frames
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        rigidb.isKinematic = false;
        ObjectPool.Instance.ReturnObject(name, gameObject);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!isAlreadyAttacked)
        {
            HurtPeople(collision.gameObject);
            isAlreadyAttacked = true;
            //Debug.Log("Hey, Im the problem");
        }
        else
        {
            ObjectPool.Instance.GetObject("Sparks(Clone)", transform.position, Quaternion.LookRotation(-transform.forward));
        }
    }
}
