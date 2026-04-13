using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 模型条目
/// </summary>
[System.Serializable]
public class TileModelEntry
{
    public string modelId;      // 模型唯一ID，如 "ground_stone"
    public string modelName;    // 显示名称，如 "石头地面"
    public Mesh mesh;           // 网格数据
    public Material material;   // 材质
    public Sprite thumbnail;    // 缩略图（用于UI）
}

/// <summary>
/// 地块模型数据库
/// </summary>
public class TileModelDatabase : MonoBehaviour
{
    [Header("===== 模型列表 =====")]
    public List<TileModelEntry> allModels;

    private Dictionary<string, TileModelEntry> modelDict;

    public static TileModelDatabase Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            BuildDictionary();
        }
        else
        {
            Destroy(gameObject);
        }

        // 如果配置为空，自动创建测试模型
        if (allModels == null || allModels.Count == 0)
        {
            CreateDefaultModel();
        }
        BuildDictionary();
    }
    void CreateDefaultModel()
    {
        // 创建一个测试用的立方体模型
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh testMesh = cube.GetComponent<MeshFilter>().sharedMesh;
        Material testMaterial = cube.GetComponent<MeshRenderer>().sharedMaterial;

        TileModelEntry defaultModel = new TileModelEntry();
        defaultModel.modelId = "ground_default";
        defaultModel.modelName = "默认地面";
        defaultModel.mesh = testMesh;
        defaultModel.material = testMaterial;

        allModels = new List<TileModelEntry>();
        allModels.Add(defaultModel);

        Destroy(cube);

        Debug.Log("已创建默认测试模型");
    }
    void BuildDictionary()
    {
        modelDict = new Dictionary<string, TileModelEntry>();
        foreach (var model in allModels)
        {
            if (!modelDict.ContainsKey(model.modelId))
            {
                modelDict.Add(model.modelId, model);
            }
            else
            {
                Debug.LogWarning($"重复的模型ID: {model.modelId}");
            }
        }
        Debug.Log($"TileModelDatabase 初始化完成，共 {modelDict.Count} 种模型");
    }
    /// <summary>
    /// 获取所有模型（用于UI）
    /// </summary>
    public List<TileModelEntry> GetAllModels()
    {
        return allModels;
    }

    /// <summary>
    /// 根据ID获取模型条目
    /// </summary>
    public TileModelEntry GetModel(string modelId)
    {
        if (string.IsNullOrEmpty(modelId)) return null;
        return modelDict.ContainsKey(modelId) ? modelDict[modelId] : null;
    }
}