using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class item_info_panel : MonoBehaviour
{
    private RectTransform rect;
    public Transform canvasTransform;
    public Vector2 safeZone;
    public bool flipX, flipY;

    private Vector2 normalVector;
    public Vector2 flipVector;

    float newPivotX, newPivotY;

    // Update is called once per frame
    private void Start()
    {
        rect = GetComponent<RectTransform>();
        normalVector = rect.pivot;
    }

    void Update()
    {
        transform.position = Input.mousePosition;

        //Debug.Log(canvasTransform.InverseTransformPoint(transform.position).x);

        if(canvasTransform.InverseTransformPoint(transform.position).x > safeZone.x)
        {

            newPivotX = flipVector.x;

        }
        else
        {

            newPivotX = normalVector.x;

        }

        if(canvasTransform.InverseTransformPoint(transform.position).y < safeZone.y)
        {

            newPivotY = flipVector.y;

        }
        else
        {

            newPivotY = normalVector.y;

        }

        rect.pivot = new Vector2(newPivotX, newPivotY);

    }

    public void ShowInfoPanel(string type = "item")
    {
        //Debug.Log("Show Panel");

        if (type == "item")
        {

            transform.GetChild(0).gameObject.SetActive(true);

        }else if(type == "skill")
        {

            transform.GetChild(1).gameObject.SetActive(true);

        }
        else if(type == "status")
        {

            transform.GetChild(2).gameObject.SetActive(true);

        }
       

    }

    public void HideInfoPanel()
    {

        for(int i = 0; i < transform.childCount; i++)
        {

            transform.GetChild(i).gameObject.SetActive(false);

        }

    }
}
