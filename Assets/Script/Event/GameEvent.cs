// GameEvents.cs - 全局事件系统
using UnityEngine;
using System;

public static class GameEvents
{
    // 地块Buff事件
    public static System.Action<string, GameObject, GameObject, float> OnTileBuffEnter;
    public static System.Action<string, GameObject, GameObject> OnTileBuffExit;
    public static Action<string, GameObject, GameObject, float> OnTileBuffStay;

    public static void TriggerBuff(string buffId, GameObject target, GameObject source, float duration)
    {
        OnTileBuffEnter?.Invoke(buffId, target, source, duration);
    }

    public static void TriggerBuffExit(string buffId, GameObject target, GameObject source)
    {
        OnTileBuffExit?.Invoke(buffId, target, source);
    }
    public static void TriggerBuffStay(string buffId, GameObject target, GameObject source, float duration)
    {
        OnTileBuffStay?.Invoke(buffId, target, source, duration);
    }
}