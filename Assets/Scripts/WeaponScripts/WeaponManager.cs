using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [HideInInspector] public float damage;
    public int friendlLayer = 10;
    public bool isHit = false;
    string particle;
    public AnimatorEventManager aem;

    protected virtual void Awake()
    {
        
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isHit)
            HurtPeople(other.gameObject);   
    }

    


    public void HurtPeople(GameObject collider)
    {
        if (collider.layer != 8 && collider.layer != friendlLayer)
        {
            ShieldScript shieldScript = collider.GetComponent<ShieldScript>();
            if (shieldScript != null)
            {
                isHit = true;
                if (shieldScript.parryWindow)
                {
                    particle = "Blue Sparks(Clone)";
                    damage = 0;
                    if (aem != null)
                        aem.animator.CrossFade("Stagger", 0.15f);
                }
                else
                {
                    damage *= 0.2f;
                }
            }

            StatesManager states = collider.GetComponent<StatesManager>();
            if (states != null && damage != 0)
            {
                if (!states.isDodge)
                {
                    states.Hurt(damage);
                    particle = "Blood(Clone)";
                }
            }
            else if (shieldScript == null)
            {
                particle = "Sparks(Clone)";
            }

            if (particle != null)
                ObjectPool.Instance.GetObject(particle, transform.position, Quaternion.LookRotation(-transform.forward));
        }
        particle = null;
    }
    
}
