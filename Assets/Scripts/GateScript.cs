using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateScript : MonoBehaviour
{
    public Rigidbody gate;

    private void Start()
    {
        gate = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            gate.useGravity = true;
            GetComponent<BoxCollider>().enabled = false;
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(OpenGate());
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            gate.useGravity = true;
        }
    }

    IEnumerator OpenGate()
    {
        gate.useGravity = false;
        while (gate.position.y < 8.72)
        {
            gate.transform.Translate(Vector3.up * 0.1f);
            yield return null;
        }
    }
}
