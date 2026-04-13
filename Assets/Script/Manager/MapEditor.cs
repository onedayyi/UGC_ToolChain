using UnityEngine;

public class MapEditor : MonoBehaviour
{
    [Header("预制体设置")]
    public GameObject tilePrefab;

    [Header("UI管理器")]
    public TileSelectorUI tileSelector;    // 左上角选择器
    public TilePropertyUI tileProperty;    // 右上角属性面板

    [Header("地图设置")]
    public int mapWidth = 5;
    public int mapHeight = 5;
    public float tileSize = 1.0f;

    private MapData mapData;
    public TileComponentModify floorData;
    private GameObject[,] tileObjects;
    private string currentBrushType = null;  // 当前画笔，null表示无
    private bool isLongPressing = false;    // 长按相关

    private string currentMapId;  // 当前正在编辑的地图ID
    private string currentMapName; // 当前地图名称
    void Start()
    {
        // 确保 floorData 不为空
        if (floorData == null)
            floorData = TileComponentModify.Instance;

        InitializeMap();
    }

    void Update()
    {
        // MouseManager 会处理所有输入，这里可以为空
    }

    void InitializeMap()
    {
        // 创建地图数据
        mapData = new MapData(mapWidth, mapHeight);

        // 创建可视化格子
        CreateVisualTiles();
    }

    void CreateVisualTiles()
    {
        tileObjects = new GameObject[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // 获取数据
                TileData tileData = mapData.GetTile(x, y);

                // 创建格子
                Vector3 worldPos = new Vector3(x * tileSize, 0, y * tileSize);
                GameObject tile = Instantiate(tilePrefab, worldPos, Quaternion.identity);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.parent = this.transform;

                // 添加视觉组件
                TileVisual visual = tile.AddComponent<TileVisual>();
                visual.tileData = tileData;

                // 添加点击处理
                TileClickHandler clickHandler = tile.AddComponent<TileClickHandler>();
                clickHandler.Initialize(this, x, y);

                // 更新外观
                visual.UpdateAppearance();

                tileObjects[x, y] = tile;
            }
        }
    }

    // 左键点击：放置当前画笔的地块
    public void OnTileLeftClicked(int x, int y)
    {
        // 安全检查
        if (mapData == null) return;
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) return;
        if (string.IsNullOrEmpty(currentBrushType)) return;

        GameObject clickedTile = tileObjects[x, y];

        // 获取旧类型并移除旧效果
        TileData oldData = mapData.GetTile(x, y);
        if (oldData != null && oldData.Type != currentBrushType)
        {
            floorData.RemoveFloorBuff(oldData.Type, clickedTile);
        }

        // 修改数据层
        mapData.SetTileType(x, y, currentBrushType);

        // 更新表现层
        TileData newData = mapData.GetTile(x, y);
        TileVisual visual = clickedTile.GetComponent<TileVisual>();
        visual.tileData = newData;
        visual.UpdateAppearance();

        // 添加新效果
        floorData.SetFloor(currentBrushType, clickedTile);

        //Debug.Log($"放置地块: ({x},{y}) -> {currentBrushType}");

        // 根据是否长按决定是否显示属性
        if (!isLongPressing)
        {
            ShowTileProperties(x, y);
        }
    }

    // 右键点击：清空当前画笔并显示属性
    public void OnTileRightClicked(int x, int y)
    {
        // 清空当前画笔
        currentBrushType = null;
        if (tileSelector != null)
        {
            tileSelector.ClearSelection();
        }

        // 显示属性
        ShowTileProperties(x, y);

        Debug.Log("清空画笔，显示格子属性");
    }

    // 显示格子属性
    public void ShowTileProperties(int x, int y)  // 改为public供外部调用
    {
        if (tileProperty == null) return;

        TileData tileData = mapData.GetTile(x, y);
        tileProperty.ShowTileProperties(tileData);
    }

    // 设置当前画笔（由UI调用）
    public void SetCurrentBrush(string brushType)
    {
        currentBrushType = brushType;
        Debug.Log($"设置画笔: {brushType}");
    }

    // 获取当前画笔
    public string GetCurrentBrush()
    {
        return currentBrushType;
    }

    // 【新增】设置长按状态（由MouseManager调用）
    public void SetLongPressing(bool pressing)
    {
        isLongPressing = pressing;
    }

    // 【新增】清除选中地块（由MouseManager调用）
    public void ClearSelectedTile()
    {
        // 可以在这里添加清除选中高亮的逻辑
        Debug.Log("清除选中地块");
    }

    // 获取tileObjects（供NavMeshBaker调用）
    public GameObject[,] GetTileObjects()
    {
        return tileObjects;
    }

    // 获取mapData（供NavMeshBaker调用）
    public MapData GetMapData()
    {
        return mapData;
    }

    // 重建地图
    public void RebuildMap(MapData newMapData, string mapId = null, string mapName = null)
    {
        if (newMapData == null) return;

        // 记录当前地图信息
        currentMapId = mapId;
        currentMapName = mapName;

        // 替换数据
        mapData = newMapData;

        // 销毁旧格子
        if (tileObjects != null)
        {
            for (int x = 0; x < tileObjects.GetLength(0); x++)
            {
                for (int y = 0; y < tileObjects.GetLength(1); y++)
                {
                    if (tileObjects[x, y] != null)
                        Destroy(tileObjects[x, y]);
                }
            }
        }

        // 更新地图尺寸
        mapWidth = mapData.width;
        mapHeight = mapData.height;

        // 重新创建
        CreateVisualTiles();

        Debug.Log("地图重建完成");
    }
    // 获取当前地图ID
    public string GetCurrentMapId()
    {
        return currentMapId;
    }

    // 获取当前地图名称
    public string GetCurrentMapName()
    {
        return currentMapName;
    }

    // 设置当前地图信息（用于新地图）
    public void SetCurrentMapInfo(string mapId, string mapName)
    {
        currentMapId = mapId;
        currentMapName = mapName;
    }
}