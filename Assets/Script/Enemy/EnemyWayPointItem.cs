using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyWaypointItem : MonoBehaviour
{
    [Header("UI组件")]
    public TMP_Text indexText;              // 显示序号
    public TMP_Text coordinateText;          // 显示坐标
    public TMP_InputField stayTimeInput;     // 停留时间输入框
    public Button editButton;                 // 编辑按钮
    public Button deleteButton;               // 删除按钮

    [Header("数据")]
    public Vector3 worldPosition;              // 世界坐标
    public float stayTime = 1f;                // 停留时间（默认1，单位0.1秒）
    private int waypointIndex;
    private RouteSettingPanel parentPanel;

    void Start()
    {
        // 绑定按钮事件
        if (editButton != null)
            editButton.onClick.AddListener(OnEditClick);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteClick);

        if (stayTimeInput != null)
            stayTimeInput.onEndEdit.AddListener(OnStayTimeChanged);
    }

    /// <summary>
    /// 初始化路径点
    /// </summary>
    public void Initialize(int index, RouteSettingPanel panel)
    {
        waypointIndex = index;
        parentPanel = panel;

        // 更新UI
        UpdateIndex(index);

        // 设置默认坐标（待编辑）
        UpdatePosition(Vector3.zero);

        // 设置默认停留时间
        if (stayTimeInput != null)
            stayTimeInput.text = stayTime.ToString();
    }

    /// <summary>
    /// 更新序号
    /// </summary>
    public void UpdateIndex(int newIndex)
    {
        waypointIndex = newIndex;
        if (indexText != null)
            indexText.text = $"{newIndex}";
    }

    /// <summary>
    /// 更新坐标
    /// </summary>
    public void UpdatePosition(Vector3 pos)
    {
        worldPosition = pos;

        if (coordinateText != null)
        {
            coordinateText.text = $"({Mathf.RoundToInt(pos.x)}, {Mathf.RoundToInt(pos.y)}, {Mathf.RoundToInt(pos.z)})";
        }

        // ✅ 通知父面板数据已改变
        if (parentPanel != null)
        {
            parentPanel.NotifyWaypointChanged();
            Debug.Log($" 通知 RouteSettingPanel 路径点 {waypointIndex} 位置已更新");
        }
    }
    /// <summary>
    /// 点击编辑按钮
    /// </summary>
    void OnEditClick()
    {
        if (parentPanel != null)
        {
            // 通知RouteSettingPanel进入路径编辑模式
            parentPanel.EnterPathEditMode(this);

            // 可以改变按钮颜色表示正在编辑
            if (editButton != null)
            {
                ColorBlock colors = editButton.colors;
                colors.normalColor = Color.green;
                editButton.colors = colors;
            }
        }
    }

    /// <summary>
    /// 点击删除按钮
    /// </summary>
    void OnDeleteClick()
    {
        if (parentPanel != null)
        {
            parentPanel.RemoveWaypoint(this);
        }
    }

    /// <summary>
    /// 停留时间改变
    /// </summary>
    void OnStayTimeChanged(string value)
    {
        if (float.TryParse(value, out float result))
        {
            stayTime = Mathf.Max(0.1f, result);

            //  通知父面板数据已改变
            if (parentPanel != null)
            {
                parentPanel.NotifyWaypointChanged();
                Debug.Log($" 通知 RouteSettingPanel 路径点 {waypointIndex} 停留时间已更新为 {stayTime}");
            }
        }
        else
        {
            stayTimeInput.text = stayTime.ToString();
        }
    }
    /// <summary>
    /// 设置停留时间
    /// </summary>
    public void SetStayTime(float time)
    {
        stayTime = time;
        if (stayTimeInput != null)
        {
            stayTimeInput.text = time.ToString();
        }
    }
    /// <summary>
    /// 获取路径点数据
    /// </summary>
    public WaypointData GetWaypointData()
    {
        return new WaypointData
        {
            position = worldPosition,
            stayTime = stayTime,
            index = waypointIndex
        };
    }

    /// <summary>
    /// 结束编辑状态（恢复按钮颜色）
    /// </summary>
    public void EndEditMode()
    {
        if (editButton != null)
        {
            ColorBlock colors = editButton.colors;
            colors.normalColor = Color.white;
            editButton.colors = colors;
        }
    }

    /// <summary>
    /// 获取路径点序号
    /// </summary>
    public int GetWaypointIndex()
    {
        return waypointIndex;
    }

}