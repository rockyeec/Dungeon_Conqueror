using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputParent : MonoBehaviour
{
    public float lockonYOffset = 0.8f;

    public StatesManager states;

    protected virtual void Awake()
    {
        states = GetComponent<StatesManager>();
        states.Init();
    }



}
