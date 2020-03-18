using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class item_slot : Slot
{
    public override void Start()
    {
        base.Start();
        type = "item";
    }

}
