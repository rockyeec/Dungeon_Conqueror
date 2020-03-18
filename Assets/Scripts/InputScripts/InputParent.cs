using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputParent : MonoBehaviour
{
    public float lockonYOffset = 0.8f;
    [HideInInspector] public Vector3 lockonPosition;

    public StatesManager states;

    public void UpdateLockonPosition()
    {
        lockonPosition = transform.position + new Vector3(0, lockonYOffset);
    }

    private void Awake()
    {
        states = GetComponent<StatesManager>();
        states.Init();
    }



}
