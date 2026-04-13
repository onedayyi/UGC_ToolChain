using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EnemyWaveItem : MonoBehaviour
{
    [Header("UI组件")]
    public TMP_Text indexText;                    // 波次序号
    public TMP_Text totalEnemiesText;              // 敌人总数
    public TMP_Text totalWaypointsText;             // 路径点总数
    public TMP_InputField waveIntervalInput;        // 波次间隔
    public Button deleteButton;                      // 删除按钮
    public Button selectButton;                      // 选中按钮（整个预制体点击）

    [Header("数据")]
    public string waveId;                            // 波次唯一ID
    private int waveIndex;                            // 波次序号
    private float waveInterval = 2f;                   // 默认波次间隔2秒
    private List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();
    private List<WaypointData> waypoints = new List<WaypointData>();

    private WavePanel parentPanel;
    private Image backgroundImage;                    // 用于高亮显示

    void Awake()
    {
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
            backgroundImage = gameObject.AddComponent<Image>();
    }

    void Start()
    {
        // 绑定按钮事件
        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClick);

        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectClick);
        else
            // 如果没有专门的selectButton，就用整个预制体点击
            GetComponent<Button>().onClick.AddListener(OnSelectClick);

        if (waveIntervalInput != null)
            waveIntervalInput.onEndEdit.AddListener(OnWaveIntervalChanged);
    }

    /// <summary>
    /// 初始化新波次
    /// </summary>
    public void Initialize(int index, WavePanel panel)
    {
        waveIndex = index;
        parentPanel = panel;

        // 生成唯一ID
        waveId = System.Guid.NewGuid().ToString();

        // 更新UI
        UpdateIndex(index);
        UpdateTotalEnemies(0);
        UpdateTotalWaypoints(0);

        // 设置默认间隔
        if (waveIntervalInput != null)
            waveIntervalInput.text = waveInterval.ToString();
    }

    /// <summary>
    /// 从保存数据加载
    /// </summary>
    public void LoadFromSaveData(WaveSaveData data, WavePanel panel)
    {
        waveId = data.waveId;
        waveIndex = data.index;
        waveInterval = data.waveInterval;
        enemyConfigs = data.enemyConfigs ?? new List<EnemyConfig>();
        waypoints = data.waypoints ?? new List<WaypointData>();
        parentPanel = panel;

        // 更新UI
        UpdateIndex(waveIndex);
        UpdateTotalEnemies(GetTotalEnemyCount());
        UpdateTotalWaypoints(waypoints.Count);

        if (waveIntervalInput != null)
            waveIntervalInput.text = waveInterval.ToString();
    }

    /// <summary>
    /// 更新波次序号
    /// </summary>
    public void UpdateIndex(int newIndex)
    {
        waveIndex = newIndex;
        if (indexText != null)
            indexText.text = $"{newIndex}";
    }

    /// <summary>
    /// 更新敌人总数
    /// </summary>
    public void UpdateTotalEnemies(int count)
    {
        if (totalEnemiesText != null)
            totalEnemiesText.text = $"敌人:{count}";
    }

    /// <summary>
    /// 更新路径点总数
    /// </summary>
    public void UpdateTotalWaypoints(int count)
    {
        if (totalWaypointsText != null)
            totalWaypointsText.text = $"路径:{count}";
    }

    /// <summary>
    /// 设置高亮状态
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        if (backgroundImage != null)
        {
            if (highlight)
                backgroundImage.color = new Color(0.2f, 0.5f, 0.8f, 0.3f); // 蓝色高亮
            else
                backgroundImage.color = new Color(1, 1, 1, 0.1f); // 半透明
        }
    }

    /// <summary>
    /// 点击波次时调用
    /// </summary>
    void OnSelectClick()
    {
        if (parentPanel != null)
        {
            parentPanel.SelectWave(this);
        }
    }

    /// <summary>
    /// 点击删除按钮
    /// </summary>
    void OnDeleteClick()
    {
        if (parentPanel != null)
        {
            parentPanel.RemoveWave(this);
        }
    }

    /// <summary>
    /// 波次间隔改变
    /// </summary>
    void OnWaveIntervalChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            waveInterval = Mathf.Max(0.1f, result);
        }
        else
        {
            waveIntervalInput.text = waveInterval.ToString();
        }
    }

    /// <summary>
    /// 更新敌人配置（从配置面板）
    /// </summary>
    public void UpdateEnemyConfigs(List<EnemyConfig> configs)
    {
        enemyConfigs = configs;
        UpdateTotalEnemies(GetTotalEnemyCount());
    }

    /// <summary>
    /// 更新路径点（从路线面板）
    /// </summary>
    public void UpdateWaypoints(List<WaypointData> newWaypoints)
    {
        waypoints = newWaypoints;
        UpdateTotalWaypoints(waypoints.Count);
    }

    /// <summary>
    /// 计算总敌人数
    /// </summary>
    int GetTotalEnemyCount()
    {
        int total = 0;
        foreach (var config in enemyConfigs)
        {
            total += config.count;  // 现在可以正常访问 count 了
        }
        return total;
    }

    /// <summary>
    /// 获取波次ID
    /// </summary>
    public string GetWaveId()
    {
        return waveId;
    }

    /// <summary>
    /// 获取敌人配置
    /// </summary>
    public List<EnemyConfig> GetEnemyConfigs()
    {
        Debug.Log($"波次 {waveIndex} 返回敌人配置: {enemyConfigs?.Count} 个");
        foreach (var config in enemyConfigs)
        {
            Debug.Log($"  敌人: {config.enemyName}, 数量: {config.count}");
        }
        return enemyConfigs;
    }

    public List<WaypointData> GetWaypoints()
    {
        Debug.Log($"波次 {waveIndex} 返回路径点: {waypoints?.Count} 个");
        return waypoints;
    }

    /// <summary>
    /// 获取波次间隔
    /// </summary>
    public float GetWaveInterval()
    {
        return waveInterval;
    }

    /// <summary>
    /// 获取保存数据
    /// </summary>
    public WaveSaveData GetWaveSaveData()
    {
        return new WaveSaveData
        {
            waveId = waveId,
            index = waveIndex,
            waveInterval = waveInterval,
            enemyConfigs = enemyConfigs,
            waypoints = waypoints
        };
    }
}