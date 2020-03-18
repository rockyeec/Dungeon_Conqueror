using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skill_slot : Slot
{
    public override void Start()
    {
        base.Start();
        type = "skill";
    }
}
