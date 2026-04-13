using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EnemyAvatar : MonoBehaviour, IPointerClickHandler
{
    [Header("UI组件")]
    public Image enemyIcon;
    public TMP_Text enemyNameText;
    public TMP_Text enemyRankText;
    public Image rankBackground;

    [Header("状态")]
    public EnemyData enemyData;  // 存储对应的敌人数据
    public bool isSelected = false;

    [Header("颜色配置")]
    public Color normalColor = Color.white;
    public Color eliteColor = Color.yellow;
    public Color bossColor = Color.red;
    public Color selectedColor = new Color(0.2f, 0.6f, 1f, 0.5f);

    private Image backgroundImage;
    private EnemySelectorPanel parentPanel;

    void Awake()
    {
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = gameObject.AddComponent<Image>();
            backgroundImage.color = new Color(1, 1, 1, 0.1f);
        }
    }

    /// <summary>
    /// 初始化Avatar
    /// </summary>
    public void Initialize(EnemyData data, EnemySelectorPanel panel)
    {
        enemyData = data;
        parentPanel = panel;

        // 设置图标和名称
        if (enemyIcon != null && data.enemyIcon != null)
            enemyIcon.sprite = data.enemyIcon;

        if (enemyNameText != null)
            enemyNameText.text = data.enemyName;

        // 根据地位设置颜色和文本
        if (enemyRankText != null)
        {
            switch (data.rank)
            {
                case EnemyRank.Normal:
                    enemyRankText.text = "普通";
                    if (rankBackground != null)
                        rankBackground.color = normalColor;
                    break;
                case EnemyRank.Elite:
                    enemyRankText.text = "精英";
                    if (rankBackground != null)
                        rankBackground.color = eliteColor;
                    break;
                case EnemyRank.Boss:
                    enemyRankText.text = "BOSS";
                    if (rankBackground != null)
                        rankBackground.color = bossColor;
                    break;
            }
        }
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (backgroundImage != null)
        {
            if (selected)
            {
                backgroundImage.color = selectedColor;
            }
            else
            {
                backgroundImage.color = new Color(1, 1, 1, 0.1f);
            }
        }
    }

    /// <summary>
    /// 点击事件
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (parentPanel != null)
        {
            parentPanel.OnEnemySelected(this);
        }
    }
}