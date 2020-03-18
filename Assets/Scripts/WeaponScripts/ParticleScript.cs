using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<AudioSource>().Play();
        StartCoroutine(DisableSelf());
    }

    IEnumerator DisableSelf()
    {
        yield return new WaitForSeconds(1);
        ObjectPool.Instance.ReturnObject(name, gameObject);
    }
}
