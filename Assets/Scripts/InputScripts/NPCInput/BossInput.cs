using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    protected override void InitUI()
    {
        ui = ObjectPool.Instance.GetObject("Boss_Canvas(Clone)", transform);
        text = ui.GetComponentInChildren<TextMeshProUGUI>();
    }

}
