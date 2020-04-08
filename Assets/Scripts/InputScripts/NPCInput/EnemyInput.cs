using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInput : NPCInput
{
    float explosionForce = 8;

    protected override void Awake()
    {
        base.Awake();
        hostileLayer = 10;
       states.aem.friendlyLayer = 11;
        states.OnRevive += States_OnRevive;
        states.OnDie += States_OnDie;
    }

    private void States_OnDie()
    {
        int rand = Random.Range(1, 3);
        for (int i = 0; i < rand; i++)
        {
            GameObject potion = ObjectPool.Instance.GetObject("Potion(Clone)", states.aem.body.position, Quaternion.identity);
            Quaternion rot = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360)));
            potion.GetComponent<PotionScript>().rigidBody.AddForce((rot * Vector3.forward + Vector3.up).normalized * explosionForce, ForceMode.Impulse);

            AwardManager.Instance.GiveXP(states.rpg.level * 0.1f + 1);
        }
    }

    private void States_OnRevive()
    {
       
    }
}
