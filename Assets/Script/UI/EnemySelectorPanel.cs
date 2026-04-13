using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class EnemySelectorPanel : MonoBehaviour
{
    [Header("UI组件")]
    public Transform contentParent;  // Scroll View的Content
    public GameObject enemyAvatarPrefab;  // EnemyAvatar预制体

    [Header("筛选按钮")]
    public Button allButton;
    public Button normalButton;
    public Button eliteButton;
    public Button bossButton;

    [Header("搜索框")]
    public TMP_InputField searchInput;

    [Header("当前选中")]
    public EnemyAvatar currentSelectedAvatar;

    // 事件：当敌人被选中时触发（用于添加到配置面板）
    public event Action<EnemyData> OnEnemySelectedForConfig;

    // 存储所有创建的Avatar
    private List<EnemyAvatar> allAvatars = new List<EnemyAvatar>();

    // 当前筛选状态
    private EnemyRank? currentFilter = null;
    private string currentSearch = "";

    void Start()
    {
        // 初始化数据库引用
        if (EnemyDatabase.Instance == null)
        {
            Debug.LogError("找不到EnemyDatabase，请确保场景中有EnemyDatabase组件");
            return;
        }

        // 加载所有敌人
        LoadAllEnemies();

        // 绑定按钮事件
        if (allButton != null)
            allButton.onClick.AddListener(() => SetFilter(null));

        if (normalButton != null)
            normalButton.onClick.AddListener(() => SetFilter(EnemyRank.Normal));

        if (eliteButton != null)
            eliteButton.onClick.AddListener(() => SetFilter(EnemyRank.Elite));

        if (bossButton != null)
            bossButton.onClick.AddListener(() => SetFilter(EnemyRank.Boss));

        // 绑定搜索事件
        if (searchInput != null)
            searchInput.onValueChanged.AddListener(OnSearchChanged);
    }

    /// <summary>
    /// 加载所有敌人
    /// </summary>
    void LoadAllEnemies()
    {
        // 获取所有敌人
        List<EnemyData> allEnemies = EnemyDatabase.Instance.GetAllEnemies();

        if (allEnemies == null || allEnemies.Count == 0)
        {
            Debug.LogWarning("没有找到任何敌人");
            return;
        }

        // 清空现有内容
        ClearContent();

        // 为每个敌人创建Avatar
        foreach (var enemy in allEnemies)
        {
            CreateEnemyAvatar(enemy);
        }

        Debug.Log($"已加载 {allAvatars.Count} 个敌人");
    }

    /// <summary>
    /// 创建单个敌人Avatar
    /// </summary>
    void CreateEnemyAvatar(EnemyData enemyData)
    {
        if (enemyAvatarPrefab == null || contentParent == null) return;

        GameObject avatarObj = Instantiate(enemyAvatarPrefab, contentParent);
        EnemyAvatar avatar = avatarObj.GetComponent<EnemyAvatar>();

        if (avatar != null)
        {
            avatar.Initialize(enemyData, this);
            allAvatars.Add(avatar);
        }
    }

    /// <summary>
    /// 清空Content
    /// </summary>
    void ClearContent()
    {
        foreach (var avatar in allAvatars)
        {
            if (avatar != null)
                Destroy(avatar.gameObject);
        }
        allAvatars.Clear();
    }

    /// <summary>
    /// 设置筛选
    /// </summary>
    void SetFilter(EnemyRank? rank)
    {
        currentFilter = rank;
        ApplyFilter();
    }

    /// <summary>
    /// 搜索改变
    /// </summary>
    void OnSearchChanged(string searchText)
    {
        currentSearch = searchText.ToLower();
        ApplyFilter();
    }

    /// <summary>
    /// 应用筛选和搜索
    /// </summary>
    void ApplyFilter()
    {
        Debug.Log($"=== 应用筛选 ===");
        Debug.Log($"当前筛选地位: {currentFilter}");
        Debug.Log($"当前搜索词: '{currentSearch}'");

        int visibleCount = 0;
        int totalCount = 0;

        foreach (var avatar in allAvatars)
        {
            if (avatar == null || avatar.enemyData == null) continue;

            totalCount++;
            bool show = true;
            string hideReason = "";

            // 筛选
            if (currentFilter.HasValue)
            {
                if (avatar.enemyData.rank != currentFilter.Value)
                {
                    show = false;
                    hideReason = $"地位不匹配 (需要{currentFilter}, 实际{avatar.enemyData.rank})";
                }
            }

            // 搜索
            if (show && !string.IsNullOrEmpty(currentSearch))
            {
                bool nameMatch = avatar.enemyData.enemyName.ToLower().Contains(currentSearch);
                bool idMatch = avatar.enemyData.enemyId.ToLower().Contains(currentSearch);

                if (!nameMatch && !idMatch)
                {
                    show = false;
                    hideReason = $"搜索词 '{currentSearch}' 不匹配";
                }
            }

            avatar.gameObject.SetActive(show);

            if (show)
            {
                visibleCount++;
                Debug.Log($"✅ 显示: {avatar.enemyData.enemyName}");
            }
            else
            {
                Debug.Log($"❌ 隐藏: {avatar.enemyData.enemyName} - {hideReason}");
            }
        }

        Debug.Log($"筛选结果: {visibleCount}/{totalCount} 个敌人可见");
    }

    /// <summary>
    /// 敌人被选中时调用
    /// </summary>
    public void OnEnemySelected(EnemyAvatar selectedAvatar)
    {
        // 取消之前选中的
        if (currentSelectedAvatar != null && currentSelectedAvatar != selectedAvatar)
        {
            currentSelectedAvatar.SetSelected(false);
        }

        // 设置新的选中
        currentSelectedAvatar = selectedAvatar;
        currentSelectedAvatar.SetSelected(true);

        Debug.Log($"选中敌人: {selectedAvatar.enemyData.enemyName}");

        // 触发事件，将敌人数据传递给CreateConfigurationPanel
        OnEnemySelectedForConfig?.Invoke(selectedAvatar.enemyData);
    }

    /// <summary>
    /// 获取当前选中的敌人数据
    /// </summary>
    public EnemyData GetSelectedEnemy()
    {
        return currentSelectedAvatar != null ? currentSelectedAvatar.enemyData : null;
    }

    /// <summary>
    /// 刷新面板（在数据库更新后调用）
    /// </summary>
    public void RefreshPanel()
    {
        LoadAllEnemies();
        currentSelectedAvatar = null;
    }
}