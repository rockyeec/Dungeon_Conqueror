using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieScript : MonoBehaviour
{
    public List<RagDollPropScript> ragDollProps = new List<RagDollPropScript>();

    [HideInInspector] public StatesManager states;
    Animator animator;

    public void Init()
    {
        animator = GetComponent<Animator>();
        for (int i = 0; i < ragDollProps.Count; i++)
        {
            ragDollProps[i].Init();
        }
    }

    private void OnEnable()
    {

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
        for (int i = 0; i < ragDollProps.Count; i++)
        {
            ragDollProps[i].ActivateProp();
        }

        yield return new WaitForSeconds(15);

        for (int i = 0; i < ragDollProps.Count; i++)
        {
            ragDollProps[i].DeactivateProp();
        }

        ObjectPool.Instance.ReturnObject(states.name, transform.parent.gameObject);
    }
}
