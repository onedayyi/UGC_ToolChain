using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 敌人数据库管理器 - 存储所有敌人ScriptableObject
/// </summary>
public class EnemyDatabase : MonoBehaviour
{
    [Header("===== 敌人数据库 =====")]
    [Tooltip("在这里拖拽所有敌人的SO文件")]
    public List<EnemyData> allEnemies = new List<EnemyData>();

    // 运行时字典 - 用于快速查找
    private Dictionary<string, EnemyData> enemyDict;
    private Dictionary<EnemyRank, List<EnemyData>> rankDict;
    private Dictionary<string, List<EnemyData>> typeDict;

    // 单例模式
    public static EnemyDatabase Instance { get; private set; }

    void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 跨场景保留
            BuildDatabases();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 构建所有查找字典
    /// </summary>
    void BuildDatabases()
    {
        enemyDict = new Dictionary<string, EnemyData>();
        rankDict = new Dictionary<EnemyRank, List<EnemyData>>();
        typeDict = new Dictionary<string, List<EnemyData>>();

        // 初始化字典
        foreach (EnemyRank rank in System.Enum.GetValues(typeof(EnemyRank)))
        {
            rankDict[rank] = new List<EnemyData>();
        }

        // 填充数据
        foreach (var enemy in allEnemies)
        {
            if (enemy == null) continue;

            // 按ID索引
            if (!enemyDict.ContainsKey(enemy.enemyId))
            {
                enemyDict.Add(enemy.enemyId, enemy);
            }
            else
            {
                Debug.LogWarning($"重复的敌人ID: {enemy.enemyId} - {enemy.enemyName}");
            }

            // 按地位索引
            if (!rankDict[enemy.rank].Contains(enemy))
            {
                rankDict[enemy.rank].Add(enemy);
            }

            // 按种类索引
            if (!string.IsNullOrEmpty(enemy.enemyType))
            {
                if (!typeDict.ContainsKey(enemy.enemyType))
                {
                    typeDict[enemy.enemyType] = new List<EnemyData>();
                }
                if (!typeDict[enemy.enemyType].Contains(enemy))
                {
                    typeDict[enemy.enemyType].Add(enemy);
                }
            }
        }

        Debug.Log($"✅ 敌人数据库初始化完成\n" +
                  $"  总敌人数量: {enemyDict.Count}\n" +
                  $"  普通敌人: {rankDict[EnemyRank.Normal].Count}\n" +
                  $"  精英敌人: {rankDict[EnemyRank.Elite].Count}\n" +
                  $"  BOSS敌人: {rankDict[EnemyRank.Boss].Count}\n" +
                  $"  种类数量: {typeDict.Count}");
    }

    #region ===== 查询方法 =====
    /// <summary>
    /// 获取所有敌人
    /// </summary>
    public List<EnemyData> GetAllEnemies()
    {
        return allEnemies;
    }
    /// <summary>
    /// 通过ID获取敌人
    /// </summary>
    public EnemyData GetEnemyByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (enemyDict.ContainsKey(id))
            return enemyDict[id];

        Debug.LogWarning($"未找到敌人: {id}");
        return null;
    }

    /// <summary>
    /// 通过名称获取敌人（返回第一个匹配的）
    /// </summary>
    public EnemyData GetEnemyByName(string name)
    {
        return allEnemies.Find(e => e.enemyName == name);
    }

    /// <summary>
    /// 获取所有普通敌人
    /// </summary>
    public List<EnemyData> GetAllNormalEnemies()
    {
        return rankDict[EnemyRank.Normal];
    }

    /// <summary>
    /// 获取所有精英敌人
    /// </summary>
    public List<EnemyData> GetAllEliteEnemies()
    {
        return rankDict[EnemyRank.Elite];
    }

    /// <summary>
    /// 获取所有BOSS敌人
    /// </summary>
    public List<EnemyData> GetAllBossEnemies()
    {
        return rankDict[EnemyRank.Boss];
    }

    /// <summary>
    /// 通过地位获取敌人
    /// </summary>
    public List<EnemyData> GetEnemiesByRank(EnemyRank rank)
    {
        return rankDict.ContainsKey(rank) ? rankDict[rank] : new List<EnemyData>();
    }

    /// <summary>
    /// 通过种类获取敌人
    /// </summary>
    public List<EnemyData> GetEnemiesByType(string enemyType)
    {
        return typeDict.ContainsKey(enemyType) ? typeDict[enemyType] : new List<EnemyData>();
    }

    /// <summary>
    /// 搜索敌人（按名称或ID）
    /// </summary>
    public List<EnemyData> SearchEnemies(string keyword)
    {
        if (string.IsNullOrEmpty(keyword)) return new List<EnemyData>();

        keyword = keyword.ToLower();
        return allEnemies.Where(e =>
            e.enemyName.ToLower().Contains(keyword) ||
            e.enemyId.ToLower().Contains(keyword) ||
            e.enemyType.ToLower().Contains(keyword)
        ).ToList();
    }

    #endregion

    #region ===== 编辑器功能 =====

#if UNITY_EDITOR
    /// <summary>
    /// 刷新数据库（在编辑器中手动调用）
    /// </summary>
    [ContextMenu("刷新数据库")]
    public void RefreshDatabase()
    {
        BuildDatabases();
    }

    /// <summary>
    /// 打印所有敌人信息
    /// </summary>
    [ContextMenu("打印所有敌人")]
    public void PrintAllEnemies()
    {
        Debug.Log("===== 所有敌人列表 =====");
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
            {
                Debug.Log($"[{enemy.rank}] {enemy.enemyId} - {enemy.enemyName} (HP:{enemy.maxHP})");
            }
        }
    }

    /// <summary>
    /// 检查重复ID
    /// </summary>
    [ContextMenu("检查重复ID")]
    public void CheckDuplicateIDs()
    {
        var idSet = new HashSet<string>();
        var duplicates = new List<string>();

        foreach (var enemy in allEnemies)
        {
            if (enemy == null) continue;

            if (idSet.Contains(enemy.enemyId))
            {
                duplicates.Add(enemy.enemyId);
            }
            else
            {
                idSet.Add(enemy.enemyId);
            }
        }

        if (duplicates.Count > 0)
        {
            Debug.LogError($"发现重复的敌人ID: {string.Join(", ", duplicates)}");
        }
        else
        {
            Debug.Log("✅ 没有重复的敌人ID");
        }
    }
#endif

    #endregion
}