using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 波次保存数据
/// </summary>
[System.Serializable]
public class WaveSaveData
{
    public string waveId;                       // 波次ID
    public string waveName;                      // 波次名称（新增）
    public int index;                            // 序号
    public float waveInterval = 2f;               // 波次间隔
    public List<EnemyConfig> enemyConfigs;        // 敌人配置
    public List<WaypointData> waypoints;          // 路径点
}

/// <summary>
/// 敌人配置数据
/// </summary>
[System.Serializable]
public class EnemyConfig
{
    public string enemyId;
    public string enemyName;
    public GameObject enemyPrefab;
    public int count = 1;
}

/// <summary>
/// 路径点数据结构
/// </summary>
[System.Serializable]
public class WaypointData
{
    public Vector3 position;        // 世界坐标
    public float stayTime = 1f;      // 停留时间（单位：秒）
    public int index;                // 序号
}
/// <summary>
/// 出怪点的波次数据（ScriptableObject便于保存）
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "Spawn/WaveData")]
public class WaveData : ScriptableObject
{
    public List<WaveSaveData> waves = new List<WaveSaveData>();
}