using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RPGManager : MonoBehaviour
{
    [System.Serializable]
    public class Bar
    {
        public float max;
        public float cur;
        public Image bar;
        public MiddleBarScript middle;

        public Bar(float maxValue, Image bar, MiddleBarScript middle)
        {
            cur = max = maxValue;
            this.bar = bar;
            this.bar.fillAmount = GetPercentage();

            if (middle == null) return;

            this.middle = middle;
            this.middle.Init(GetPercentage());
        }

        public void ModifyCur(float amount)
        {
            cur += amount;
            if (cur > max)
            {
                cur = max;
            }

            UpdateBars();
        }

        public void ModifyMax(float amount, float prevPercent)
        {
            max += amount;
            cur = max * prevPercent;
            UpdateBars();
        }

        public float GetPercentage()
        {
            return cur / max;
        }

        public void SetCurToZero()
        {
            cur = 0;
            UpdateBars();
        }

        public void Reset()
        {
            cur = max;
            UpdateBars();
        }

        public void SetMax(float toBecome)
        {
            float per = GetPercentage();
            max = toBecome;
            cur = max * per;
            UpdateBars();
        }

        public void UpdateBars()
        {
            if (!bar.transform.parent.parent.gameObject.activeSelf) return;
            bar.fillAmount = GetPercentage();
            if (middle == null) return;
            middle.ChangePercentageTo(GetPercentage());
        }
    }



    public string characterName;
    public int level = 1;
    [HideInInspector]
    public Bar health;
    [HideInInspector]
    public Bar stamina;
    [HideInInspector]
    public Bar experience;

    float startExp = 10;
    public float startHealth = 12;
    public float startStamina = 12;
    public float startMinDamage = 2;
    public float startMaxDamage = 5;
    public float startMoveSpeed = 3.5f;
    public float startDefence = 1.2f;
    public float startStamRegenRate = 8;

    [HideInInspector] public float minDamage;
    [HideInInspector] public float maxDamage;
    float moveSpeed;
    float defence;
    float stamRegenRate;



    // Inventory
    [HideInInspector]
    public List<Pickuppable> generalStuff = new List<Pickuppable>();
    [HideInInspector]
    public List<IAttackable> weapons = new List<IAttackable>();
    [HideInInspector]
    public List<EquippableShield> shields = new List<EquippableShield>();
    [HideInInspector]
    public List<Queue<PotionScript>> potions = new List<Queue<PotionScript>>();
    [HideInInspector]
    public List<float> statModifier = new List<float>();
    GameObject potionEffect;
    





    //events
    public event Action OnLevelUp = delegate { };
    public event Action OnDrinkPotion = delegate { };
    

    public void Init()
    {
        for (int i = 0; i < PotionScript.potionTypes.Count; i++)
        {
            potions.Add(new Queue<PotionScript>());
            statModifier.Add(new float());
        }

        Image hbi = GetComponentInChildren<HealthBarIdentifier>().GetComponent<Image>();
        Image sbi = GetComponentInChildren<StaminaBarIdentifier>().GetComponent<Image>();
        health = new Bar(startHealth, hbi, hbi.transform.parent.GetComponentInChildren<MiddleBarScript>());
        stamina = new Bar(startStamina, sbi, sbi.transform.parent.GetComponentInChildren<MiddleBarScript>());


        ExpBarScript expBar = GetComponentInChildren<ExpBarScript>();
        if (expBar != null)
        {
            experience = new Bar(startExp, expBar.GetComponent<Image>(), expBar.transform.parent.GetComponentInChildren<MiddleBarScript>());
            experience.SetCurToZero();
        }

        PotionEffectIdentifier temp = GetComponentInChildren<PotionEffectIdentifier>();
        if (temp != null)
        {
            potionEffect = temp.gameObject;
            potionEffect.SetActive(false);
        }
    }

    public void TriggerGiveXP(float amount)
    {
        if (experience == null) return;

        experience.cur += amount;
        if (experience.cur >= experience.max)
        {
            experience.cur %= experience.max;
            UpdateStatsAccordingToLevel(++level);
            OnLevelUp();
        }
        experience.max *= 1.01f; // increase by 1%
        experience.UpdateBars();
    }

    public void UpdateStatsAccordingToLevel(int level)
    {
        this.level = level;
        health.SetMax(level * 5 + startHealth);
        stamina.SetMax(level * 5 + startStamina);
        minDamage = startMinDamage + 0.88f * level;
        maxDamage = startMaxDamage + 1.2f * level;
        moveSpeed = startMoveSpeed + 0.05f * level;
        stamRegenRate = startStamRegenRate + 0.15f * level;
        defence = startDefence + 0.5f * level;
    }

    public bool EnoughStamina()
    {
        return stamina.GetPercentage() > 0.05f;
    }



    // Potion Effects

    public void RegenHealth(float delta)
    {
        if (statModifier[0] == 0)
            return;

        health.ModifyCur(delta * statModifier[0]); // refer to PotionScript for content of index
    }

    public void RegenStamina(float delta)
    {
        stamina.ModifyCur(delta * (stamRegenRate + statModifier[1])); // refer to PotionScript for content of index
    }

    public float GetSpeed()
    {
        // Cap speed at 5.5
        float temp = moveSpeed + statModifier[2]; // refer to PotionScript for content of index
        if (temp > 5.5f)
            temp = 5.5f;
        return temp;
    }

    public float GetDamage()
    {
        return UnityEngine.Random.Range(minDamage, maxDamage) + statModifier[3]; // refer to PotionScript for content of index
    }

    public float GetDefence()
    {
        return defence + statModifier[4]; // refer to PotionScript for content of index
    }


    public bool IsPotionsEmpty()
    {
        for (int i = 0; i < PotionScript.potionTypes.Count; i++)
        {
            if (potions[i].Count != 0)
                return false;
        }
        return true;
    }

    public IEnumerator PotionEffect(int index, float value, float duration)
    {
        statModifier[index] += value;

        if (potionEffect != null)
            potionEffect.SetActive(true);

        OnDrinkPotion();
        yield return new WaitForSeconds(duration);

        statModifier[index] -= value;
        OnDrinkPotion();
        
        for (int i = 0; i < statModifier.Count; i++)
        {
            if (statModifier[i] != 0)
                yield break;
        }

        if (potionEffect != null)
            potionEffect.SetActive(false);

    }
}
