using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    float delta;

    public float maxDistance = 1.8f;
    public float shoulderDistance = 0.85f;
    public float smoothTime = 0.15f;
    public float mouseSensitivity = 2;
    public Vector2 pitchMinMax = new Vector2(-40, 40);
    public float followSpeed = 8;
    [HideInInspector] public float currentFollowSpeed;

    public float lockonPitchAdjustment = 0.7f;

    Transform cameraPivot;
    Transform target;
    Camera mainCam;

    float pitch;
    float yaw;
    float smoothX, smoothY, smoothXVel, smoothYVel;


    public InputParent enemyTarget;

    [HideInInspector] public bool lockon;

    float shoulderOffset;
    Vector3 shoulderPos;

    Vector3[] points = new Vector3[5];
    float distance;

    public void Init(Transform target)
    {
        mainCam = Camera.main;
        this.target = target;
        cameraPivot = Camera.main.transform.parent;
        shoulderOffset = shoulderDistance;
        UpdateShoulderPos();
    }

    public void Tick(float delta)
    {
        this.delta = delta;
        HandleShoulderCamera();
        FollowTarget();
        HandleRotation();
        ControlDistance();
    }
    void FollowTarget()
    {
        Vector3 p =
            Vector3.Lerp
            (
                transform.position,
                target.position,
                delta * currentFollowSpeed
            );
        transform.position = p;
    }

    void HandleRotation()
    {

        if (enemyTarget != null)
        {
            Vector3 yaw = enemyTarget.states.aem.body.position - mainCam.transform.position;// transform.position;
            yaw.y = 0;
            transform.rotation = Quaternion.RotateTowards
                (
                    transform.rotation, 
                    Quaternion.LookRotation(yaw.normalized), 
                    delta * 450
                );
            this.yaw = transform.eulerAngles.y;


            Vector3 pitch = (enemyTarget.states.aem.body.position + Vector3.up * lockonPitchAdjustment) - (transform.position + Vector3.up);
            pitch.z = Mathf.Sqrt(Mathf.Pow(pitch.z, 2) +Mathf.Pow(pitch.x, 2));
            pitch.x = 0;
            cameraPivot.localRotation = Quaternion.RotateTowards
                (
                    cameraPivot.localRotation,
                    Quaternion.LookRotation(pitch.normalized),
                    delta * 450
                );
            //pitch = cameraPivot.localEulerAngles.x;
        }
        else
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");

            smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXVel, delta * smoothTime);
            smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYVel, delta * smoothTime);

            pitch -= smoothY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

            yaw += smoothX * mouseSensitivity;

            cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
            transform.rotation = Quaternion.Euler(0, yaw, 0);
        }        
    }

    void UpdateShoulderPos()
    {
        shoulderPos = cameraPivot.localPosition;
        shoulderPos.x = shoulderOffset;
    }



    float buttonTimer = 0;
    void HandleShoulderCamera()
    {
        if (buttonTimer > 0)
        {
            buttonTimer -= delta;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && buttonTimer <= 0)
        {
            buttonTimer = 0.15f;
            shoulderOffset = -shoulderOffset;
            UpdateShoulderPos();
        }

        if (cameraPivot.localPosition != shoulderPos)
        {
            cameraPivot.localPosition = Vector3.Lerp
                (
                    cameraPivot.localPosition, 
                    shoulderPos, 
                    delta * followSpeed
                );
        }
    }




    

    void ControlDistance()
    {
        float z = mainCam.nearClipPlane;
        float y = z * Mathf.Tan(mainCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float x = y * mainCam.aspect;

        /*points[0] = mainCam.transform.position + cameraPivot.rotation * new Vector3(-x, y, z);
        points[1] = mainCam.transform.position + cameraPivot.rotation * new Vector3(x, y, z);
        points[2] = mainCam.transform.position + cameraPivot.rotation * new Vector3(-x, -y, z);
        points[3] = mainCam.transform.position + cameraPivot.rotation * new Vector3(x, -y, z);*/
        points[4] = mainCam.transform.position - mainCam.transform.forward;


        float multiplier = 1.3f;
        points[0] = mainCam.transform.position + cameraPivot.rotation * new Vector3(-x, y, 0) * multiplier;
        points[1] = mainCam.transform.position + cameraPivot.rotation * new Vector3(x, y, 0) * multiplier;
        points[2] = mainCam.transform.position + cameraPivot.rotation * new Vector3(-x, -y, 0) * multiplier;
        points[3] = mainCam.transform.position + cameraPivot.rotation * new Vector3(x, -y, 0) * multiplier;


        distance = maxDistance;
        for (int i = 0; i < 5; i++)
        {
            Debug.DrawLine(cameraPivot.position, points[i]);

            RaycastHit hitInfo;
            if (Physics.Linecast(cameraPivot.position, points[i], out hitInfo,
                1 << 0 | 1 << 1 | 1 << 2 | 1 << 4 | 1 << 5 | 1 << 11))
            {
                if (hitInfo.distance < distance)
                {
                    distance = hitInfo.distance;
                }
            }
        }

        mainCam.transform.localPosition = Vector3.Lerp
            (
                mainCam.transform.localPosition,
                new Vector3(0, 0, -distance),
                delta * followSpeed
            );
    }



}
