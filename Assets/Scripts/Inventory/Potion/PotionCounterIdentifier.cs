using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PotionCounterIdentifier : MonoBehaviour
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI counter;
    public Image image;


    public void DisablePotionHUD()
    {
        image.enabled = false;
        text.enabled = false;
        counter.enabled = false;
    }
    public void UpdateValues(Sprite icon, string description, int count)
    {
        image.enabled = true;
        image.sprite = icon;
        
        text.enabled = true;
        text.text = description;

        counter.enabled = true;
        counter.text = "x" + count;
    }
}
