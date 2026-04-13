using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 明日方舟风格敌人数据 - 简化测试版
/// </summary>
[CreateAssetMenu(fileName = "Enemy_", menuName = "GameData/Enemy", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("===== 基本信息 =====")]
    public string enemyId;                    // 敌人ID
    public string enemyName;                   // 敌人名称
    public string enemyType;                    // 种类 (如: 感染生物)
    public EnemyRank rank = EnemyRank.Normal;    // 地位 (普通/精英/BOSS)

    [TextArea(2, 3)]
    public string description;                  // 描述

    [Header("===== 基础属性 =====")]
    public int maxHP = 100;                      // 最大生命值
    public int attack = 0;                      // 攻击力
    public int defense = 0;                       // 防御力
    public int magicResistance = 0;                 // 法术抗性
    public int elementResistance = 0;               // 元素抗性
    public int weight = 0;                          // 重量等级

    [Header("===== 移动属性 =====")]
    public float moveSpeed = 1.0f;                  // 移动速度
    public MoveType moveType = MoveType.Ground;     // 行动方式

    [Header("===== 攻击属性 =====")]
    public AttackType attackType = AttackType.Melee; // 攻击方式
    public float attackRange = 0f;                    // 攻击半径 (-- 表示0)
    public float attackInterval = 1f;                // 攻击间隔

    [Header("===== 其他属性 =====")]
    public int healthRecovery = 0;                     // 生命自回速度
    public int damageResistance = 0;                    // 损伤抵抗
    public int tauntLevel = 0;                          // 基础嘲讽等级
    public int targetValue = 1;                          // 目标价值

    [Header("===== 视觉表现 =====")]
    public GameObject enemyPrefab;                       // 敌人模型
    public Sprite enemyIcon;                              // 头像图标
}

/// <summary>
/// 敌人地位
/// </summary>
public enum EnemyRank
{
    Normal,         // 普通
    Elite,          // 精英
    Boss            // BOSS
}

/// <summary>
/// 行动方式
/// </summary>
public enum MoveType
{
    Ground,         // 地面
    Flying,         // 飞行
    Both            // 两者
}

/// <summary>
/// 攻击方式
/// </summary>
public enum AttackType
{
    Melee,          // 近战
    Ranged,         // 远程
    None            // 不攻击
}