using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawn_Floor : MonoBehaviour
{
    [Header("波次数据")]
    public WaveData waveData;                    // 每个出怪点自己的波次数据

    [Header("敌人预制体")]
    public GameObject enemyPrefab;

    [Header("生成设置")]
    public float spawnHeightOffset = 0.5f;
    public bool useRaycastGround = true;
    public LayerMask groundLayer = 1 << 0;

    [Header("波次控制")]
    public bool autoStart = true;
    public int currentWaveIndex = -1;

    [Header("计时系统")]
    public float countdownTime = 0f;
    public bool isCountingDown = false;

    // 当前使用的波次数据
    private List<WaveSaveData> allWaves;
    private WaveSaveData currentWave;

    // 协程引用
    private Coroutine spawnCoroutine;

    // 存储动态创建的路径点对象
    private List<GameObject> tempWaypoints = new List<GameObject>();

    // 引用 MouseManager（可选，用于通知）
    private MouseManager mouseManager;

#if UNITY_EDITOR
    void OnValidate()
    {
        // 在编辑器中自动查找 MouseManager
        if (mouseManager == null)
        {
            mouseManager = FindObjectOfType<MouseManager>();
        }

        // 在编辑器中创建 WaveData（如果没有）
        if (waveData == null)
        {
            waveData = ScriptableObject.CreateInstance<WaveData>();
            waveData.name = $"WaveData_{gameObject.name}";
            waveData.waves = new List<WaveSaveData>();
            Debug.Log($"为出怪点 {gameObject.name} 创建 WaveData");
        }
    }
#endif

    void Start()
    {
        // 自动查找 MouseManager
        if (mouseManager == null)
        {
            mouseManager = FindObjectOfType<MouseManager>();
        }

        // 初始化波次数据 - 确保每个出怪点有自己的WaveData
        if (waveData == null)
        {
            waveData = ScriptableObject.CreateInstance<WaveData>();
            waveData.name = $"WaveData_{gameObject.name}_{gameObject.GetInstanceID()}";
            waveData.waves = new List<WaveSaveData>();
            Debug.Log($"为出怪点 {gameObject.name} 创建独立的WaveData");
        }

        allWaves = waveData.waves;

        if (autoStart)
            StartSpawning();
    }
    void Update()
    {
        // 倒计时逻辑
        if (isCountingDown)
        {
            countdownTime -= Time.deltaTime;

            if (countdownTime <= 0)
            {
                isCountingDown = false;  // 先关闭倒计时标志
                StartNextWave();          // 开始下一波
            }
        }
    }
    /// <summary>
    /// 获取波次数据（供 MouseManager 调用）
    /// </summary>
    public WaveData GetWaveData()
    {
        // 确保数据存在
        if (waveData == null)
        {
            waveData = ScriptableObject.CreateInstance<WaveData>();
            waveData.name = $"WaveData_{gameObject.name}_{gameObject.GetInstanceID()}";
            waveData.waves = new List<WaveSaveData>();
        }
        return waveData;
    }

    /// <summary>
    /// 设置波次数据（由 MouseManager 回调）
    /// </summary>
    public void SetWaveData(WaveData newWaveData)
    {
        waveData = newWaveData;
        allWaves = waveData.waves;
        Debug.Log($"出怪点 {gameObject.name} 数据已更新，共 {waveData.waves.Count} 个波次");
    }

    /// <summary>
    /// 当鼠标点击选中这个出怪点时调用（由 MouseManager 调用）
    /// </summary>
    public void OnSelected(MouseManager manager)
    {
        Debug.Log($"=== 出怪点被选中: {gameObject.name} ===");

        mouseManager = manager;

        // 确保数据存在
        WaveData dataToSend = GetWaveData();

        // 通知 MouseManager，让它转发数据到 WavePanel
        if (mouseManager != null)
        {
            mouseManager.OnSpawnSelected(this, dataToSend);
        }
    }

    /// <summary>
    /// 开始生成敌人
    /// </summary>
    public void StartSpawning()
    {
        Debug.Log($"=== {gameObject.name} StartSpawning 被调用 ===");

        if (waveData == null)
        {
            Debug.LogError($"出怪点 {gameObject.name} waveData 为 null！");
            return;
        }

        if (waveData.waves == null || waveData.waves.Count == 0)
        {
            //Debug.LogWarning($"出怪点 {gameObject.name}: 没有配置任何波次！");
            return;
        }

        allWaves = waveData.waves;
        Debug.Log($"出怪点 {gameObject.name}: 开始生成，共 {allWaves.Count} 个波次");

        // 打印每个波次的详细信息
        for (int i = 0; i < allWaves.Count; i++)
        {
            var wave = allWaves[i];
            int enemyCount = 0;
            if (wave.enemyConfigs != null)
            {
                foreach (var config in wave.enemyConfigs)
                {
                    enemyCount += config.count;
                }
            }
            Debug.Log($"  波次 {i + 1}: 敌人总数={enemyCount}, 路径点={wave.waypoints?.Count}, 间隔={wave.waveInterval}");
        }

        currentWaveIndex = -1;
        StartCountdownForNextWave();
    }

    /// <summary>
    /// 为下一波开始倒计时
    /// </summary>
    void StartCountdownForNextWave()
    {
        int nextIndex = currentWaveIndex + 1;

        if (nextIndex >= allWaves.Count)
        {
            Debug.Log($"出怪点 {gameObject.name}: 所有波次已完成！");
            isCountingDown = false;
            return;
        }

        WaveSaveData nextWave = allWaves[nextIndex];
        countdownTime = nextWave.waveInterval;
        isCountingDown = true;

        Debug.Log($"出怪点 {gameObject.name}: 第 {nextIndex + 1} 波倒计时开始: {countdownTime} 秒");
    }

    /// <summary>
    /// 开始下一波
    /// </summary>
    void StartNextWave()
    {
        currentWaveIndex++;
        currentWave = allWaves[currentWaveIndex];

        Debug.Log($"=== 出怪点 {gameObject.name}: 开始第 {currentWaveIndex + 1} 波 ===");

        // 清理上一波创建的临时路径点
        ClearTempWaypoints();

        // 停止之前的生成协程（安全起见）
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        // 开始新波次的生成，并等待它完成
        StartCoroutine(StartWaveAndThenCountdown(currentWave));
    }

    /// <summary>
    /// 开始波次，完成后开始下一波倒计时
    /// </summary>
    IEnumerator StartWaveAndThenCountdown(WaveSaveData wave)
    {
        // 执行当前波次
        yield return StartCoroutine(SpawnWave(wave));

        // 当前波次完成后，再开始下一波倒计时
        StartCountdownForNextWave();
    }

    /// <summary>
    /// 生成单个波次的敌人
    /// </summary>
    IEnumerator SpawnWave(WaveSaveData wave)
    {
        Debug.Log($"出怪点 {gameObject.name}: SpawnWave 协程开始");

        if (wave.enemyConfigs == null || wave.enemyConfigs.Count == 0)
        {
            Debug.LogWarning($"出怪点 {gameObject.name}: 波次 {currentWaveIndex + 1} 没有配置敌人");
            yield break;
        }

        // 获取该波次的路径点
        List<WaypointData> waypointData = wave.waypoints;

        if (waypointData == null || waypointData.Count == 0)
        {
            Debug.LogWarning($"出怪点 {gameObject.name}: 波次 {currentWaveIndex + 1} 没有配置路径点");
            yield break;
        }

        Debug.Log($"出怪点 {gameObject.name}: 波次 {currentWaveIndex + 1} 共有 {wave.enemyConfigs.Count} 种敌人，路径点 {waypointData.Count} 个");

        // 遍历所有敌人配置
        foreach (var enemyConfig in wave.enemyConfigs)
        {
            Debug.Log($"  处理敌人配置: {enemyConfig.enemyName}, 数量={enemyConfig.count}");

            // 根据数量生成多个敌人
            for (int i = 0; i < enemyConfig.count; i++)
            {
                Debug.Log($"    生成第 {i + 1}/{enemyConfig.count} 个 {enemyConfig.enemyName}");
                SpawnEnemy(enemyConfig, waypointData);

                // 使用 waveInterval 作为敌人出现间隔
                float interval = wave.waveInterval;
                Debug.Log($"    等待 {interval} 秒后生成下一个敌人");
                yield return new WaitForSeconds(interval);
            }
        }

        Debug.Log($"出怪点 {gameObject.name}: 波次 {currentWaveIndex + 1} 生成完成");
    }

    /// <summary>
    /// 生成单个敌人
    /// </summary>
    void SpawnEnemy(EnemyConfig enemyConfig, List<WaypointData> waypointData)
    {
        Debug.Log($"出怪点 {gameObject.name}: SpawnEnemy 开始 - {enemyConfig.enemyName} (ID: {enemyConfig.enemyId})");

        // 从数据库获取完整的敌人数据
        EnemyData enemyData = EnemyDatabase.Instance?.GetEnemyByID(enemyConfig.enemyId);

        if (enemyData == null)
        {
            Debug.LogError($"出怪点 {gameObject.name}: 找不到敌人ID {enemyConfig.enemyId} 的数据！");
            return;
        }

        // 使用敌人数据中的预制体
        GameObject prefabToUse = enemyData.enemyPrefab;
        if (prefabToUse == null)
        {
            Debug.LogError($"出怪点 {gameObject.name}: 敌人 {enemyData.enemyName} 的预制体为 null！");
            return;
        }

        // 计算生成位置
        Vector3 spawnPosition = CalculateSpawnPosition();

        // 生成敌人，并设置为当前出怪点的子对象
        GameObject enemyObj = Instantiate(prefabToUse, spawnPosition, Quaternion.identity, this.transform);
        enemyObj.name = $"{enemyData.enemyName}_{Time.time}";

        Debug.Log($"  敌人实例化成功: {enemyObj.name}，父对象: {transform.name}");

        // ===== 关键部分：获取 Enemy 组件并赋值 =====
        Enemy enemyComponent = enemyObj.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            // 从 EnemyData 给 Enemy 组件赋值
            enemyComponent.Initialize(enemyData);
            Debug.Log($"Enemy组件初始化: {enemyData.enemyName}, HP:{enemyData.maxHP}, ATK:{enemyData.attack}");
        }
        else
        {
            Debug.LogError($"敌人预制体上没有 Enemy 组件！");
        }

        // 获取EnemyMovement组件并设置路径
        EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            // 提取路径点位置数组
            Vector3[] positions = new Vector3[waypointData.Count];
            for (int i = 0; i < waypointData.Count; i++)
            {
                positions[i] = waypointData[i].position;
                Debug.Log($"  路径点 {i}: {positions[i]}, 停留: {waypointData[i].stayTime}");
            }

            // 设置路径点并开始巡逻
            movement.SetPathPoints(positions, waypointData[0].stayTime);
            Debug.Log($"  路径设置完成，{positions.Length} 个点");
        }
        else
        {
            Debug.LogError($"  敌人预制体上没有 EnemyMovement 组件！");
        }
    }

    /// <summary>
    /// 计算合适的生成位置
    /// </summary>
    Vector3 CalculateSpawnPosition()
    {
        Vector3 basePosition = transform.position;

        if (useRaycastGround)
        {
            // 从上方发射射线检测地面
            RaycastHit hit;
            float rayStartHeight = 10f;
            float rayDistance = 20f;

            Debug.DrawRay(basePosition + Vector3.up * rayStartHeight, Vector3.down * rayDistance, Color.red, 2f);

            if (Physics.Raycast(basePosition + Vector3.up * rayStartHeight, Vector3.down, out hit, rayDistance, groundLayer))
            {
                return hit.point + Vector3.up * 1f;
            }
        }

        return basePosition + Vector3.up * 1f;
    }

    /// <summary>
    /// 根据路径点数据创建Transform数组
    /// </summary>
    Transform[] CreateWaypointTransforms(List<WaypointData> waypointData)
    {
        Transform[] transforms = new Transform[waypointData.Count];

        for (int i = 0; i < waypointData.Count; i++)
        {
            GameObject waypointObj = new GameObject($"TempWaypoint_{i}");
            waypointObj.transform.position = waypointData[i].position;
            tempWaypoints.Add(waypointObj);
            transforms[i] = waypointObj.transform;
        }

        return transforms;
    }

    /// <summary>
    /// 清理临时创建的路径点
    /// </summary>
    void ClearTempWaypoints()
    {
        foreach (var wp in tempWaypoints)
        {
            if (wp != null)
                Destroy(wp);
        }
        tempWaypoints.Clear();
    }

    /// <summary>
    /// 停止生成
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        isCountingDown = false;
        ClearTempWaypoints();
        Debug.Log($"出怪点 {gameObject.name}: 停止生成");
    }

    /// <summary>
    /// 重置波次
    /// </summary>
    public void ResetSpawning()
    {
        StopSpawning();
        currentWaveIndex = -1;
        countdownTime = 0;
        isCountingDown = false;

        Debug.Log($"出怪点 {gameObject.name}: 重置波次");
    }

    void OnDestroy()
    {
        ClearTempWaypoints();
    }
}