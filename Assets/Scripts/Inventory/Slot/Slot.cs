using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public item_info_panel info_panel;
    protected string type;

    // Start is called before the first frame update
    public virtual void Start()
    {
        info_panel = GameObject.Find("ItemInfo").GetComponent<item_info_panel>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Pointer Enter");
        info_panel.ShowInfoPanel(type);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Pointer Exit");
        info_panel.HideInfoPanel();
    }
}
