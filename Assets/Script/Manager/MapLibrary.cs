using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapLibrary : MonoBehaviour
{
    private static MapLibrary _instance;
    public static MapLibrary Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<MapLibrary>();
            return _instance;
        }
    }

    private string mapFolderPath;
    public List<MapSaveData> availableMaps = new List<MapSaveData>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        mapFolderPath = Application.dataPath + "/Maps/";

        if (!Directory.Exists(mapFolderPath))
            Directory.CreateDirectory(mapFolderPath);

        RefreshMapList();
    }

    public void RefreshMapList()
    {
        availableMaps.Clear();

        string[] files = Directory.GetFiles(mapFolderPath, "*.json");
        foreach (string file in files)
        {
            string json = File.ReadAllText(file);
            MapSaveData mapData = JsonUtility.FromJson<MapSaveData>(json);
            availableMaps.Add(mapData);
        }

        Debug.Log($"找到 {availableMaps.Count} 张地图");
    }

    public void SaveMap(MapData mapData, string mapName)
    {
        MapSaveData saveData = new MapSaveData(mapData, mapName);
        string json = JsonUtility.ToJson(saveData, true);
        string filePath = mapFolderPath + saveData.mapId + ".json";

        File.WriteAllText(filePath, json);
        RefreshMapList();
        Debug.Log($"地图已保存: {mapName}");
    }
    // 加载已有地图
    public MapData LoadMap(string mapId, out string mapName, out string createTime)
    {
        mapName = null;
        createTime = null;

        string filePath = mapFolderPath + mapId + ".json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            MapSaveData saveData = JsonUtility.FromJson<MapSaveData>(json);

            mapName = saveData.mapName;
            createTime = saveData.createTime;

            return saveData.ToMapData();
        }

        Debug.LogError($"地图不存在: {mapId}");
        return null;
    }
    // 重载简化版本（兼容旧代码）
    public MapData LoadMap(string mapId)
    {
        return LoadMap(mapId, out _, out _);
    }
    //删除地图
    public void DeleteMap(string mapId)
    {
        string filePath = mapFolderPath + mapId + ".json";
        if (File.Exists(filePath))
            File.Delete(filePath);

        RefreshMapList();
    }
    // 覆盖保存已有地图
    public void OverwriteMap(MapData mapData, string mapId, string mapName)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogError("无法覆盖：地图ID为空");
            return;
        }

        string filePath = mapFolderPath + mapId + ".json";

        if (File.Exists(filePath))
        {
            // 读取原有数据
            string oldJson = File.ReadAllText(filePath);
            MapSaveData oldData = JsonUtility.FromJson<MapSaveData>(oldJson);

            // 创建新的保存数据，但保留原有ID和创建时间
            MapSaveData saveData = new MapSaveData(mapData, mapName)
            {
                mapId = mapId,  // 使用原有ID
                createTime = oldData.createTime  // 保留创建时间
            };

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, json);

            RefreshMapList();
            Debug.Log($"地图已覆盖保存: {mapName}");
        }
        else
        {
            Debug.LogWarning($"文件不存在，创建新文件: {mapName}");
            SaveMap(mapData, mapName);
        }
    }
}