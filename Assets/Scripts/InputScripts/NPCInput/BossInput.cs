using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossInput : EnemyInput
{
    
    // Update is called once per frame
    override protected void GetInput()
    {
        base.GetInput();
        if (!ui.activeSelf && !states.isDead)
        {
            ui.SetActive(true);
        }
    }
}
