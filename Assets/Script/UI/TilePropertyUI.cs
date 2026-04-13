using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TilePropertyUI : MonoBehaviour
{
    [Header("属性显示")]
    public GameObject propertyPanel;           // 整个属性面板
    public TextMeshProUGUI positionText;       // 位置： (3,5)
    public TextMeshProUGUI typeNameText;       // 类型名称：火焰地板
    public Image typeColorImage;               // 类型颜色图标

    [Header("基础属性")]
    public Toggle canWalkToggle;                // 可通行
    public Toggle canPlaceToggle;               // 可部署
    public TextMeshProUGUI heightText;          // 高度

    [Header("特殊效果")]
    public GameObject effectPanel;              // 效果面板
    public TextMeshProUGUI effectNameText;      // 效果名称：寒冷效果
    public TextMeshProUGUI effectDescriptionText; // 效果描述：站在上面的单位减速30%
    public Slider effectSlider;                 // 效果强度
    public TextMeshProUGUI effectValueText;     // 效果数值

    [Header("编辑按钮")]
    public Button applyButton;                   // 应用修改按钮
    public Button cancelButton;                  // 取消修改按钮

    private MapEditor mapEditor;
    private TileData currentTileData;

    void Start()
    {
        mapEditor = FindObjectOfType<MapEditor>();

        // 初始隐藏属性面板
        propertyPanel.SetActive(false);

        // 绑定按钮事件
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplyChanges);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelChanges);
    }

    // 显示格子的属性（由MapEditor调用）
    public void ShowTileProperties(TileData tileData)
    {
        if (tileData == null) return;

        currentTileData = tileData;
        propertyPanel.SetActive(true);

        // 获取类型定义
        TileTypeDefinition typeDef = TileTypeDatabase.Instance.GetType(tileData.Type);

        // 1. 基本信息
        positionText.text = $"位置: ({tileData.X}, {tileData.Y})";
        typeNameText.text = typeDef.displayName;
        typeColorImage.color = typeDef.color;

        // 2. 基础属性（只读显示）
        canWalkToggle.isOn = typeDef.canWalk;
        canPlaceToggle.isOn = typeDef.canPlace;
        heightText.text = typeDef.height.ToString();

        // 3. 检查是否有特殊效果
        CheckSpecialEffects(tileData);

        Debug.Log($"显示格子属性: ({tileData.X},{tileData.Y}) - {typeDef.displayName}");
    }

    // 检查特殊效果
    void CheckSpecialEffects(TileData tileData)
    {
        // 根据地块类型显示不同的效果
        switch (tileData.Type)
        {
            case "fire":
                ShowEffect("火焰效果", "站在上面的单位每秒受到10点伤害", 10f);
                break;

            case "ice":
                ShowEffect("寒冷效果", "站在上面的单位移动速度降低30%", 0.3f);
                break;

            case "poison":
                ShowEffect("中毒效果", "站在上面的单位每秒中毒，持续3秒", 5f);
                break;

            case "heal":
                ShowEffect("治疗效果", "站在上面的单位每秒恢复5点生命", 5f);
                break;

            default:
                // 没有特殊效果，隐藏效果面板
                effectPanel.SetActive(false);
                break;
        }
    }

    void ShowEffect(string name, string description, float value)
    {
        effectPanel.SetActive(true);
        effectNameText.text = name;
        effectDescriptionText.text = description;
        effectSlider.value = value;
        effectValueText.text = value.ToString("F1");
    }

    // 进入编辑模式
    public void EnterEditMode()
    {
        // 让Toggle可编辑
        canWalkToggle.interactable = true;
        canPlaceToggle.interactable = true;

        // 显示应用/取消按钮
        applyButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
    }

    // 应用修改
    void ApplyChanges()
    {
        if (currentTileData == null) return;

        // 这里可以实现自定义属性覆盖
        // 比如给格子添加自定义的可通行设置
        currentTileData.SetProperty("customWalkable", canWalkToggle.isOn);
        currentTileData.SetProperty("customPlaceable", canPlaceToggle.isOn);

        Debug.Log($"应用修改到格子 ({currentTileData.X},{currentTileData.Y})");

        // 退出编辑模式
        ExitEditMode();
    }

    // 取消修改
    void CancelChanges()
    {
        // 重新显示原始属性
        if (currentTileData != null)
        {
            ShowTileProperties(currentTileData);
        }

        ExitEditMode();
    }

    void ExitEditMode()
    {
        canWalkToggle.interactable = false;
        canPlaceToggle.interactable = false;
        applyButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }

    // 隐藏属性面板
    public void HideProperties()
    {
        propertyPanel.SetActive(false);
        currentTileData = null;
    }
}