using UnityEngine;

public class TileVisual : MonoBehaviour
{
    public TileData tileData;

    // 只需要这两个组件
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Awake()
    {
        // 获取或添加 MeshFilter
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        // 获取或添加 MeshRenderer
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    void Start()
    {
        UpdateAppearance();
        // ? 新增：确保特殊组件存在
        EnsureSpecialComponents();
    }

    /// <summary>
    /// 更新外观（颜色、高度、模型）
    /// </summary>
    public void UpdateAppearance()
    {
        if (tileData == null) return;

        // 1. 设置颜色（用于编辑模式）
        UpdateColor();

        // 2. 设置高度
        UpdateHeight();

        // 3. 设置模型（重要！）
        UpdateModel();
    }

    /// <summary>
    /// 更新颜色
    /// </summary>
    void UpdateColor()
    {
        Color tileColor = tileData.GetColor();
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.color = tileColor;
        }
    }

    /// <summary>
    /// 更新高度
    /// </summary>
    void UpdateHeight()
    {
        Vector3 pos = transform.position;
        pos.y = tileData.GetHeight() * 1f;
        transform.position = pos;
    }

    /// <summary>
    /// 更新模型（从数据库加载）
    /// </summary>
    void UpdateModel()
    {
        string modelId = tileData.GetModelId();
        if (string.IsNullOrEmpty(modelId))
        {
            Debug.LogWarning($"地块 ({tileData.X},{tileData.Y}) 没有配置模型ID");
            return;
        }

        var modelData = TileModelDatabase.Instance?.GetModel(modelId);
        if (modelData == null)
        {
            Debug.LogWarning($"找不到模型数据: {modelId}");
            return;
        }

        // 更换 Mesh 和 Material
        if (modelData.mesh != null)
            meshFilter.mesh = modelData.mesh;

        if (modelData.material != null)
            meshRenderer.material = modelData.material;

        Debug.Log($"地块 ({tileData.X},{tileData.Y}) 模型已更新: {modelId}");
    }

    /// <summary>
    /// 手动更换模型（供编辑器调用）
    /// </summary>
    public void ChangeModel(string modelId)
    {
        if (tileData == null) return;

        // 保存自定义模型ID
        tileData.SetCustomModel(modelId);

        // 立即更新模型
        UpdateModel();

        Debug.Log($"地块 ({tileData.X},{tileData.Y}) 模型已更换为: {modelId}");
    }

    /// <summary>
    /// 重置为默认模型
    /// </summary>
    public void ResetToDefaultModel()
    {
        if (tileData == null) return;

        tileData.ResetToDefaultModel();
        UpdateModel();

        Debug.Log($"地块 ({tileData.X},{tileData.Y}) 已重置为默认模型");
    }

    // ==================== 新增：特殊组件管理 ====================

    /// <summary>
    /// 确保特殊组件存在（根据地块类型）
    /// </summary>
    void EnsureSpecialComponents()
    {
        if (tileData == null) return;

        string typeId = tileData.Type;

        // 先移除不匹配的特殊组件
        RemoveMismatchedComponents(typeId);

        // 根据地块类型添加对应组件
        switch (typeId)
        {
            case "Spawn":
                if (GetComponent<Spawn_Floor>() == null)
                {
                    gameObject.AddComponent<Spawn_Floor>();
                    Debug.Log($"地块 ({tileData.X},{tileData.Y}) 添加 Spawn_Floor 组件");
                }
                break;

            case "Ice_Floor":
                if (GetComponent<Ice_Floor>() == null)
                {
                    gameObject.AddComponent<Ice_Floor>();
                    Debug.Log($"地块 ({tileData.X},{tileData.Y}) 添加 Ice_Floor 组件");
                }
                break;

            case "Flame_Floor":
                if (GetComponent<Flame_Floor>() == null)
                {
                    gameObject.AddComponent<Flame_Floor>();
                    Debug.Log($"地块 ({tileData.X},{tileData.Y}) 添加 Flame_Floor 组件");
                }
                break;
            default:
                // 普通地块：移除所有特殊组件
                RemoveAllSpecialComponents();
                break;
        }
    }

    /// <summary>
    /// 移除不匹配的特殊组件
    /// </summary>
    void RemoveMismatchedComponents(string currentType)
    {
        // 定义哪些地块类型需要哪些组件
        bool shouldHaveSpawn = (currentType == "Spawn");
        bool shouldHaveIce = (currentType == "Ice_Floor");
        bool shouldHaveFlame = (currentType == "Flame_Floor");
        bool shouldHaveTreatment = (currentType == "Treatment_Floor");

        // 移除不应该存在的组件
        if (!shouldHaveSpawn && GetComponent<Spawn_Floor>() != null)
            Destroy(GetComponent<Spawn_Floor>());

        if (!shouldHaveIce && GetComponent<Ice_Floor>() != null)
            Destroy(GetComponent<Ice_Floor>());

        if (!shouldHaveFlame && GetComponent<Flame_Floor>() != null)
            Destroy(GetComponent<Flame_Floor>());
    }

    /// <summary>
    /// 移除所有特殊组件
    /// </summary>
    void RemoveAllSpecialComponents()
    {
        if (GetComponent<Spawn_Floor>() != null)
            Destroy(GetComponent<Spawn_Floor>());

        if (GetComponent<Ice_Floor>() != null)
            Destroy(GetComponent<Ice_Floor>());

        if (GetComponent<Flame_Floor>() != null)
            Destroy(GetComponent<Flame_Floor>());
    }
}