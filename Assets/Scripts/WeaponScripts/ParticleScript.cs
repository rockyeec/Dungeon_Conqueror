using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    //AudioSource aS;
    public AudioClip aC;
    [Range (0, 1)]
    public float volumeScale;
    private void Awake()
    {
        //aS = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        //aS.Play();
        if (GameMasterScript.Instance.audioSource != null)
            GameMasterScript.Instance.audioSource.PlayOneShot(aC, volumeScale);
        StartCoroutine(DisableSelf());
    }

    IEnumerator DisableSelf()
    {
        yield return new WaitForSeconds(1);
        ObjectPool.Instance.ReturnObject(name, gameObject);
    }
}
