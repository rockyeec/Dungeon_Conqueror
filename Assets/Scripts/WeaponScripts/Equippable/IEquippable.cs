﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquippable
{
    void Equip(AnimatorEventManager wielder);
    void Unequip();
}
