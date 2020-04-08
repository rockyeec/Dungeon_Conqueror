using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInput : NPCInput
{
    float explosionForce = 8;
    public List<string> weaponList = new List<string>();
    public List<string> shieldList = new List<string>();

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

        if (states.rpg.currentWeapon != null)
            states.rpg.currentWeapon.Unequip();

        if (states.rpg.currentShield != null)
            states.rpg.currentShield.Unequip();
    }

    private void States_OnRevive()
    {
        if (weaponList.Count != 0)
        {
            int random = UnityEngine.Random.Range(0, weaponList.Count);
            GameObject temp = ObjectPool.Instance.GetObject(weaponList[random], Vector3.zero, Quaternion.identity);
            rpg.currentWeapon = temp.GetComponent<IAttackable>();
            rpg.currentWeapon.Equip(states.aem);


            backOffDistance = 1.45f;
            if (temp.name == "Bow(Clone)")
            {
                stopDistance = 25;
            }
            else
            {
                stopDistance = 2.72f;
            }
        }

        if (shieldList.Count != 0)
        {
            int random = UnityEngine.Random.Range(0, shieldList.Count);
            rpg.currentShield = ObjectPool.Instance.GetObject(shieldList[random], Vector3.zero, Quaternion.identity).GetComponent<EquippableShield>();
            rpg.currentShield.Equip(states.aem);
        }
    }
}
