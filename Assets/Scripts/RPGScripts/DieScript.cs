using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieScript : MonoBehaviour
{

    [HideInInspector] public StatesManager states;
    Animator animator;

    public void Init()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (animator == null)
            return;

        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;

        animator.enabled = true;
        animator.SetBool("dead", true);      

        StartCoroutine(DisableSelf());
    }

    IEnumerator DisableSelf()
    {
        yield return new WaitForSeconds(0.95f);

        animator.enabled = false;

        yield return new WaitForSeconds(25);

        ObjectPool.Instance.ReturnObject(states.name, states.gameObject);
    }
}
