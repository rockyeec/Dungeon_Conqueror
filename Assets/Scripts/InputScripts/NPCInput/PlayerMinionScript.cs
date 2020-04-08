using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMinionScript : AllyInput
{
    [HideInInspector]
    public int index;

    protected override void Awake()
    {
        base.Awake();

        states.OnDie += States_OnDie;
    }

    private void States_OnDie()
    {
        commander.states.aem.RemoveMinionFromList(index);
        commander.UpdateStatsText();
    }
}
