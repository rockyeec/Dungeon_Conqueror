using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiddleBarScript : MonoBehaviour
{
    Image image;
    public float changeDuration = 0.33f;



    public void Init(float percentage)
    {
        image = GetComponent<Image>();
        image.fillAmount = percentage;
    }
    
    public void ChangePercentageTo(float percentage)
    {
        StartCoroutine(ChangeValueTo(percentage));       
    }

    public IEnumerator ChangeValueTo(float percentage)
    {
        yield return new WaitForSeconds(0.33f);

        float initial = image.fillAmount;
        float timer = 0;
        while (timer < changeDuration)
        {
            timer += Time.deltaTime;
            image.fillAmount = Mathf.Lerp(initial, percentage, timer / changeDuration);
            yield return null;
        }
        image.fillAmount = percentage;
    }
}
