using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class MapSelectorUI : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject mapButtonPrefab;     // 地图按钮预制体
    public Transform mapListContainer;     // 地图列表容器
    public GameObject mapSelectorPanel;    // 整个选择面板

    [Header("输入框")]
    public TMP_InputField newMapNameInput; // 新地图名称输入框

    [Header("引用")]
    public MapEditor mapEditor;
    public MapLibrary mapLibrary;

    private List<GameObject> mapButtons = new List<GameObject>();

    void Start()
    {
        if (mapLibrary == null)
            mapLibrary = MapLibrary.Instance;

        if (mapEditor == null)
            mapEditor = FindObjectOfType<MapEditor>();

        RefreshMapList();
    }

    // 刷新地图列表UI
    public void RefreshMapList()
    {
        // 清除旧按钮
        foreach (var btn in mapButtons)
            Destroy(btn);
        mapButtons.Clear();

        // 【关键】每次刷新都重新排序，确保顺序正确
        var sortedMaps = mapLibrary.availableMaps
            .OrderBy(m => DateTime.Parse(m.createTime))  // 按创建时间排序
            .ToList();
        Debug.Log($"刷新列表，共 {sortedMaps.Count} 张地图");
        // 打印顺序以便调试
        for (int i = 0; i < sortedMaps.Count; i++)
        {
            Debug.Log($"{i}: {sortedMaps[i].mapName} - {sortedMaps[i].createTime}");
        }

        // 创建新按钮
        foreach (var mapData in sortedMaps)
        {
            CreateMapButton(mapData);
        }

    }

    void CreateMapButton(MapSaveData mapData)
    {
        GameObject btnObj = Instantiate(mapButtonPrefab, mapListContainer);

        // 设置按钮文字
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = $"{mapData.mapName}\n{mapData.lastModified:yyyy-MM-dd}";
        }

        // 添加点击事件
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => OnMapSelected(mapData));

        // 添加删除按钮（可选）
        Button deleteBtn = btnObj.transform.Find("DeleteButton")?.GetComponent<Button>();
        if (deleteBtn != null)
        {
            deleteBtn.onClick.AddListener(() => OnMapDeleted(mapData));
        }

        mapButtons.Add(btnObj);
    }

    void OnMapSelected(MapSaveData mapData)
    {
        Debug.Log($"选择地图: {mapData.mapName}");

        // 加载地图数据
        MapData loadedMap = mapLibrary.LoadMap(mapData.mapId, out string mapName, out string createTime);

        // 让MapEditor重新绘制
        mapEditor.RebuildMap(loadedMap);

        if (loadedMap != null)
        {
            // 让MapEditor重新绘制，并传入地图ID和名称
            mapEditor.RebuildMap(loadedMap, mapData.mapId, mapData.mapName);

            // 在输入框中显示当前地图名称（方便覆盖保存）
            if (newMapNameInput != null)
            {
                newMapNameInput.text = mapData.mapName;
            }
        }
    }

    void OnMapDeleted(MapSaveData mapData)
    {
        mapLibrary.DeleteMap(mapData.mapId);
        RefreshMapList();
    }

    // 新建地图
    public void OnCreateNewMap()
    {
        string mapName = newMapNameInput.text;
        if (string.IsNullOrEmpty(mapName))
            mapName = "新地图";

        // 创建新地图数据
        MapData newMap = new MapData(20, 20);  // 默认大小

        // 保存
        mapLibrary.SaveMap(newMap, mapName);

        // 清空当前地图ID（因为是新地图，还没加载到编辑器）
        mapEditor.SetCurrentMapInfo(null, null);

        // 【修复】清空输入框
        newMapNameInput.text = "";

        // 刷新列表
        RefreshMapList();
    }

    // 保存当前地图
    public void OnSaveCurrentMap()
    {
        if (mapEditor == null || mapLibrary == null) return;

        string mapName = newMapNameInput.text;
        if (string.IsNullOrEmpty(mapName))
            mapName = $"地图_{DateTime.Now:yyyyMMdd_HHmmss}";

        string currentMapId = mapEditor.GetCurrentMapId();

        if (string.IsNullOrEmpty(currentMapId))
        {
            // 没有当前地图ID（比如新创建的地图还没保存过），创建新文件
            Debug.Log("当前地图没有ID，创建新文件");
            mapLibrary.SaveMap(mapEditor.GetMapData(), mapName);

            // 获取刚保存的地图ID（需要在SaveMap后返回）
            // 简化起见，这里刷新列表后让用户手动点击
        }
        else
        {
            // 有当前地图ID，覆盖保存
            Debug.Log($"覆盖保存地图: {currentMapId}");
            mapLibrary.OverwriteMap(mapEditor.GetMapData(), currentMapId, mapName);

            // 更新MapEditor中的名称（可能已修改）
            mapEditor.SetCurrentMapInfo(currentMapId, mapName);
        }

        // 清空输入框
        newMapNameInput.text = "";

        // 刷新列表
        RefreshMapList();
    }
}
