using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 运行时Buff实例
/// </summary>
[System.Serializable]
public class ActiveBuff
{
    public BuffData data;           // SO数据
    public GameObject source;        // 来源（哪个地块施加的）
    public float remainingTime;      // 剩余时间
    public float timer;              // 触发计时器
    public int currentStack = 1;            // 当前层数
    public BuffDurationType durationType;   //持续时间类型
    public bool isApplied;                  // 是否已应用（用于一次性效果）
}

public class BuffManager : MonoBehaviour
{
    [Header("===== 所属单位 =====")]
    public Enemy enemyComponent;

    // 当前激活的Buff列表
    public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    // 按来源分类的Buff
    private Dictionary<GameObject, List<ActiveBuff>> buffsBySource = new Dictionary<GameObject, List<ActiveBuff>>();

    // 属性缓存
    private float cachedSpeedMultiplier = 1f;

    // 存储当前激活的特效实例（key: buffId）
    private Dictionary<string, GameObject> activeEffects = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (enemyComponent == null)
            enemyComponent = GetComponent<Enemy>();
    }

    void OnEnable()
    {
        GameEvents.OnTileBuffEnter += OnBuffEnter;
        GameEvents.OnTileBuffExit += OnBuffExit;
    }

    void OnDisable()
    {
        GameEvents.OnTileBuffEnter -= OnBuffEnter;
        GameEvents.OnTileBuffExit -= OnBuffExit;
    }

    void OnBuffEnter(string buffId, GameObject target, GameObject source, float duration)
    {
        if (target != gameObject) return;

        BuffData buffData = BuffDatabase.Instance?.GetBuffByID(buffId);
        if (buffData == null)
        {
            Debug.LogError($"找不到Buff数据: {buffId}");
            return;
        }

        // 统一使用 AddBuff 方法
        AddBuff(buffData, source, duration);
    }

    void OnBuffExit(string buffId, GameObject target, GameObject source)
    {
        if (target != gameObject || source == null) return;
        RemoveBuff(buffId, source);
    }

    void Update()
    {
        // 倒序遍历更新
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = activeBuffs[i];

            if (buff?.data == null)
            {
                activeBuffs.RemoveAt(i);
                continue;
            }

            // 处理持续时间
            if (buff.durationType == BuffDurationType.Timed)
            {
                buff.remainingTime -= Time.deltaTime;

                // 持续伤害效果
                if (buff.data.effectType == BuffEffectType.Damage)
                {
                    buff.timer += Time.deltaTime;
                    if (buff.timer >= buff.data.tickInterval)
                    {
                        ApplyDamage(buff);
                        buff.timer = 0f;
                    }
                }

                if (buff.remainingTime <= 0)
                {
                    RemoveBuff(buff);
                }
            }
        }

        RecalculateAttributes();
    }

    /// <summary>
    /// 添加Buff - 统一入口
    /// </summary>
    public void AddBuff(BuffData buffData, GameObject source, float? customDuration = null)
    {
        if (buffData == null) return;
        if (source == null) source = gameObject;

        float finalDuration = customDuration ?? buffData.duration;

        // 查找同名Buff
        ActiveBuff existingSameId = activeBuffs.Find(b => b.data.buffId == buffData.buffId);

        if (existingSameId != null)
        {
            // 已有同名Buff：处理叠加和时长比较
            HandleExistingBuff(existingSameId, buffData, source, finalDuration);
            // 触发效果（周期性伤害等）
            TriggerBuffEffect(existingSameId);
            return;
        }

        // 没有同名Buff，根据持续时间类型添加新Buff
        switch (buffData.durationType)
        {
            case BuffDurationType.Instant:
                ApplyInstantEffect(buffData);
                break;

            case BuffDurationType.Timed:
                AddTimedBuff(buffData, source, finalDuration);
                break;

            case BuffDurationType.Permanent:
                AddPermanentBuff(buffData, source);
                break;
        }
    }

    /// <summary>
    /// 添加定时Buff（带自定义时长）
    /// </summary>
    void AddTimedBuff(BuffData buffData, GameObject source, float duration)
    {
        ActiveBuff newBuff = new ActiveBuff
        {
            data = buffData,
            source = source,
            remainingTime = duration,  // 使用传入的duration
            timer = 0f,
            currentStack = 1,
            durationType = BuffDurationType.Timed
        };

        activeBuffs.Add(newBuff);
        Debug.Log($"添加新Buff: {buffData.buffName}，持续时间: {duration}秒，来源: {source?.name}");

        // 按来源分类（用于地块移除）
        if (!buffsBySource.ContainsKey(source))
            buffsBySource[source] = new List<ActiveBuff>();
        buffsBySource[source].Add(newBuff);

        // 添加特效
        AttachBuffEffect(buffData);

        // 根据效果类型初始化
        if (buffData.effectType == BuffEffectType.attribute)
        {
            RecalculateAttributes();
        }
        else if (buffData.effectType == BuffEffectType.State)
        {
            ApplyStateEffect(buffData);
            newBuff.isApplied = true;
        }
    }

    /// <summary>
    /// 添加永久Buff
    /// </summary>
    void AddPermanentBuff(BuffData buffData, GameObject source)
    {
        ActiveBuff newBuff = new ActiveBuff
        {
            data = buffData,
            source = source,
            remainingTime = -1f,
            timer = 0f,
            currentStack = 1,
            durationType = BuffDurationType.Permanent
        };

        activeBuffs.Add(newBuff);
        Debug.Log($"添加新永久Buff: {buffData.buffName}");

        if (!buffsBySource.ContainsKey(source))
            buffsBySource[source] = new List<ActiveBuff>();
        buffsBySource[source].Add(newBuff);

        AttachBuffEffect(buffData);

        if (buffData.effectType == BuffEffectType.attribute)
            RecalculateAttributes();
    }

    /// <summary>
    /// 处理已存在的同名Buff
    /// </summary>
    void HandleExistingBuff(ActiveBuff existing, BuffData newBuffData, GameObject source, float newDuration)
    {
        Debug.Log($"处理同名Buff: {existing.data.buffName}, 当前剩余: {existing.remainingTime}秒, 新时长: {newDuration}秒");

        // 叠加逻辑
        if (newBuffData.stackable && existing.currentStack < newBuffData.maxStack)
        {
            existing.currentStack++;
            Debug.Log($"Buff叠加: {newBuffData.buffName} 层数 {existing.currentStack}");
            existing.source = source;

            if (newBuffData.effectType == BuffEffectType.attribute)
                RecalculateAttributes();
        }

        // 时长逻辑：根据配置决定是否刷新
        if (newBuffData.refreshOnReapply)
        {
            // 刷新模式：直接设置为新时长
            existing.remainingTime = newDuration;
            Debug.Log($"刷新Buff时间 -> {newDuration}秒");
        }
        else
        {
            // 保留模式：只取更长的那个
            if (newDuration > existing.remainingTime)
            {
                existing.remainingTime = newDuration;
                Debug.Log($"延长Buff时间 -> {newDuration}秒");
            }
        }
    }

    /// <summary>
    /// 触发Buff效果（用于周期性检测）
    /// </summary>
    void TriggerBuffEffect(ActiveBuff buff)
    {
        if (buff == null || buff.data == null) return;

        switch (buff.data.effectType)
        {
            case BuffEffectType.Damage:
                // 造成伤害
                float totalDamage = buff.data.effectValue * buff.currentStack;
                enemyComponent?.TakeDamage(totalDamage, buff.data.damageType);
                Debug.Log($"{gameObject.name} 受到 {totalDamage} 点 {buff.data.buffName} 伤害");
                break;

            case BuffEffectType.attribute:
                // 属性类Buff不需要每帧触发
                break;

            case BuffEffectType.State:
                // 状态类Buff不需要每帧触发
                break;
        }
    }

    /// <summary>
    ///  附加Buff特效 - 直接挂载在对象下
    /// </summary>
    void AttachBuffEffect(BuffData buffData)
    {
        // 如果已经有这个ID的特效，先移除旧的
        if (activeEffects.ContainsKey(buffData.buffId))
        {
            Destroy(activeEffects[buffData.buffId]);
            activeEffects.Remove(buffData.buffId);
        }

        // 如果有特效预制体
        if (buffData.effectPrefab != null)
        {
            // 直接实例化并挂载在当前对象下
            GameObject effect = Instantiate(buffData.effectPrefab, transform);

            // 可以设置局部位置（如果在预制体中已经设置好，这步可以省略）
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localRotation = Quaternion.identity;

            // 记录特效
            activeEffects[buffData.buffId] = effect;

            Debug.Log($" 为 {gameObject.name} 添加 {buffData.buffName} 特效");
        }
    }

    /// <summary>
    ///  移除Buff特效
    /// </summary>
    void RemoveBuffEffect(string buffId)
    {
        if (activeEffects.ContainsKey(buffId))
        {
            Destroy(activeEffects[buffId]);
            activeEffects.Remove(buffId);
            Debug.Log($" 移除 {buffId} 特效");
        }
    }

    void ApplyInstantEffect(BuffData buffData)
    {
        switch (buffData.effectType)
        {
            case BuffEffectType.Damage:
                if (enemyComponent != null)
                {
                    enemyComponent.TakeDamage(buffData.effectValue, buffData.damageType);
                }
                break;
            case BuffEffectType.State:
                ApplyStateEffect(buffData);
                break;
        }

        // 瞬时效果也可以播放特效（短暂出现后消失）
        if (buffData.effectPrefab != null)
        {
            GameObject effect = Instantiate(buffData.effectPrefab, transform);
            effect.transform.localPosition = Vector3.zero;
            Destroy(effect, 1f);
        }
    }

    void ApplyDamage(ActiveBuff buff)
    {
        if (enemyComponent == null) return;

        float totalDamage = buff.data.effectValue * buff.currentStack;

        //  使用 buff.data 中的伤害类型
        enemyComponent.TakeDamage(totalDamage, buff.data.damageType);
    }

    void ApplyStateEffect(BuffData buffData)
    {
        if (enemyComponent == null) return;

        switch (buffData.buffId)
        {
            case "buff_stun":
                enemyComponent.Stun(buffData.duration);
                break;
        }
    }

    void RecalculateAttributes()
    {
        if (enemyComponent == null) return;

        cachedSpeedMultiplier = 1f;

        foreach (var buff in activeBuffs)
        {
            if (buff?.data?.effectType != BuffEffectType.attribute) continue;

            float totalValue = buff.data.effectValue * buff.currentStack;

            // 只处理移速相关的Buff
            if (buff.data.attributeType == AttributeType.moveSpeed)
                cachedSpeedMultiplier *= totalValue;
        }

        enemyComponent.SetSpeedMultiplier(cachedSpeedMultiplier);
    }

    /// <summary>
    /// 移除指定来源的Buff
    /// </summary>
    public void RemoveBuff(string buffId, GameObject source)
    {
        if (source == null || !buffsBySource.ContainsKey(source)) return;

        var toRemove = buffsBySource[source].Find(b => b.data?.buffId == buffId);
        if (toRemove != null)
            RemoveBuff(toRemove);
    }

    /// <summary>
    /// 移除指定的Buff实例
    /// </summary>
    void RemoveBuff(ActiveBuff buff)
    {
        if (buff == null) return;

        //  移除特效
        if (buff.data != null)
        {
            RemoveBuffEffect(buff.data.buffId);
        }

        activeBuffs.Remove(buff);

        if (buff.source != null && buffsBySource.ContainsKey(buff.source))
        {
            buffsBySource[buff.source].Remove(buff);
            if (buffsBySource[buff.source].Count == 0)
                buffsBySource.Remove(buff.source);
        }

        RecalculateAttributes();
    }

    ActiveBuff GetBuffFromSource(string buffId, GameObject source)
    {
        return buffsBySource.ContainsKey(source)
            ? buffsBySource[source].Find(b => b.data?.buffId == buffId)
            : null;
    }

    public void ClearAllBuffs()
    {
        //  清除所有特效
        foreach (var effect in activeEffects.Values)
        {
            if (effect != null)
                Destroy(effect);
        }
        activeEffects.Clear();

        activeBuffs.Clear();
        buffsBySource.Clear();
        RecalculateAttributes();
    }

    void OnDestroy()
    {
        // 确保销毁时清理特效
        ClearAllBuffs();
    }
}