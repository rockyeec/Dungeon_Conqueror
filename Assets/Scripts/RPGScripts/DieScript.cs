using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieScript : MonoBehaviour
{
    
    private void OnEnable()
    {
        AwardManager.Instance.GiveXP(33);
        GetComponent<Animator>().SetBool("dead", true);

        StartCoroutine(DisableSelf());
    }

    IEnumerator DisableSelf()
    {
        yield return new WaitForSeconds(15);
        ObjectPool.Instance.ReturnObject(transform.parent.name, transform.parent.gameObject);
    }
}
