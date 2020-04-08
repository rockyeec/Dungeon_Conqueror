using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public int friendlyLayer = 10;
    [HideInInspector] public bool isHit = false;
    [HideInInspector] public Vector3 ownerForward;
    string particle;
    [HideInInspector] public AnimatorEventManager wielder;



    public void HurtPeople(GameObject collider)
    {
        if (collider.layer != 8 && collider.layer != friendlyLayer)
        {
            InputParent ip = collider.GetComponent<InputParent>();
            if (ip != null)
            {
                ModifyHostileStats(ip);
            }
            else
            {
                particle = "Sparks(Clone)";
            }

            if (particle != null)
            {
                ObjectPool.Instance.GetObject(particle, transform.position, Quaternion.LookRotation(-transform.forward));
                particle = null;
            }
        }
    }
    

    void ModifyHostileStats(InputParent ip)
    {
        float totalDamage = damage - ip.states.rpg.GetDefence();
        if (totalDamage < 0)
            totalDamage = 0;

        bool hasHitShield = false;

        EquippableShield equippableShield = ip.states.rpg.currentShield;
        if (equippableShield != null)
        {
            if (equippableShield.shieldScript.gameObject.activeSelf)
            {
                float angle = Vector3.SignedAngle(ip.transform.forward, ownerForward, Vector3.up);
                //Debug.Log(angle);
                if (angle > 90 || angle < -90)
                {
                    //isHit = true;
                    hasHitShield = true;
                    particle = "Blue Sparks(Clone)";
                    if (equippableShield.shieldScript.parryWindow)
                    {
                        if (wielder != null)
                            wielder.animator.CrossFade("Stagger", 0.15f);
                    }
                    else
                    {
                        ip.states.rpg.stamina.ModifyCur(-totalDamage * 1.75f);
                    }
                }
            }
        }


        if (!ip.states.isDodge && !hasHitShield/* && !isHit*/)
        {
            ip.states.Hurt(totalDamage);

            particle = totalDamage == 0 ? "Blue Sparks(Clone)" : "Blood(Clone)";

            if (totalDamage == 0)
                if (wielder != null)
                    wielder.animator.CrossFade("Stagger", 0.15f);

            if (ip.ui != null)
            {
                if (!ip.ui.activeSelf)
                    ip.StartCoroutine(ip.ShowUI());
            }
        }
    }
}
