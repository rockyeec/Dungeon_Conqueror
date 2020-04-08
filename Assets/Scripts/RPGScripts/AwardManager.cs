using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwardManager : MonoBehaviour
{
    [HideInInspector]
    public RPGManager playerStats;

    public static AwardManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;        
    }
    public void Init()
    {
        playerStats = GameMasterScript.thePlayer.states.rpg;
    }

    public void GiveXP(float amount)
    {
        playerStats.TriggerGiveXP(amount);
    }

}
