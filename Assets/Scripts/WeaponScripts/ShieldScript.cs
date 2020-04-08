using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldScript : MonoBehaviour
{
    [HideInInspector] public bool parryWindow;
    private void OnEnable()
    {
        StopCoroutine(Parry());
        parryWindow = true;
        StartCoroutine(Parry());
    }

    private IEnumerator Parry()
    {
        yield return new WaitForSeconds(0.4f);
        parryWindow = false;
    }
}
