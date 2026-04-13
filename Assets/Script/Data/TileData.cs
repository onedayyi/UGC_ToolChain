using UnityEngine;
using System.Collections.Generic;

public class TileData
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public string Type { get; set; }
    public string CustomModelId;  // 自定义模型ID（null表示使用默认）
    // 添加字典来存储自定义属性
    private Dictionary<string, object> customProperties = new Dictionary<string, object>();

    public TileData(int x, int y, string type)
    {
        X = x;
        Y = y;
        Type = type;
        CustomModelId = null;
    }
    /// <summary>
    /// 获取当前使用的模型ID
    /// </summary>
    public string GetModelId()
    {
        // 优先使用自定义模型
        if (!string.IsNullOrEmpty(CustomModelId))
            return CustomModelId;

        // 否则使用地块类型的默认模型
        var tileType = TileTypeDatabase.Instance.GetType(Type);
        return tileType.defaultModelId;
    }

    /// <summary>
    /// 设置自定义模型
    /// </summary>
    public void SetCustomModel(string modelId)
    {
        CustomModelId = modelId;
    }

    /// <summary>
    /// 重置为默认模型
    /// </summary>
    public void ResetToDefaultModel()
    {
        CustomModelId = null;
    }
    // ===== 颜色相关 =====
    public Color GetColor()
    {
        return TileTypeDatabase.Instance.GetType(Type).color;
    }

    // ===== 高度相关 =====
    public int GetHeight()
    {
        return TileTypeDatabase.Instance.GetType(Type).height;
    }

    // ===== 可通行相关 =====
    public bool CanWalk()
    {
        // 优先检查是否有自定义属性
        if (customProperties.ContainsKey("customWalkable"))
        {
            return (bool)customProperties["customWalkable"];
        }
        // 否则返回数据库默认值
        return TileTypeDatabase.Instance.GetType(Type).canWalk;
    }

    // ===== 可部署相关 =====
    public bool CanPlace()
    {
        // 优先检查是否有自定义属性
        if (customProperties.ContainsKey("customPlaceable"))
        {
            return (bool)customProperties["customPlaceable"];
        }
        // 否则返回数据库默认值
        return TileTypeDatabase.Instance.GetType(Type).canPlace;
    }

    // ===== 【新增】设置自定义属性 =====
    public void SetProperty(string key, object value)
    {
        if (customProperties.ContainsKey(key))
        {
            customProperties[key] = value;  // 修改已有属性
        }
        else
        {
            customProperties.Add(key, value);  // 添加新属性
        }

        Debug.Log($"Tile ({X},{Y}) 设置属性: {key} = {value}");
    }

    // ===== 【新增】获取自定义属性 =====
    public T GetProperty<T>(string key, T defaultValue = default)
    {
        if (customProperties.ContainsKey(key) && customProperties[key] is T)
        {
            return (T)customProperties[key];
        }
        return defaultValue;
    }

    // ===== 【新增】检查是否有某个自定义属性 =====
    public bool HasProperty(string key)
    {
        return customProperties.ContainsKey(key);
    }

    // ===== 【新增】移除自定义属性 =====
    public void RemoveProperty(string key)
    {
        if (customProperties.ContainsKey(key))
        {
            customProperties.Remove(key);
        }
    }

    // ===== 【新增】获取所有自定义属性（用于调试）=====
    public Dictionary<string, object> GetAllProperties()
    {
        return new Dictionary<string, object>(customProperties);
    }
}