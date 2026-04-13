using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("===== 基础信息 =====")]
    public string enemyId;              // 敌人ID
    public string enemyName;            // 敌人名称
    public EnemyRank rank;              // 敌人地位

    [Header("===== 当前状态 =====")]
    public float currentHP;             // 当前生命值
    public bool isAlive = true;         // 是否存活
    public bool isStunned = false;      // 是否眩晕
    public bool isSlowed = false;       // 是否减速

    [Header("===== 组件引用 =====")]
    public NavMeshAgent agent;          // 导航组件
    public Animator animator;           // 动画控制器
    public Collider enemyCollider;      // 碰撞体
    public GameObject icon;             // 头顶图标（血条等）

    // 原始属性（从EnemyData读取）
    public float maxHP;                 // 最大生命值
    public float attackPower;           // 攻击力
    public float defense;               // 防御力
    public float magicResistance;       // 法术抗性
    public float moveSpeed;             // 移动速度
    public float attackInterval;        // 攻击间隔

    // 当前战斗状态
    protected float attackTimer = 0f;
    protected GameObject currentTarget;

    protected virtual void Awake()
    {
        // 自动获取组件
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider>();
    }

    protected void Update()
    {
        // 让头顶图标始终面向相机（Billboard效果）
        if (Camera.main != null && icon != null)
            icon.transform.rotation = Camera.main.transform.rotation;
    }

    /// <summary>
    /// 初始化敌人数据（由Spawn_Floor调用）
    /// </summary>
    public virtual void Initialize(EnemyData data)
    {
        enemyId = data.enemyId;
        enemyName = data.enemyName;
        rank = data.rank;

        // 保存基础属性
        maxHP = data.maxHP;
        currentHP = maxHP;
        attackPower = data.attack;
        defense = data.defense;
        magicResistance = data.magicResistance;
        moveSpeed = data.moveSpeed;
        attackInterval = data.attackInterval;

        // 设置NavMeshAgent速度
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }

        Debug.Log($"敌人初始化: {enemyName}, HP: {currentHP}");
    }

    /// <summary>
    /// 受伤
    /// </summary>
    public virtual void TakeDamage(float damage, DamageType damageType = DamageType.Physical)
    {
        float finalDamage = damage;

        // 根据伤害类型计算最终伤害
        switch (damageType)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(1, damage - defense);
                break;
            case DamageType.Arts:
                finalDamage = damage * (1 - magicResistance / 100f);
                break;
            case DamageType.True:
                finalDamage = damage;
                break;
            case DamageType.Heal:
                finalDamage = -damage;  // 治疗
                break;
        }

        currentHP -= finalDamage;

        // 显示伤害数字
        if (DamageNumberManager.Instance != null)
        {
            Debug.Log($"显示伤害数字: {finalDamage}");
            Vector3 displayPos = transform.position + Vector3.up * 2f; // 在敌人头顶显示
            DamageNumberManager.Instance.ShowDamage(displayPos, Mathf.Abs(finalDamage), damageType);
        }

        // 限制HP范围
        if (currentHP > maxHP)
            currentHP = maxHP;

        Debug.Log($"{enemyName} 受到 {finalDamage} 点 {damageType} 伤害，剩余 HP: {currentHP}");

        // 触发受伤动画
        if (animator != null)
            animator.SetTrigger("Hurt");

        // 检查是否死亡
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // 为了兼容旧的调用方式，保留一个简化版本
    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, DamageType.Physical);  // 默认物理伤害
    }

    /// <summary>
    /// 死亡
    /// </summary>
    protected virtual void Die()
    {
        isAlive = false;
        Debug.Log($"{enemyName} 死亡");

        // 停止移动
        if (agent != null)
            agent.isStopped = true;

        // 触发死亡动画
        if (animator != null)
            animator.SetTrigger("Die");

        // 禁用碰撞体
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        // 延迟销毁
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// 设置移动速度（用于减速/加速效果）
    /// </summary>
    public virtual void SetSpeedMultiplier(float multiplier)
    {
        if (agent != null)
        {
            agent.speed = moveSpeed * multiplier;
            Debug.Log($"{enemyName} 速度变为: {agent.speed} (原速: {moveSpeed}, 倍率: {multiplier})");
        }
    }

    /// <summary>
    /// 眩晕
    /// </summary>
    public virtual void Stun(float duration)
    {
        if (isStunned) return;

        isStunned = true;
        if (agent != null)
            agent.isStopped = true;

        // 启动协程恢复
        StartCoroutine(RecoverFromStun(duration));
    }

    System.Collections.IEnumerator RecoverFromStun(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        if (agent != null && isAlive)
            agent.isStopped = false;
        Debug.Log($"{enemyName} 眩晕结束");
    }

    /// <summary>
    /// 攻击目标
    /// </summary>
    protected virtual void Attack(GameObject target)
    {
        // 由子类实现具体攻击逻辑
    }

    /// <summary>
    /// 重置状态（用于对象池复用）
    /// </summary>
    public virtual void ResetEnemy()
    {
        currentHP = maxHP;
        isAlive = true;
        isStunned = false;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = moveSpeed;
        }

        if (enemyCollider != null)
            enemyCollider.enabled = true;
    }
}