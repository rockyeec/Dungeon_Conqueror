using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarSript : MonoBehaviour
{
    Transform mainCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(mainCam);
        transform.forward = -transform.forward;
    }
}
