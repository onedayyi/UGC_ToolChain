using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Buff数据库 - 存储所有Buff SO文件
/// </summary>
public class BuffDatabase : MonoBehaviour
{
    [Header("===== Buff配置 =====")]
    [Tooltip("拖拽所有Buff SO文件到这里")]
    public List<BuffData> allBuffs = new List<BuffData>();

    // 运行时字典 - 快速查找
    private Dictionary<string, BuffData> buffDict;

    // 按效果类型分类
    private Dictionary<BuffEffectType, List<BuffData>> buffsByType;

    // 单例
    public static BuffDatabase Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 构建数据库
    /// </summary>
    void BuildDatabase()
    {
        buffDict = new Dictionary<string, BuffData>();
        buffsByType = new Dictionary<BuffEffectType, List<BuffData>>();

        // 初始化类型字典
        foreach (BuffEffectType type in System.Enum.GetValues(typeof(BuffEffectType)))
        {
            buffsByType[type] = new List<BuffData>();
        }

        // 填充数据
        foreach (var buff in allBuffs)
        {
            if (buff == null) continue;

            // 检查ID是否有效
            if (string.IsNullOrEmpty(buff.buffId))
            {
                Debug.LogWarning($"Buff {buff.name} 没有设置ID，已跳过");
                continue;
            }

            // 添加到ID字典
            if (!buffDict.ContainsKey(buff.buffId))
            {
                buffDict.Add(buff.buffId, buff);
            }
            else
            {
                Debug.LogWarning($"重复的Buff ID: {buff.buffId}");
            }

            // 添加到类型字典
            if (!buffsByType[buff.effectType].Contains(buff))
            {
                buffsByType[buff.effectType].Add(buff);
            }
        }

        Debug.Log($" Buff数据库初始化完成，共 {buffDict.Count} 个Buff");
    }

    /// <summary>
    /// 通过ID获取Buff
    /// </summary>
    public BuffData GetBuffByID(string buffId)
    {
        if (string.IsNullOrEmpty(buffId)) return null;

        if (buffDict.ContainsKey(buffId))
            return buffDict[buffId];

        Debug.LogWarning($"未找到Buff: {buffId}");
        return null;
    }

    /// <summary>
    /// 获取所有Buff
    /// </summary>
    public List<BuffData> GetAllBuffs()
    {
        return allBuffs;
    }

    /// <summary>
    /// 按类型获取Buff列表
    /// </summary>
    public List<BuffData> GetBuffsByType(BuffEffectType type)
    {
        return buffsByType.ContainsKey(type) ? buffsByType[type] : new List<BuffData>();
    }

    /// <summary>
    /// 搜索Buff
    /// </summary>
    public List<BuffData> SearchBuffs(string keyword)
    {
        if (string.IsNullOrEmpty(keyword)) return new List<BuffData>();

        keyword = keyword.ToLower();
        return allBuffs.Where(b =>
            b.buffName.ToLower().Contains(keyword) ||
            b.buffId.ToLower().Contains(keyword)
        ).ToList();
    }

#if UNITY_EDITOR
    [ContextMenu("刷新数据库")]
    public void RefreshDatabase()
    {
        BuildDatabase();
    }

    [ContextMenu("打印所有Buff")]
    public void PrintAllBuffs()
    {
        Debug.Log("=== 所有Buff列表 ===");
        foreach (var buff in allBuffs)
        {
            Debug.Log($"[{buff.effectType}] {buff.buffId} - {buff.buffName}");
        }
    }
#endif
}