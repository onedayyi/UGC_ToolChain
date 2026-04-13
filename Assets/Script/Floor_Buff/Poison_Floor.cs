using UnityEngine;
using System.Collections.Generic;
using System;

public class Poison_Floor : Buff_Floor
{
    private string thisBuffId = "Buff_poison";  //buffID
    private float thisCheckInterval = 0.5f;     //´¥·¢¼ä¸ô
    public float thisBuffDuration = 10f;
    private void Awake()
    {
        BuffId = thisBuffId;
        CheckInterval = thisCheckInterval;
        BuffDuration = thisBuffDuration;
    }
}