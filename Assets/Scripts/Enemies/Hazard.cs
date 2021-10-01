using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Stats")]
    public int damage = 1;
    public bool stunned = false;

    public virtual void Reset()
    {
        stunned = false;
    }
}
