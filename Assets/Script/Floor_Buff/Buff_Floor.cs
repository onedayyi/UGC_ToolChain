using UnityEngine;
using System.Collections.Generic;

public class Buff_Floor : MonoBehaviour
{
    public virtual string BuffId { get; set; } = "Buff_recover";
    public virtual float CheckInterval { get; set; } = 0.2f;  // 周期性检测间隔
    public virtual float BuffDuration { get; set; } = 10f;

    // 记录当前在地块上的敌人
    private HashSet<GameObject> enemiesOnFloor = new HashSet<GameObject>();
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= CheckInterval)
        {
            ApplyBuffToAllEnemies();
            timer = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesOnFloor.Add(other.gameObject);
            Debug.Log($"敌人进入地板，当前地块敌人数量: {enemiesOnFloor.Count}");

            // 进入时立即施加Buff
            GameEvents.TriggerBuff(BuffId, other.gameObject, this.gameObject, this.BuffDuration);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemiesOnFloor.Remove(other.gameObject);
            Debug.Log($"敌人离开当前地板，当前地块敌人数量: {enemiesOnFloor.Count}");
        }
    }

    void ApplyBuffToAllEnemies()
    {
        if (enemiesOnFloor.Count == 0) return;

        foreach (var enemy in enemiesOnFloor)
        {
            if (enemy != null)
            {
                GameEvents.TriggerBuff(BuffId, enemy, this.gameObject, this.BuffDuration);
                Debug.Log($"周期性检测：给 {enemy.name} 施加 {BuffId}");
            }
        }
    }
}