using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Attack
{
    public string name;
    public Collider col;

    public float damageValue;
    public float activeTime;
    public float recoveryTime;
    public float stunTime;
}
