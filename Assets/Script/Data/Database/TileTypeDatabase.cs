using UnityEngine;
using System.Collections.Generic;

// 这个类用来定义"一种地块类型"长什么样
[System.Serializable]                           //字典
public class TileTypeDefinition
{
    public string id;           // 唯一标识，比如 "ground"
    public string displayName;  // 显示名字，比如 "地面"
    public bool canWalk;        // 能不能走上去
    public bool canPlace;       // 能不能放干员
    public int height;          // 高度（0地面，1高台，2墙壁）
    public Color color;         // 显示颜色
    public string buffId;        // 关联的Buff ID (如 "buff_flame")
    public string defaultModelId; // 默认模型ID
    // 构造函数
    public TileTypeDefinition(string id, string name, bool walk, bool place, int h, Color c,
                                        string buffId = null, string defaultModelId = null)
    {
        this.id = id;
        this.displayName = name;
        this.canWalk = walk;
        this.canPlace = place;
        this.height = h;
        this.color = c;
        this.buffId = buffId;
        this.defaultModelId = defaultModelId;
    }
    public bool HasBuff => !string.IsNullOrEmpty(buffId);
}

// 类型数据库
public class TileTypeDatabase
{
    // ===== 单例模式 =====
    private static TileTypeDatabase _instance;
    public static TileTypeDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new TileTypeDatabase();
            }
            return _instance;
        }
    }

    // ===== 【核心】字典：用来存储所有类型，以ID为key快速查找 =====
    private Dictionary<string, TileTypeDefinition> typeDict = new Dictionary<string, TileTypeDefinition>();

    // ===== 私有构造函数（不让外部new）=====
    private TileTypeDatabase()
    {
        // 添加地面类型
        AddType(new TileTypeDefinition(
            "Ground",       // id
            "地面",         // 显示名
            true,           // 可以走
            true,           // 可以放干员
            0,              // 高度0
            Color.green ,    // 绿色
            defaultModelId: "ground_default"
        ));

        // 添加高台类型
        AddType(new TileTypeDefinition(
            "Highground",   // id
            "高台",         // 显示名
            false,          // 不能走
            true,           // 可以放干员
            1,              // 高度1
            Color.yellow,    // 黄色
            defaultModelId: "highground_default"
        ));

        // 添加墙壁类型
        AddType(new TileTypeDefinition(
            "Wall",         // id
            "墙壁",         // 显示名
            false,          // 不能走
            false,          // 不能放干员
            1,              // 高度2
            Color.gray,      // 灰色
            defaultModelId: "wall_default"
        ));

        // 添加出怪点
        AddType(new TileTypeDefinition(
            "Spawn",        // id
            "出怪点",       // 显示名
            false,           // 可以走
            false,          // 不能放干员
            1,              // 高度1
            Color.red,       // 红色
            defaultModelId: "spawn_default"
        ));
        
        // 添加出保护点
        AddType(new TileTypeDefinition(
            "Protect",        // id
            "保护点",       // 显示名
            true,           // 可以走
            false,          // 不能放干员
            1,              // 高度1
            Color.blue,       // 蓝色
            defaultModelId: "protect_default"
        ));

        AddType(new TileTypeDefinition(
            "Recover_Floor",  //id
            "治疗地板",     //显示名
            true,           //可以走
            true,           //可以放干员
            0,              //高度0
            new Color(0, 1, 1),   //青色
            "Buff_recover"
            ));

        AddType(new TileTypeDefinition(
            "Flame_Floor",  //id
            "火焰地板",     //显示名
            true,           //可以走
            true,           //可以放干员
            0,              //高度0
            Color.magenta,   //洋红色
            "Buff_flame",
            defaultModelId: "floor_flame"
            ));

        AddType(new TileTypeDefinition(
            "Ice_Floor",  //id
            "寒冰地板",     //显示名
            true,           //可以走
            true,           //可以放干员
            0,              //高度0
            new Color(160f/255f,214f/255f,255f/255f),
            "Buff_ice",
            defaultModelId: "floor_ice"
            ));
        AddType(new TileTypeDefinition(
            "Poison_Floor",  //id
            "毒地板",     //显示名
            true,           //可以走
            true,           //可以放干员
            0,              //高度0
            new Color(0.1f, 0.5f, 0.1f),
            "Buff_poison"
            ));

        Debug.Log($"TileTypeDatabase 初始化完成，字典里有 {typeDict.Count} 种类型");
    }

    // ===== 【核心】添加类型到字典 =====
    private void AddType(TileTypeDefinition type)
    {
        // 检查是否已经存在相同的ID
        if (!typeDict.ContainsKey(type.id))
        {
            // 添加到字典，key是id，value是整个定义
            typeDict.Add(type.id, type);
            //Debug.Log($"添加类型: {type.id} -> {type.displayName}");
        }
        else
        {
            Debug.LogWarning($"类型ID {type.id} 已存在，添加失败");
        }
    }

    // ===== 【核心】根据ID获取类型定义 =====
    public TileTypeDefinition GetType(string id)
    {
        // 在字典中查找
        if (typeDict.ContainsKey(id))
        {
            return typeDict[id];  // 找到了，返回对应的定义
        }
        else
        {
            // 没找到，返回地面作为默认
            //Debug.LogWarning($"找不到类型: {id}，返回默认地面");
            return typeDict["Ground"];
        }
    }

    // ===== 检查某个ID是否存在 =====
    public bool HasType(string id)
    {
        return typeDict.ContainsKey(id);
    }

    // ===== 获取所有类型的列表（方便做UI）=====
    public List<TileTypeDefinition> GetAllTypes()
    {
        // 把字典里的所有值转成列表
        return new List<TileTypeDefinition>(typeDict.Values);
    }

    // ===== 打印所有类型（调试用）=====
    public void PrintAllTypes()
    {
        Debug.Log("=== 所有地块类型 ===");
        foreach (var pair in typeDict)
        {
            TileTypeDefinition type = pair.Value;
            Debug.Log($"ID: {type.id}, 名称: {type.displayName}, 颜色: {type.color}");
        }
    }
}