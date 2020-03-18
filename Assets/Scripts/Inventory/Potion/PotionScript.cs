using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionScript// : MonoBehaviour
{
    public void UsePotion(RPGManager rpg)
    {
        rpg.health.ModifyCur(55);
    }
}
