using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;
using Unity.AI.Navigation;

public class NavMeshBaker : MonoBehaviour
{
    [Header("组件引用")]
    public NavMeshSurface navMeshSurface;  // NavMeshSurface组件
    public MapEditor mapEditor;            // 地图编辑器，用来获取TileData

    [Header("UI按钮")]
    public Button bakeButton;              // 烘焙按钮

    [Header("UI提示")]
    public Text statusText;                 // 状态提示文本
    public Slider progressSlider;           // 进度条（可选）

    [Header("设置")]
    public float bakeDelay = 0.1f;          // 烘焙延迟
    public bool showDebugLog = true;

    void Start()
    {
        // 自动查找组件
        if (navMeshSurface == null)
            navMeshSurface = FindObjectOfType<NavMeshSurface>();

        if (mapEditor == null)
            mapEditor = FindObjectOfType<MapEditor>();

        if (bakeButton == null)
            bakeButton = GetComponent<Button>();

        // 绑定按钮事件
        if (bakeButton != null)
        {
            bakeButton.onClick.AddListener(OnBakeButtonClicked);
        }
        else
        {
            Debug.LogError("请指定烘焙按钮！");
        }

        if (progressSlider != null)
            progressSlider.gameObject.SetActive(false);
    }

    // 按钮点击时触发
    void OnBakeButtonClicked()
    {
        StartCoroutine(BakeNavMeshCoroutine());
    }

    IEnumerator BakeNavMeshCoroutine()
    {
        UpdateStatus("开始处理地块...", Color.yellow);

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = 0;
        }

        // 1. 先遍历所有地块，设置NavMesh组件
        yield return StartCoroutine(SetupAllTilesNavMesh());

        // 2. 延迟一下，确保组件都设置好了
        yield return new WaitForSeconds(bakeDelay);

        // 3. 烘焙NavMesh
        UpdateStatus("烘焙 NavMesh...", Color.yellow);

        if (progressSlider != null)
        {
            float fakeProgress = 0;
            while (fakeProgress < 0.9f)
            {
                fakeProgress += Time.deltaTime * 0.5f;
                progressSlider.value = fakeProgress;
                yield return null;
            }
        }

        // 实际烘焙
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();

            if (showDebugLog) Debug.Log("NavMesh 烘焙完成");

            if (progressSlider != null)
            {
                progressSlider.value = 1;
                yield return new WaitForSeconds(0.3f);
                progressSlider.gameObject.SetActive(false);
            }

            UpdateStatus("烘焙完成！", Color.green);
        }
        else
        {
            UpdateStatus("错误：找不到 NavMeshSurface！", Color.red);
        }

        // 2秒后清除状态
        yield return new WaitForSeconds(2f);
        ClearStatus();
    }

    // 遍历所有地块，根据canWalk设置NavMesh组件
    IEnumerator SetupAllTilesNavMesh()
    {
        if (mapEditor == null || mapEditor.GetMapData() == null)
        {
            Debug.LogError("无法获取地图数据");
            yield break;
        }

        MapData mapData = mapEditor.GetMapData();
        GameObject[,] tileObjects = mapEditor.GetTileObjects();

        if (tileObjects == null)
        {
            Debug.LogError("tileObjects 为 null");
            yield break;
        }

        int totalTiles = mapData.width * mapData.height;
        int processedTiles = 0;

        UpdateStatus($"正在处理 {totalTiles} 个地块...", Color.yellow);

        // 遍历所有地块
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                TileData tileData = mapData.GetTile(x, y);
                GameObject tileObj = tileObjects[x, y];

                if (tileData != null && tileObj != null)
                {
                    // 根据canWalk设置组件
                    SetupTileNavMesh(tileObj, tileData.CanWalk());
                }

                processedTiles++;

                // 更新进度条
                if (progressSlider != null)
                {
                    float progress = (float)processedTiles / totalTiles;
                    progressSlider.value = progress * 0.5f; // 预留50%给烘焙
                }

                // 每处理10个地块，等待一帧，避免卡顿
                if (processedTiles % 10 == 0)
                {
                    yield return null;
                }
            }
        }

        UpdateStatus("地块处理完成，准备烘焙...", Color.yellow);
        yield return null;
    }

    // 设置单个地块的NavMesh组件
    void SetupTileNavMesh(GameObject tile, bool canWalk)
    {
        if (tile == null) return;

        // 移除现有的NavMesh相关组件
        NavMeshModifier modifier = tile.GetComponent<NavMeshModifier>();
        if (modifier != null) Object.DestroyImmediate(modifier);

        NavMeshObstacle obstacle = tile.GetComponent<NavMeshObstacle>();
        if (obstacle != null) Object.DestroyImmediate(obstacle);

        if (canWalk)
        {
            // 可通行：添加NavMeshModifier
            modifier = tile.AddComponent<NavMeshModifier>();
            modifier.area = 0;  // Walkable
            modifier.overrideArea = true;
            modifier.ignoreFromBuild = false;
            tile.isStatic = true;

            //if (showDebugLog) Debug.Log($"地块 {tile.name} 设置为可通行");
        }
        else
        {
            // 不可通行：添加NavMeshObstacle
            obstacle = tile.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;

            // 根据Collider设置障碍物大小
            BoxCollider box = tile.GetComponent<BoxCollider>();
            if (box != null)
            {
                obstacle.size = box.size;
                obstacle.center = box.center;
            }
            else
            {
                obstacle.size = Vector3.one;
                obstacle.center = Vector3.zero;
            }

            tile.isStatic = false;

            if (showDebugLog) Debug.Log($"地块 {tile.name} 设置为障碍物");
        }
    }

    void UpdateStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }

        if (showDebugLog) Debug.Log(message);
    }

    void ClearStatus()
    {
        if (statusText != null)
        {
            statusText.text = "";
        }
    }
}