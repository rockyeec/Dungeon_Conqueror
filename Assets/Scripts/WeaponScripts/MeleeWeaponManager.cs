using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponManager : WeaponManager
{

    private void OnTriggerEnter(Collider other)
    {
        HurtPeople(other.gameObject);
    }

}
