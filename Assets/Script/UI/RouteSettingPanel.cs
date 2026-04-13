using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RouteSettingPanel : MonoBehaviour
{
    [Header("UI组件")]
    public Button addWaypointButton;
    public Transform waypointContainer;
    public GameObject waypointPrefab;

    [Header("当前波次信息")]
    public string currentWaveId;
    public string currentWaveName;  // 用于调试

    private List<EnemyWaypointItem> waypointItems = new List<EnemyWaypointItem>();
    public MouseManager mouseManager;

    // 防止重复加载的标志
    private bool isLoading = false;

    void Start()
    {
        if (addWaypointButton != null)
            addWaypointButton.onClick.AddListener(OnAddWaypointClick);
    }

    /// <summary>
    /// 显示指定波次的路线设置
    /// </summary>
    public void ShowForWave(string waveId, List<WaypointData> waypoints)
    {
        // 如果正在加载相同的波次，不重复处理
        if (currentWaveId == waveId && isLoading) return;

        Debug.Log($"显示波次 {waveId} 的路线，路径点数量: {waypoints?.Count}");

        currentWaveId = waveId;
        gameObject.SetActive(true);

        // 强制清空并重新加载
        ForceLoadWaypoints(waypoints);
    }

    /// <summary>
    /// 强制重新加载路径点
    /// </summary>
    void ForceLoadWaypoints(List<WaypointData> waypointDataList)
    {
        isLoading = true;

        // 彻底清空现有路径点
        ClearAllWaypointsImmediate();

        if (waypointDataList != null && waypointDataList.Count > 0)
        {
            // 根据保存的数据创建路径点
            foreach (var waypointData in waypointDataList)
            {
                CreateWaypointItem(waypointData);
            }

            Debug.Log($"重新加载完成，当前 {waypointItems.Count} 个路径点");
        }

        // 强制刷新UI布局
        Canvas.ForceUpdateCanvases();

        isLoading = false;
    }

    /// <summary>
    /// 创建单个路径点项
    /// </summary>
    EnemyWaypointItem CreateWaypointItem(WaypointData data)
    {
        if (waypointPrefab == null || waypointContainer == null) return null;

        GameObject waypointObj = Instantiate(waypointPrefab, waypointContainer);
        EnemyWaypointItem waypointItem = waypointObj.GetComponent<EnemyWaypointItem>();

        if (waypointItem != null)
        {
            waypointItem.Initialize(data.index, this);
            waypointItem.UpdatePosition(data.position);
            waypointItem.SetStayTime(data.stayTime);
            waypointItems.Add(waypointItem);

            return waypointItem;
        }

        return null;
    }

    /// <summary>
    /// 立即清空所有路径点
    /// </summary>
    void ClearAllWaypointsImmediate()
    {
        // 逆序销毁，避免修改列表时的错误
        for (int i = waypointItems.Count - 1; i >= 0; i--)
        {
            if (waypointItems[i] != null && waypointItems[i].gameObject != null)
            {
                DestroyImmediate(waypointItems[i].gameObject);
            }
        }
        waypointItems.Clear();

        // 强制清理容器下的所有子对象（双重保障）
        if (waypointContainer != null)
        {
            foreach (Transform child in waypointContainer)
            {
                if (child != null && child.gameObject != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 点击添加路径点按钮
    /// </summary>
    void OnAddWaypointClick()
    {
        if (waypointPrefab == null || waypointContainer == null) return;

        int index = waypointItems.Count + 1;

        // 创建默认路径点数据
        WaypointData newData = new WaypointData
        {
            index = index,
            position = Vector3.zero,
            stayTime = 1f
        };

        EnemyWaypointItem newItem = CreateWaypointItem(newData);

        if (newItem != null)
        {
            // 滚动到最新添加的项
            Canvas.ForceUpdateCanvases();
            ScrollRect scrollRect = waypointContainer.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }

            NotifyWavePanelDataChanged();
        }
    }

    /// <summary>
    /// 删除指定的路径点
    /// </summary>
    public void RemoveWaypoint(EnemyWaypointItem waypointItem)
    {
        if (waypointItems.Contains(waypointItem))
        {
            waypointItems.Remove(waypointItem);

            if (waypointItem != null && waypointItem.gameObject != null)
            {
                Destroy(waypointItem.gameObject);
            }

            RefreshWaypointIndices();
            NotifyWavePanelDataChanged();
        }
    }

    /// <summary>
    /// 刷新所有路径点序号
    /// </summary>
    void RefreshWaypointIndices()
    {
        for (int i = 0; i < waypointItems.Count; i++)
        {
            if (waypointItems[i] != null)
            {
                waypointItems[i].UpdateIndex(i + 1);
            }
        }
    }

    /// <summary>
    /// 进入路径编辑模式
    /// </summary>
    public void EnterPathEditMode(EnemyWaypointItem editingWaypoint)
    {
        if (mouseManager != null)
        {
            mouseManager.EnterPathEditMode(editingWaypoint);
        }
    }

    /// <summary>
    /// 当路径点数据改变时调用
    /// </summary>
    public void NotifyWaypointChanged()
    {
        if (isLoading) return;  // 加载中不通知
        NotifyWavePanelDataChanged();
    }

    /// <summary>
    /// 清空所有路径点（公共方法）
    /// </summary>
    public void ClearAllWaypoints()
    {
        ClearAllWaypointsImmediate();
        NotifyWavePanelDataChanged();
    }

    /// <summary>
    /// 获取当前波次的所有路径点数据
    /// </summary>
    public List<WaypointData> GetWaypointData()
    {
        List<WaypointData> dataList = new List<WaypointData>();
        foreach (var waypointItem in waypointItems)
        {
            if (waypointItem != null)
            {
                dataList.Add(waypointItem.GetWaypointData());
            }
        }

        // 按序号排序
        dataList.Sort((a, b) => a.index.CompareTo(b.index));
        return dataList;
    }

    /// <summary>
    /// 通知WavePanel数据已改变
    /// </summary>
    void NotifyWavePanelDataChanged()
    {
        if (string.IsNullOrEmpty(currentWaveId)) return;

        WavePanel wavePanel = FindObjectOfType<WavePanel>();
        if (wavePanel != null)
        {
            wavePanel.OnRoutePanelChanged(currentWaveId, GetWaypointData());
        }
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // 清理资源
        ClearAllWaypointsImmediate();
    }
}