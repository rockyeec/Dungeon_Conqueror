using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RPGManager : MonoBehaviour
{
    [System.Serializable]
    public class Bar
    {
        public float max;
        public float cur;
        public Image bar;

        public Bar()
        {
            cur = max;
        }
        public Bar(float value)
        {
            cur = max = value;
        }
        public Bar(float maxValue, Image bar)
        {
            cur = max = maxValue;
            this.bar = bar;
            this.bar.fillAmount = GetPercentage();
        }

        public void ModifyCur(float amount)
        {
            cur += amount;
            if (cur > max)
            {
                cur = max;
            }

            bar.fillAmount = GetPercentage();
        }

        public void ModifyMax(float amount, float prevPercent)
        {
            max += amount;
            cur = max * prevPercent;
            bar.fillAmount = GetPercentage();
        }

        public float GetPercentage()
        {
            return cur / max;
        }

        public void SetCurToZero()
        {
            cur = 0;
            bar.fillAmount = GetPercentage();
        }

        public void Reset()
        {
            cur = max;
            bar.fillAmount = GetPercentage();
        }
    }


    public int level = 1;

    [HideInInspector]
    public Bar health;
    [HideInInspector]
    public Bar stamina;
    [HideInInspector]
    public Bar experience;

    public float startExp = 100;
    public float startHealth = 33;
    public float startStamina = 33;

    //public float damage = 35;
    public float minDamage = 2;
    public float maxDamage = 7;
    public float moveSpeed = 3.5f;
    // public whatever status1, status2;



    // Inventory
    [HideInInspector]
    public List<PotionScript> potions = new List<PotionScript>();
    

    [SerializeField] float stamRegenRate = 2;


    public void Init()
    {
        health = new Bar(startHealth, GetComponentInChildren<HealthBarIdentifier>().GetComponent<Image>());
        stamina = new Bar(startStamina, GetComponentInChildren<StaminaBarIdentifier>().GetComponent<Image>());


        ExpBarScript expBar = GetComponentInChildren<ExpBarScript>();
        if (expBar != null)
        {
            experience = new Bar(startExp, expBar.GetComponent<Image>());
            experience.SetCurToZero();
        }



        //temporary test potion
        for (int i = 0; i < 3; i++)
        {
            potions.Add(new PotionScript());
        }
    }

    public void Tick()
    {
        if (experience.GetPercentage() >= 1)
        {
            experience.SetCurToZero();
            level++;
            health.ModifyMax(5, health.GetPercentage());
            stamina.ModifyMax(3, stamina.GetPercentage());
            minDamage *= 1.05f;
            maxDamage *= 1.05f;
            moveSpeed *= 1.02f;
            stamRegenRate *= 1.02f;
        }


        
    }

    public void RegenStamina(float delta)
    {
        stamina.ModifyCur(delta * stamRegenRate);
    }

    public bool EnoughStamina()
    {
        return stamina.GetPercentage() > 0.05f;
    }

    public float GetDamage()
    {
        return UnityEngine.Random.Range(minDamage, maxDamage);
    }

}
