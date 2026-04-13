using UnityEngine;
using System.Collections.Generic;
using System;

public class Ice_Floor : Buff_Floor
{
    private string thisBuffId = "Buff_ice";  //buffID
    private float thisCheckInterval = 0.5f;     //触发间隔
    public float thisBuffDuration = 10f;
    private void Awake()
    {
        BuffId = thisBuffId;
        CheckInterval = thisCheckInterval;
        BuffDuration = thisBuffDuration;
    }
}