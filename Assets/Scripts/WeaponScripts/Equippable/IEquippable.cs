using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquippable
{
    string Name { get; }
    string Type { get; }

    void Equip(AnimatorEventManager wielder);
    IEquippable Unequip();    
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
