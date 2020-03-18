using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSript : WeaponManager
{
    bool isAlreadyAttacked = false;

    [HideInInspector]
    public Rigidbody rigidb;

    protected override void Awake()
    {
        base.Awake();
        rigidb = GetComponent<Rigidbody>();
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

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        ObjectPool.Instance.ReturnObject(name, gameObject);
    }

    IEnumerator StopRigidbody()
    {
        rigidb.isKinematic = true;
        yield return new WaitForFixedUpdate();
        rigidb.isKinematic = false;
    }

    protected void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer != 8)
        {
            if (!isAlreadyAttacked)
            {
                isAlreadyAttacked = true;
                HurtPeople(collision.gameObject);
            }
            else
            {
                ObjectPool.Instance.GetObject("Sparks(Clone)", transform.position, Quaternion.LookRotation(-transform.forward));
            }
        }
    }
}
