using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public RPGManager rpg;

    [Header("Input")]
    public string playerName = "";
    public string playerClass = "";

    [Header("HP")]
    [Space(5)]
    public int curHP = 10;
    public int maxHP = 15;

    [Header("MP")]
    [Space(5)]
    public int curMP = 10;
    public int maxMP = 15;

    [Header("XP")]
    [Space(5)]
    public int curXP = 10;
    public int maxXP = 15;
    public int curLvl = 1;

    [Header("Stats")]
    [Space(5)]
    public int curAttack = 1;
    public int curSpeed = 1;
    public int curSkillPoint = 1;

    
    [Header("Dependencies")]
    [Space(30)]
    public TextMeshProUGUI player_name;
    public TextMeshProUGUI player_class;

    public Transform curHPBar;
    public TextMeshProUGUI hpText;
   

    public Transform curMPBar;
    public TextMeshProUGUI mpText;
    

    public Transform curXPBar;
    public TextMeshProUGUI xpText;
   
    public TextMeshProUGUI lvlText;

    public TextMeshProUGUI statAttack;
    public TextMeshProUGUI statSpeed;
    public TextMeshProUGUI statSkill;

    private void OnEnable()
    {
        if (rpg == null) return;

        UpdateName();
        UpdateHP();
        UpdateMP();
        UpdateXP();
        UpdateStatus();
    }

    public void UpdateHP()
    {
        curHP = (int)rpg.health.cur;
        maxHP = (int)rpg.health.max;

        if (curHP <= 0)
        {
            curHP = 0;
        }

        if(curHP >= maxHP)
        {
            curHP = maxHP;
        }

        hpText.text = curHP + " / " + maxHP;

        curHPBar.localScale = new Vector3((float)curHP / (float)maxHP, curHPBar.localScale.y, curHPBar.localScale.z); 

    }

    public void UpdateMP()
    {
        curMP = (int)rpg.stamina.cur;
        maxMP = (int)rpg.stamina.max;

        if (curMP <= 0)
        {
            curMP = 0;
        }

        if (curMP >= maxMP)
        {
            curMP = maxMP;
        }

        mpText.text = curMP + " / " + maxMP;

        curMPBar.localScale = new Vector3((float)curMP / (float)maxMP, curMPBar.localScale.y, curMPBar.localScale.z);

    }

    public void UpdateXP()
    {
        curLvl = rpg.level;
        curXP = (int)rpg.experience.cur;
        maxXP = (int)rpg.experience.max;

        if (curXP <= 0)
        {
            curXP = 0;
        }

        if (curXP >= maxXP)
        {
            curXP = maxXP;
        }

        xpText.text = curXP + " / " + maxXP;

        lvlText.text = "Level " + curLvl;

        curXPBar.localScale = new Vector3((float)curXP / (float)maxXP, curXPBar.localScale.y, curXPBar.localScale.z);
    }

    public void UpdateName()
    {

        player_name.text = playerName;
        player_class.text = playerClass;

    }

    public void UpdateStatus()
    {
        curAttack = (int)((rpg.minDamage + rpg.maxDamage) / 2);
        curSpeed = (int)rpg.moveSpeed;


        statAttack.text = curAttack.ToString();
        statSpeed.text = curSpeed.ToString();
        statSkill.text = curSkillPoint.ToString();

    }

}
