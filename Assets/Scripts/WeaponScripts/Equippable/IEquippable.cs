using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquippable
{
    void Equip(AnimatorEventManager wielder);
    void Unequip();    
}

public interface IAttackable : IEquippable
{
    void Attack();

    void EndAttack();

    void LaunchArrow();

    void LaunchMultipleArrowsVertically();
    void LaunchMultipleArrowsHorizontally();

    StatesManager.MoveSet MoveSet { get; }
}
