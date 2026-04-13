using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class MouseManager : MonoBehaviour
{
    [Header("组件引用")]
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public Camera mainCamera;
    public LayerMask tileLayer;
    public LayerMask spawnFloorLayer;           // 出怪点层级

    [Header("编辑器引用")]
    public MapEditor mapEditor;
    public TileSelectorUI tileSelector;
    public WavePanel wavePanel;                  // 引用WavePanel

    [Header("长按设置")]
    public float longPressTime = 0.05f;

    public enum EditMode
    {
        TileEdit,      // 地块编辑模式
        PropertyEdit,  // 属性编辑模式
        PathEdit,      // 路径编辑模式
        SpawnSelect    // 出怪点选择模式
    }

    private EditMode currentMode = EditMode.PropertyEdit;
    private string defaultTileType = "Ground";

    // 路径编辑模式相关变量
    private EnemyWaypointItem currentEditingWaypoint;
    private bool isPathEditing = false;

    // 长按相关变量
    private bool isMouseDown = false;
    private float mouseDownTime = 0f;
    private bool isLongPressing = false;
    private GameObject lastProcessedTile = null;

    // 当前选中的出怪点
    private Spawn_Floor currentSelectedSpawn;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (eventSystem == null)
            eventSystem = FindObjectOfType<EventSystem>();

        if (uiRaycaster == null)
            uiRaycaster = FindObjectOfType<GraphicRaycaster>();

        if (mapEditor == null)
            mapEditor = FindObjectOfType<MapEditor>();

        if (tileSelector == null)
            tileSelector = FindObjectOfType<TileSelectorUI>();

        if (wavePanel == null)
            wavePanel = FindObjectOfType<WavePanel>(true);

        SetMode(EditMode.PropertyEdit);
    }

    void Update()
    {
        // 处理鼠标按下
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            mouseDownTime = Time.time;
            isLongPressing = false;
            lastProcessedTile = null;
        }

        // 处理长按检测（只在TileEdit模式下启用长按）
        if (isMouseDown && !isLongPressing && !IsPointerOverUI() && currentMode == EditMode.TileEdit)
        {
            if (Time.time - mouseDownTime > longPressTime)
            {
                isLongPressing = true;
                //Debug.Log("进入长按批量模式");
                if (mapEditor != null)
                    mapEditor.SetLongPressing(true);
            }
        }

        // 处理鼠标抬起
        if (Input.GetMouseButtonUp(0))
        {
            if (!isLongPressing && isMouseDown)
            {
                HandleLeftClick();
            }

            isMouseDown = false;
            isLongPressing = false;
            lastProcessedTile = null;

            if (mapEditor != null)
                mapEditor.SetLongPressing(false);
        }

        // 处理长按中的批量放置
        if (isLongPressing && Input.GetMouseButton(0) && currentMode == EditMode.TileEdit)
        {
            HandleLongPressDrag();
        }

        // 处理右键点击
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    // 处理单击
    void HandleLeftClick()
    {
        if (IsPointerOverUI())
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            // 首先检查是否点击了出怪点（在属性编辑模式下）
            if (currentMode == EditMode.PropertyEdit)
            {
                Spawn_Floor spawn = hit.collider.GetComponent<Spawn_Floor>();
                if (spawn != null)
                {
                    HandleSpawnClick(spawn);
                    return;
                }
            }

            // 检查是否点击了地块
            if (((1 << hit.collider.gameObject.layer) & tileLayer) != 0)
            {
                TileClickHandler clickHandler = hit.collider.GetComponent<TileClickHandler>();
                if (clickHandler != null)
                {
                    HandleTileClick(clickHandler);
                }
            }
        }
    }

    /// <summary>
    /// 处理出怪点点击
    /// </summary>
    void HandleSpawnClick(Spawn_Floor spawn)
    {
        Debug.Log($"点击出怪点: {spawn.gameObject.name}");

        // 记录当前选中的出怪点
        currentSelectedSpawn = spawn;

        // 调用出怪点的 OnSelected 方法，传入 this 引用
        spawn.OnSelected(this);
    }

    /// <summary>
    /// 出怪点被选中时的回调（由 Spawn_Floor 调用）
    /// </summary>
    public void OnSpawnSelected(Spawn_Floor spawn, WaveData waveData)
    {
        Debug.Log($"MouseManager: 出怪点 {spawn.name} 被选中，转发数据到 WavePanel");

        // 转发数据到 WavePanel
        if (wavePanel != null)
        {
            wavePanel.LoadWaveData(waveData, this);
        }
        else
        {
            Debug.LogError("MouseManager: wavePanel 为 null！");
        }
    }

    /// <summary>
    /// WavePanel 数据更新时的回调
    /// </summary>
    public void OnWaveDataUpdated(WaveData updatedWaveData)
    {
        if (currentSelectedSpawn != null)
        {
            Debug.Log($"MouseManager: 收到更新后的数据，转发回出怪点 {currentSelectedSpawn.name}");
            currentSelectedSpawn.SetWaveData(updatedWaveData);
        }
    }

    /// <summary>
    /// 处理地块点击
    /// </summary>
    void HandleTileClick(TileClickHandler clickHandler)
    {
        Debug.Log($"单击地块 ({clickHandler.tileX},{clickHandler.tileY})");

        switch (currentMode)
        {
            case EditMode.TileEdit:
                Debug.Log("地块编辑模式：修改地块");
                if (mapEditor != null)
                    mapEditor.OnTileLeftClicked(clickHandler.tileX, clickHandler.tileY);
                break;

            case EditMode.PropertyEdit:
                Debug.Log("属性编辑模式：查看属性");
                if (mapEditor != null)
                    mapEditor.ShowTileProperties(clickHandler.tileX, clickHandler.tileY);
                break;

            case EditMode.PathEdit:
                Debug.Log("路径编辑模式：设置路径点位置");
                if (isPathEditing && currentEditingWaypoint != null)
                {
                    Vector3 hitPoint = clickHandler.transform.position;
                    currentEditingWaypoint.UpdatePosition(hitPoint);
                    ExitPathEditMode();
                    Debug.Log($"路径点已设置到: {hitPoint}");
                }
                break;
        }
    }

    // 处理长按拖拽批量放置
    void HandleLongPressDrag()
    {
        if (IsPointerOverUI()) return;

        if (currentMode != EditMode.TileEdit) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, tileLayer))
        {
            TileClickHandler clickHandler = hit.collider.GetComponent<TileClickHandler>();
            if (clickHandler != null)
            {
                GameObject currentTile = hit.collider.gameObject;

                if (currentTile != lastProcessedTile)
                {
                    //Debug.Log($"长按批量放置: ({clickHandler.tileX},{clickHandler.tileY})");
                    if (mapEditor != null)
                        mapEditor.OnTileLeftClicked(clickHandler.tileX, clickHandler.tileY);
                    lastProcessedTile = currentTile;
                }
            }
        }
    }

    // 处理右键点击
    void HandleRightClick()
    {
        // 如果正在路径编辑模式，右键取消编辑
        if (isPathEditing)
        {
            ExitPathEditMode();
            Debug.Log("取消路径点编辑");
            return;
        }

        if (IsPointerOverUI())
        {
            Debug.Log("右键点击UI，不处理地块");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, tileLayer))
        {
            TileClickHandler clickHandler = hit.collider.GetComponent<TileClickHandler>();
            if (clickHandler != null)
            {
                string oldBrush = mapEditor != null ? mapEditor.GetCurrentBrush() : "";
                if (mapEditor != null)
                {
                    mapEditor.SetCurrentBrush(defaultTileType);
                    mapEditor.OnTileLeftClicked(clickHandler.tileX, clickHandler.tileY);
                    mapEditor.SetCurrentBrush(oldBrush);
                }

                Debug.Log($"右键点击地块，修改为默认: {defaultTileType}");
            }
        }
        else
        {
            SetMode(EditMode.PropertyEdit);
            if (mapEditor != null)
                mapEditor.SetCurrentBrush(null);

            if (tileSelector != null)
            {
                tileSelector.ClearSelection();
            }

            // 右键空白处，隐藏SpawnPanel
            if (wavePanel != null)
            {
                wavePanel.HideSpawnPanel();
            }

            // 清除当前选中的出怪点记录
            currentSelectedSpawn = null;

            Debug.Log("右键空白处，隐藏面板并切换到属性编辑模式");
        }
    }

    bool IsPointerOverUI()
    {
        if (uiRaycaster == null || eventSystem == null) return false;

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        uiRaycaster.Raycast(pointerData, results);

        return results.Count > 0;
    }

    /// <summary>
    /// 进入路径编辑模式
    /// </summary>
    public void EnterPathEditMode(EnemyWaypointItem waypoint)
    {
        currentEditingWaypoint = waypoint;
        isPathEditing = true;
        currentMode = EditMode.PathEdit;

        Debug.Log($"进入路径编辑模式，正在编辑路径点 {waypoint.GetWaypointIndex()}");
    }

    /// <summary>
    /// 退出路径编辑模式
    /// </summary>
    void ExitPathEditMode()
    {
        if (currentEditingWaypoint != null)
        {
            currentEditingWaypoint.EndEditMode();
        }

        currentEditingWaypoint = null;
        isPathEditing = false;
        currentMode = EditMode.PropertyEdit;

        Debug.Log("退出路径编辑模式");
    }

    public void SetMode(EditMode mode)
    {
        if (isPathEditing)
        {
            ExitPathEditMode();
        }

        currentMode = mode;

        string modeName = "";
        if (mode == EditMode.TileEdit)
            modeName = "地块编辑模式";
        else if (mode == EditMode.PropertyEdit)
            modeName = "属性编辑模式";
        else if (mode == EditMode.PathEdit)
            modeName = "路径编辑模式";
        else if (mode == EditMode.SpawnSelect)
            modeName = "出怪点选择模式";

        Debug.Log($"切换到: {modeName}");
    }

    public void OnTileTypeSelected(string tileType)
    {
        SetMode(EditMode.TileEdit);
        if (mapEditor != null)
            mapEditor.SetCurrentBrush(tileType);
        //Debug.Log($"选择地块类型: {tileType}，进入地块编辑模式");
    }

    public EditMode GetCurrentMode()
    {
        return currentMode;
    }

    public bool IsPathEditing()
    {
        return isPathEditing;
    }

    /// <summary>
    /// 获取当前选中的出怪点
    /// </summary>
    public Spawn_Floor GetCurrentSelectedSpawn()
    {
        return currentSelectedSpawn;
    }
}