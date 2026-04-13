using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemySticker : MonoBehaviour
{
    [Header("UI组件")]
    public Image enemyIcon;           // 敌人图标
    public TMP_Text enemyNameText;     // 敌人名称
    public TMP_Text countText;         // 新增：数量显示文本
    public Button addCountButton;       // 新增：增加数量按钮
    public Button minusCountButton;     // 新增：减少数量按钮

    [Header("数据")]
    public string enemyId;             // 敌人ID
    public string enemyName;           // 敌人名称
    public GameObject enemyPrefab;      // 实际敌人预制体
    public int count = 1;               // 新增：敌人数量

    private CreateConfigurationPanel parentPanel;

    void Start()
    {
        if (addCountButton != null)
            addCountButton.onClick.AddListener(OnAddCount);

        if (minusCountButton != null)
            minusCountButton.onClick.AddListener(OnMinusCount);
    }

    public void Initialize(string id, string name, GameObject prefab, Sprite icon, CreateConfigurationPanel panel = null)
    {
        enemyId = id;
        enemyName = name;
        enemyPrefab = prefab;
        parentPanel = panel;

        if (enemyNameText != null)
            enemyNameText.text = name;

        if (enemyIcon != null && icon != null)
            enemyIcon.sprite = icon;

        UpdateCountDisplay();
    }

    void OnAddCount()
    {
        count++;
        UpdateCountDisplay();
        Debug.Log($"增加数量: {enemyName} 现在有 {count} 个");
        parentPanel?.NotifyWavePanelDataChanged();
    }

    void OnMinusCount()
    {
        if (count > 1)
        {
            count--;
            UpdateCountDisplay();
            Debug.Log($"减少数量: {enemyName} 现在有 {count} 个");
            parentPanel?.NotifyWavePanelDataChanged();
        }
    }

    public void UpdateCountDisplay()
    {
        if (countText != null)
            countText.text = $"x{count}";
    }

    public void SetParentPanel(CreateConfigurationPanel panel)
    {
        parentPanel = panel;
    }
}