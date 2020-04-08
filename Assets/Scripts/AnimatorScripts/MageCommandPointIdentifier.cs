using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageCommandPointIdentifier : MonoBehaviour
{
    void Update()
    {
        Vector3 spin = transform.eulerAngles;
        spin.y = (spin.y + Time.deltaTime * 150) % 360;
        transform.eulerAngles = spin;
    }
}
