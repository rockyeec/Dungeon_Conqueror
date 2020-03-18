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

    /*private void Start()
    {
        StartCoroutine(WaitForSpawnManager());
    }*/

    /*IEnumerator WaitForSpawnManager()*/
    public void Init()
    {
       /* yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();*/

        playerStats = SpawnManager.thePlayer.states.rpg;
    }

    public void GiveXP(float amount)
    {
        playerStats.experience.ModifyCur(amount);
    }

}
