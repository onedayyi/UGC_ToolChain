using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CreateConfigurationPanel : MonoBehaviour
{
    [Header("UI组件")]
    // 移除 addEnemyButton
    public Button deleteLastButton;             // 删除最后一个按钮
    public TMP_InputField spawnIntervalInput;   // 出现间隔输入框
    public Transform stickerContainer;           // 贴纸容器（放所有EnemySticker）

    [Header("预制体")]
    public GameObject enemyStickerPrefab;        // 敌人贴纸预制体

    [Header("当前波次信息")]
    public string currentWaveId;                  // 当前编辑的波次ID

    private List<EnemySticker> stickers = new List<EnemySticker>(); // 已创建的贴纸列表

    // 引用敌人选择面板
    private EnemySelectorPanel enemySelectorPanel;

    void Start()
    {
        // 移除 addEnemyButton 的绑定
        if (deleteLastButton != null)
            deleteLastButton.onClick.AddListener(OnDeleteLastClick);

        // 设置默认间隔
        if (spawnIntervalInput != null)
            spawnIntervalInput.text = "1.0";

        // 查找敌人选择面板
        enemySelectorPanel = FindObjectOfType<EnemySelectorPanel>(true);
        if (enemySelectorPanel != null)
        {
            // 订阅敌人选择事件
            enemySelectorPanel.OnEnemySelectedForConfig += OnEnemySelectedFromPanel;
        }
        else
        {
            Debug.LogError("找不到 EnemySelectorPanel！");
        }
    }

    void OnDestroy()
    {
        // 取消订阅
        if (enemySelectorPanel != null)
        {
            enemySelectorPanel.OnEnemySelectedForConfig -= OnEnemySelectedFromPanel;
        }
    }

    /// <summary>
    /// 当从敌人选择面板选中敌人时调用
    /// </summary>
    private void OnEnemySelectedFromPanel(EnemyData enemyData)
    {
        if (enemyData == null) return;

        Debug.Log($"从选择面板添加敌人: {enemyData.enemyName}");

        // 创建贴纸
        CreateSticker(enemyData);
    }

    /// <summary>
    /// 创建贴纸（现在接收EnemyData）
    /// </summary>
    void CreateSticker(EnemyData enemyData)
    {
        if (enemyStickerPrefab == null || stickerContainer == null) return;

        GameObject stickerObj = Instantiate(enemyStickerPrefab, stickerContainer);

        // 重置Transform确保正确显示
        RectTransform rect = stickerObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.anchoredPosition = Vector2.zero;
        }

        // 初始化贴纸数据
        EnemySticker sticker = stickerObj.GetComponent<EnemySticker>();
        if (sticker != null)
        {
            // 传入敌人数据
            sticker.Initialize(
                enemyData.enemyId,
                enemyData.enemyName,
                enemyData.enemyPrefab,
                enemyData.enemyIcon,
                this
            );
            stickers.Add(sticker);

            Debug.Log($"贴纸创建成功，当前贴纸数量: {stickers.Count}");

            // 简单的生成动画
            StartCoroutine(PlaySpawnAnimation(stickerObj));

            // 通知WavePanel数据已更新
            NotifyWavePanelDataChanged();
        }
        else
        {
            Debug.LogError("贴纸预制体上没有 EnemySticker 组件！");
        }
    }

    /// <summary>
    /// 点击删除最后一个按钮 - 删除最后添加的敌人贴纸
    /// </summary>
    void OnDeleteLastClick()
    {
        if (stickers.Count > 0)
        {
            EnemySticker lastSticker = stickers[stickers.Count - 1];
            StartCoroutine(PlayDeleteAnimation(lastSticker.gameObject));

            // 通知WavePanel数据已更新
            NotifyWavePanelDataChanged();
        }
    }

    /// <summary>
    /// 加载指定波次的敌人配置数据
    /// </summary>
    public void LoadWaveData(string waveId, List<EnemyConfig> enemyConfigs)
    {
        Debug.Log($"加载波次 {waveId} 的敌人数据，共 {enemyConfigs?.Count} 个敌人");

        currentWaveId = waveId;

        // 清空当前所有贴纸
        ClearAllStickers();

        if (enemyConfigs == null || enemyConfigs.Count == 0)
        {
            Debug.Log("没有敌人数据");
            return;
        }

        // 根据保存的配置重新创建贴纸
        foreach (var config in enemyConfigs)
        {
            // 从数据库获取完整的敌人数据
            EnemyData enemyData = EnemyDatabase.Instance?.GetEnemyByID(config.enemyId);

            GameObject stickerObj = Instantiate(enemyStickerPrefab, stickerContainer);
            EnemySticker sticker = stickerObj.GetComponent<EnemySticker>();

            if (sticker != null)
            {
                if (enemyData != null)
                {
                    sticker.Initialize(
                        enemyData.enemyId,
                        enemyData.enemyName,
                        enemyData.enemyPrefab,
                        enemyData.enemyIcon,
                        this
                    );
                }
                else
                {
                    sticker.Initialize(config.enemyId, config.enemyName, null, null, this);
                }

                sticker.count = config.count;
                sticker.UpdateCountDisplay();
                stickers.Add(sticker);
            }
        }
    }

    /// <summary>
    /// 获取当前敌人配置（用于保存）
    /// </summary>
    public List<EnemyConfig> GetEnemyConfigs()
    {
        List<EnemyConfig> configs = new List<EnemyConfig>();
        foreach (var sticker in stickers)
        {
            configs.Add(new EnemyConfig
            {
                enemyId = sticker.enemyId,
                enemyName = sticker.enemyName,
                enemyPrefab = sticker.enemyPrefab,
                count = sticker.count
            });
        }
        return configs;
    }

    /// <summary>
    /// 通知WavePanel数据已改变
    /// </summary>
    public void NotifyWavePanelDataChanged()
    {
        if (string.IsNullOrEmpty(currentWaveId))
        {
            Debug.LogWarning("currentWaveId 为空，无法通知 WavePanel");
            return;
        }

        WavePanel wavePanel = FindObjectOfType<WavePanel>();
        if (wavePanel != null)
        {
            List<EnemyConfig> configs = GetEnemyConfigs();
            Debug.Log($"通知WavePanel数据改变：波次 {currentWaveId}，敌人数量 {configs.Count}");
            wavePanel.OnConfigPanelChanged(currentWaveId, configs);
        }
        else
        {
            Debug.LogError("找不到 WavePanel！");
        }
    }

    /// <summary>
    /// 清空所有贴纸
    /// </summary>
    public void ClearAllStickers()
    {
        foreach (var sticker in stickers)
        {
            if (sticker != null && sticker.gameObject != null)
                Destroy(sticker.gameObject);
        }
        stickers.Clear();
    }

    /// <summary>
    /// 生成动画 - 从小变大
    /// </summary>
    System.Collections.IEnumerator PlaySpawnAnimation(GameObject obj)
    {
        obj.transform.localScale = Vector3.zero;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.localScale = Vector3.one * t;
            yield return null;
        }

        obj.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 删除动画 - 从大变小然后销毁
    /// </summary>
    System.Collections.IEnumerator PlayDeleteAnimation(GameObject obj)
    {
        // 从列表中移除
        EnemySticker sticker = obj.GetComponent<EnemySticker>();
        if (sticker != null && stickers.Contains(sticker))
            stickers.Remove(sticker);

        // 缩小动画
        float duration = 0.15f;
        float elapsed = 0f;
        Vector3 originalScale = obj.transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.localScale = originalScale * (1 - t);
            yield return null;
        }

        Destroy(obj);

        // 通知数据改变
        NotifyWavePanelDataChanged();
    }
}