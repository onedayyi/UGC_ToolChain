using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recover_Floor : Buff_Floor
{
    private string thisBuffId = "Buff_recover";
    private float thisCheckInterval = 0.5f;
    public float thisBuffDuration = 10f;

    private void Awake()
    {
        BuffId = thisBuffId;
        CheckInterval = thisCheckInterval;
        BuffDuration = thisBuffDuration;
    }
}
