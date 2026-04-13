using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class WavePanel : MonoBehaviour
{
    [Header("UI组件")]
    public Button addWaveButton;
    public Transform waveContainer;
    public GameObject wavePrefab;

    [Header("关联面板")]
    public RouteSettingPanel routePanel;
    public CreateConfigurationPanel configPanel;

    [Header("面板控制")]
    public GameObject spawnPanel;  // 整个SpawnPanel的根对象

    private List<EnemyWaveItem> waves = new List<EnemyWaveItem>();
    private EnemyWaveItem currentSelectedWave;

    // 当前显示的数据（副本）
    private WaveData currentDisplayData;

    // 引用 MouseManager 用于回调
    private MouseManager mouseManager;

    void Start()
    {
        if (addWaveButton != null)
            addWaveButton.onClick.AddListener(OnAddWaveClick);

        if (spawnPanel != null)
            spawnPanel.SetActive(false);

        // 自动查找 MouseManager
        if (mouseManager == null)
            mouseManager = FindObjectOfType<MouseManager>();
    }

    /// <summary>
    /// 显示SpawnPanel
    /// </summary>
    public void ShowSpawnPanel()
    {
        if (spawnPanel != null)
        {
            spawnPanel.SetActive(true);
            Debug.Log("显示SpawnPanel");
        }
    }

    /// <summary>
    /// 隐藏SpawnPanel
    /// </summary>
    public void HideSpawnPanel()
    {
        if (spawnPanel != null)
        {
            spawnPanel.SetActive(false);
            Debug.Log("隐藏SpawnPanel");
        }
    }

    /// <summary>
    /// 加载波次数据（由MouseManager调用）
    /// </summary>
    public void LoadWaveData(WaveData waveData, MouseManager manager)
    {
        Debug.Log($"=== WavePanel.LoadWaveData: 加载波次数据 ===");

        mouseManager = manager;
        currentDisplayData = waveData;

        // 显示面板
        ShowSpawnPanel();

        // 刷新UI
        RefreshUI();
    }

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    void RefreshUI()
    {
        // 清空当前所有波次
        ClearAllWaves();

        // 清空其他面板
        if (configPanel != null)
            configPanel.ClearAllStickers();

        if (routePanel != null)
            routePanel.ClearAllWaypoints();

        if (currentDisplayData == null || currentDisplayData.waves == null || currentDisplayData.waves.Count == 0)
        {
            Debug.Log("没有波次数据，创建默认空波次");
            // 创建一个默认波次方便编辑
            CreateDefaultWave();
            return;
        }

        Debug.Log($"找到 {currentDisplayData.waves.Count} 个波次数据");

        // 创建保存的波次
        foreach (var saveData in currentDisplayData.waves)
        {
            GameObject waveObj = Instantiate(wavePrefab, waveContainer);
            EnemyWaveItem waveItem = waveObj.GetComponent<EnemyWaveItem>();

            if (waveItem != null)
            {
                waveItem.LoadFromSaveData(saveData, this);
                waves.Add(waveItem);
                Debug.Log($"加载波次: {saveData.waveName}, 敌人: {saveData.enemyConfigs?.Count}, 路径: {saveData.waypoints?.Count}");
            }
        }

        // 如果有波次，选中第一个
        if (waves.Count > 0)
        {
            SelectWave(waves[0]);
        }
    }

    /// <summary>
    /// 创建默认波次
    /// </summary>
    void CreateDefaultWave()
    {
        if (wavePrefab == null || waveContainer == null) return;

        GameObject waveObj = Instantiate(wavePrefab, waveContainer);
        EnemyWaveItem waveItem = waveObj.GetComponent<EnemyWaveItem>();

        if (waveItem != null)
        {
            int index = waves.Count + 1;
            waveItem.Initialize(index, this);
            waves.Add(waveItem);

            SelectWave(waveItem);
        }

        // 通知数据改变
        NotifyDataChanged();
    }

    /// <summary>
    /// 点击添加波次按钮
    /// </summary>
    void OnAddWaveClick()
    {
        if (wavePrefab == null || waveContainer == null) return;

        GameObject waveObj = Instantiate(wavePrefab, waveContainer);
        EnemyWaveItem waveItem = waveObj.GetComponent<EnemyWaveItem>();

        if (waveItem != null)
        {
            int index = waves.Count + 1;
            waveItem.Initialize(index, this);
            waves.Add(waveItem);

            SelectWave(waveItem);
        }

        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = waveContainer.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        // 数据改变，通知MouseManager
        NotifyDataChanged();
    }

    /// <summary>
    /// 选中某个波次
    /// </summary>
    public void SelectWave(EnemyWaveItem waveItem)
    {
        if (currentSelectedWave != null)
        {
            currentSelectedWave.SetHighlight(false);
        }

        currentSelectedWave = waveItem;
        currentSelectedWave.SetHighlight(true);

        UpdateOtherPanels(waveItem);
    }

    /// <summary>
    /// 更新其他面板显示当前波次的数据
    /// </summary>
    void UpdateOtherPanels(EnemyWaveItem waveItem)
    {
        if (waveItem == null) return;

        Debug.Log($"更新其他面板: 波次 {waveItem.GetWaveId()}");

        if (configPanel != null)
        {
            configPanel.LoadWaveData(waveItem.GetWaveId(), waveItem.GetEnemyConfigs());
        }

        if (routePanel != null)
        {
            routePanel.ShowForWave(waveItem.GetWaveId(), waveItem.GetWaypoints());
        }
    }

    /// <summary>
    /// 当配置面板中的数据改变时调用
    /// </summary>
    public void OnConfigPanelChanged(string waveId, List<EnemyConfig> enemyConfigs)
    {
        EnemyWaveItem waveItem = FindWaveById(waveId);
        if (waveItem != null)
        {
            int totalEnemies = 0;
            foreach (var config in enemyConfigs)
            {
                totalEnemies += config.count;
            }
            waveItem.UpdateTotalEnemies(totalEnemies);
            waveItem.UpdateEnemyConfigs(enemyConfigs);

            // 数据改变，通知MouseManager
            NotifyDataChanged();
        }
    }

    /// <summary>
    /// 当路线面板中的数据改变时调用
    /// </summary>
    public void OnRoutePanelChanged(string waveId, List<WaypointData> waypoints)
    {
        EnemyWaveItem waveItem = FindWaveById(waveId);
        if (waveItem != null)
        {
            waveItem.UpdateTotalWaypoints(waypoints.Count);
            waveItem.UpdateWaypoints(waypoints);

            // 数据改变，通知MouseManager
            NotifyDataChanged();
        }
    }

    /// <summary>
    /// 通知MouseManager数据已改变
    /// </summary>
    void NotifyDataChanged()
    {
        if (mouseManager != null && currentDisplayData != null)
        {
            // 更新currentDisplayData
            currentDisplayData.waves = GetAllWaveData();

            // 通过MouseManager传回数据
            mouseManager.OnWaveDataUpdated(currentDisplayData);
            Debug.Log($"通知MouseManager数据已更新，共 {currentDisplayData.waves.Count} 个波次");
        }
    }

    /// <summary>
    /// 删除波次
    /// </summary>
    public void RemoveWave(EnemyWaveItem waveItem)
    {
        if (waves.Contains(waveItem))
        {
            int index = waves.IndexOf(waveItem);
            waves.Remove(waveItem);
            Destroy(waveItem.gameObject);

            if (currentSelectedWave == waveItem)
            {
                currentSelectedWave = null;

                if (configPanel != null)
                    configPanel.ClearAllStickers();

                if (routePanel != null)
                    routePanel.ClearAllWaypoints();
            }

            RefreshWaveIndices();

            if (waves.Count > 0)
            {
                int newIndex = Mathf.Min(index, waves.Count - 1);
                SelectWave(waves[newIndex]);
            }

            // 数据改变，通知MouseManager
            NotifyDataChanged();
        }
    }

    /// <summary>
    /// 刷新所有波次序号
    /// </summary>
    void RefreshWaveIndices()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            waves[i].UpdateIndex(i + 1);
        }
    }

    /// <summary>
    /// 根据ID查找波次
    /// </summary>
    EnemyWaveItem FindWaveById(string waveId)
    {
        foreach (var wave in waves)
        {
            if (wave.GetWaveId() == waveId)
                return wave;
        }
        return null;
    }

    /// <summary>
    /// 获取所有波次数据
    /// </summary>
    public List<WaveSaveData> GetAllWaveData()
    {
        List<WaveSaveData> allData = new List<WaveSaveData>();
        foreach (var wave in waves)
        {
            allData.Add(wave.GetWaveSaveData());
        }
        return allData;
    }

    /// <summary>
    /// 清空所有波次
    /// </summary>
    void ClearAllWaves()
    {
        foreach (var wave in waves)
        {
            if (wave != null)
                Destroy(wave.gameObject);
        }
        waves.Clear();
        currentSelectedWave = null;
    }

    /// <summary>
    /// 获取当前显示的数据（供调试用）
    /// </summary>
    public WaveData GetCurrentDisplayData()
    {
        return currentDisplayData;
    }
}