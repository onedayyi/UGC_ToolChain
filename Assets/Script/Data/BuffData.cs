using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Buff数据模板 - 每个Buff一个文件
/// </summary>
[CreateAssetMenu(fileName = "Buff_", menuName = "GameData/Buff", order = 2)]
public class BuffData : ScriptableObject
{
    [Header("===== 基本信息 =====")]
    public string buffId;                    // 唯一ID (如 "buff_flame", "buff_ice")
    public string buffName;                   // 显示名称 (如 "燃烧", "冰冻")
    public string buffDescription;             // 描述文本
    public Sprite buffIcon;                    // UI图标

    [Header("===== Buff类型 =====")]
    public BuffCategory category = BuffCategory.Debuff;  // 增益/减益
    public BuffEffectType effectType;                   // 效果类型

    [Header("===== 受影响的属性 =====")]
    public AttributeType attributeType;         //生命、攻击、防御、移速

    [Header("===== 持续时间类型 =====")]
    public BuffDurationType durationType;       //即时、有限、无限

    [Header("===== 效果数值 =====")]
    public DamageType damageType;
    public float effectValue = 10f;             // 效果数值
    public float tickInterval = 0.5f;           // 触发间隔（适用于DOT/HOT）
    public float duration = 5f;                 // 持续时间（秒）
    public bool stackable = false;              // 是否可叠加
    public bool refreshOnReapply = true;        // 重复施加时是否刷新时间
    public int maxStack = 3;                    // 最大叠加层数

    [Header("===== 视觉效果 =====")]
    public GameObject effectPrefab;               // 特效预制体
    public AudioClip applySound;                   // 施加音效
    public AudioClip tickSound;                    // 触发音效
    public Color effectColor = Color.white;        // 效果颜色
}

// 添加自定义 Inspector（在同一个文件末尾）
#if UNITY_EDITOR
[CustomEditor(typeof(BuffData))]
public class BuffDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuffData buff = (BuffData)target;

        // 基本信息
        EditorGUILayout.LabelField("===== 基本信息 =====", EditorStyles.boldLabel);
        buff.buffId = EditorGUILayout.TextField("Buff ID", buff.buffId);
        buff.buffName = EditorGUILayout.TextField("Buff Name", buff.buffName);
        buff.buffDescription = EditorGUILayout.TextField("Description", buff.buffDescription);
        buff.buffIcon = (Sprite)EditorGUILayout.ObjectField("Icon", buff.buffIcon, typeof(Sprite), false);

        // Buff类型
        EditorGUILayout.LabelField("===== Buff类型 =====", EditorStyles.boldLabel);
        buff.category = (BuffCategory)EditorGUILayout.EnumPopup("Category", buff.category);
        buff.effectType = (BuffEffectType)EditorGUILayout.EnumPopup("Effect Type", buff.effectType);

        // 受影响的属性（只在 attribute 类型时显示）
        if (buff.effectType == BuffEffectType.attribute)
        {
            buff.attributeType = (AttributeType)EditorGUILayout.EnumPopup("Attribute Type", buff.attributeType);
        }

        // 持续时间类型
        EditorGUILayout.LabelField("===== 持续时间类型 =====", EditorStyles.boldLabel);
        buff.durationType = (BuffDurationType)EditorGUILayout.EnumPopup("Duration Type", buff.durationType);

        // 效果数值
        EditorGUILayout.LabelField("===== 效果数值 =====", EditorStyles.boldLabel);

        // 只在 Damage 类型时显示伤害类型
        if (buff.effectType == BuffEffectType.Damage)
        {
            buff.damageType = (DamageType)EditorGUILayout.EnumPopup("Damage Type", buff.damageType);
        }

        buff.effectValue = EditorGUILayout.FloatField("Effect Value", buff.effectValue);

        // 如果不是 Instant 类型，显示以下字段
        if (buff.durationType != BuffDurationType.Instant)
        {
            buff.tickInterval = EditorGUILayout.FloatField("Tick Interval", buff.tickInterval);
            buff.duration = EditorGUILayout.FloatField("Duration", buff.duration);
            buff.stackable = EditorGUILayout.Toggle("Stackable", buff.stackable);

            buff.refreshOnReapply = EditorGUILayout.Toggle("Refresh On Reapply", buff.refreshOnReapply);
            buff.maxStack = EditorGUILayout.IntField("Max Stack", buff.maxStack);
            
        }

        // 视觉效果
        EditorGUILayout.LabelField("===== 视觉效果 =====", EditorStyles.boldLabel);
        buff.effectPrefab = (GameObject)EditorGUILayout.ObjectField("Effect Prefab", buff.effectPrefab, typeof(GameObject), false);
        buff.applySound = (AudioClip)EditorGUILayout.ObjectField("Apply Sound", buff.applySound, typeof(AudioClip), false);
        buff.tickSound = (AudioClip)EditorGUILayout.ObjectField("Tick Sound", buff.tickSound, typeof(AudioClip), false);
        buff.effectColor = EditorGUILayout.ColorField("Effect Color", buff.effectColor);

        // 保存修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(buff);
        }
    }
}
#endif

/// <summary>
/// 属性分类
/// </summary>
public enum AttributeType
{
    currentHP,       // 当前生命
    attackPower,     // 攻击力
    defense,         // 防御力
    moveSpeed        // 移速
}

/// <summary>
/// Buff分类
/// </summary>
public enum BuffCategory
{
    Buff,       // 增益
    Debuff,     // 减益
    Special     // 特殊
}

/// <summary>
/// Buff效果类型
/// </summary>
public enum BuffEffectType
{
    Damage,
    attribute,
    State
}

/// <summary>
/// 持续时间类型
/// </summary>
public enum BuffDurationType
{
    Instant,    // 即时生效（如伤害）
    Timed,      // 计时消失
    Permanent,  // 永久（直到被移除）
}

public enum DamageType
{
    Physical,   // 物理伤害
    Arts,       // 法术伤害
    True,       // 真实伤害
    Heal        // 治疗
}