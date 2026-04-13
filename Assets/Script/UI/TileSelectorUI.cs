using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TileSelectorUI : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject buttonPrefab;      // 按钮预制体
    public Transform buttonContainer;    // 按钮容器（放在Content下）
    public ScrollRect scrollRect;        // 滚动视图组件

    [Header("当前选中")]
    public Image currentBrushIcon;       // 当前画笔图标
    public TextMeshProUGUI currentBrushName; // 当前画笔名称

    private MapEditor mapEditor;
    private string currentSelectedTileType = "ground";
    private Dictionary<string, GameObject> typeButtons = new Dictionary<string, GameObject>();

    void Start()
    {
        mapEditor = FindObjectOfType<MapEditor>();
        if (mapEditor == null)
        {
            Debug.LogError("找不到MapEditor！");
            return;
        }

        // 初始化地块选择面板
        InitializeTileButtons();
    }

    void InitializeTileButtons()
    {
        // 【修复】安全删除所有现有的按钮（只删除子物体，不影响预制体）
        ClearExistingButtons();

        // 从数据库获取所有地块类型
        List<TileTypeDefinition> allTypes = TileTypeDatabase.Instance.GetAllTypes();

        Debug.Log($"找到 {allTypes.Count} 种地块类型");

        foreach (var type in allTypes)
        {
            CreateButtonForType(type);
        }

        // 默认选中第一个类型
        if (allTypes.Count > 0)
        {
            OnTileTypeSelected(allTypes[0].id);
        }
    }

    // 【新增】安全清除现有按钮的方法
    void ClearExistingButtons()
    {
        // 检查buttonContainer是否为null
        if (buttonContainer == null)
        {
            Debug.LogError("buttonContainer 未赋值！");
            return;
        }

        // 从后往前遍历删除所有子物体
        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = buttonContainer.GetChild(i);
            // 只删除场景中的实例，不影响预制体
            //Destroy(child.gameObject);
        }

        // 清空字典
        typeButtons.Clear();
    }

    void CreateButtonForType(TileTypeDefinition type)
    {
        // 检查必要组件
        if (buttonPrefab == null)
        {
            Debug.LogError("buttonPrefab 未赋值！");
            return;
        }

        if (buttonContainer == null)
        {
            Debug.LogError("buttonContainer 未赋值！");
            return;
        }

        // 创建按钮实例
        GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
        buttonObj.name = $"Btn_{type.id}";

        // 设置按钮图标颜色
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = type.color;
        }

        // 设置按钮文字
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = type.displayName;
        }

        // 添加点击事件
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            string capturedType = type.id;
            button.onClick.AddListener(() => OnTileTypeSelected(capturedType));
        }

        // 保存引用
        typeButtons[type.id] = buttonObj;

        //Debug.Log($"创建按钮: {type.displayName}");
    }

    void OnTileTypeSelected(string tileTypeId)
    {
        currentSelectedTileType = tileTypeId;

        // 【修改】不直接调用MapEditor，而是调用MouseManager
        MouseManager mouseManager = FindObjectOfType<MouseManager>();
        if (mouseManager != null)
        {
            mouseManager.OnTileTypeSelected(tileTypeId);
        }
        else
        {
            // 兼容旧版
            if (mapEditor != null)
                mapEditor.SetCurrentBrush(tileTypeId);
        }

        // 更新UI显示
        UpdateCurrentBrushDisplay(tileTypeId);
        HighlightSelectedButton(tileTypeId);

        Debug.Log($"选中地块: {tileTypeId}");
    }

    void UpdateCurrentBrushDisplay(string tileTypeId)
    {
        var typeDef = TileTypeDatabase.Instance.GetType(tileTypeId);

        // 更新图标颜色
        if (currentBrushIcon != null)
        {
            currentBrushIcon.color = typeDef.color;
        }

        // 更新名称
        if (currentBrushName != null)
        {
            currentBrushName.text = typeDef.displayName;
        }
    }

    void HighlightSelectedButton(string selectedTypeId)
    {
        foreach (var pair in typeButtons)
        {
            string typeId = pair.Key;
            GameObject btnObj = pair.Value;

            if (btnObj == null) continue;

            Image btnImage = btnObj.GetComponent<Image>();
            if (btnImage != null)
            {
                if (typeId == selectedTypeId)
                {
                    // 选中状态：亮一点
                    //btnImage.color = Color.white;
                }
                else
                {
                    // 未选中：恢复原色
                    var typeDef = TileTypeDatabase.Instance.GetType(typeId);
                    btnImage.color = typeDef.color;
                }
            }
        }
    }

    public string GetCurrentTileType()
    {
        return currentSelectedTileType;
    }

    public void ClearSelection()
    {
        currentSelectedTileType = null;
        if (currentBrushIcon != null)
        {
            currentBrushIcon.color = Color.white;
        }
        if (currentBrushName != null)
        {
            currentBrushName.text = "无";
        }
    }

    // 刷新按钮列表
    public void RefreshTileTypes()
    {
        InitializeTileButtons();
    }
}