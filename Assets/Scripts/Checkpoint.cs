using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public float yPos;
    public bool active;
    public GameObject particles;

    void Start()
    {
        yPos = transform.position.y;
    }

    public void ActivateCkeckpoint()
    {
        if (!active)
        {
            active = true;
            particles.SetActive(true);
        }
    }
}
