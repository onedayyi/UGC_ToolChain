using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    [Header("开始按钮")]
    public Button startWaveButton;

    [Header("控制按钮")]
    public Button stopWaveButton;
    public Button resetWaveButton;

    void Start()
    {
        if (startWaveButton != null)
            startWaveButton.onClick.AddListener(StartAllSpawners);

        if (stopWaveButton != null)
            stopWaveButton.onClick.AddListener(StopAllSpawners);

        if (resetWaveButton != null)
            resetWaveButton.onClick.AddListener(ResetAllSpawners);
    }

    void StartAllSpawners()
    {
        Debug.Log("=== 开始所有出怪点 ===");

        Spawn_Floor[] spawners = FindObjectsOfType<Spawn_Floor>();

        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                // 检查是否有波次配置
                if (spawner.waveData != null && spawner.waveData.waves.Count > 0)
                {
                    Debug.Log($"启动出怪点: {spawner.gameObject.name}，共 {spawner.waveData.waves.Count} 波");
                    spawner.StartSpawning();
                }
                else
                {
                    //Debug.LogWarning($"出怪点 {spawner.gameObject.name} 没有配置波次数据");
                }
            }
        }
    }

    void StopAllSpawners()
    {
        Debug.Log("停止所有出怪点");

        Spawn_Floor[] spawners = FindObjectsOfType<Spawn_Floor>();

        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.StopSpawning();
            }
        }
    }

    void ResetAllSpawners()
    {
        Debug.Log("重置所有出怪点");

        Spawn_Floor[] spawners = FindObjectsOfType<Spawn_Floor>();

        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.ResetSpawning();
            }
        }
    }
}