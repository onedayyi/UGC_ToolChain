using UnityEngine;

// 这个类用来管理整张地图的所有格子数据
public class MapData
{
    // ===== 基本属性 =====
    public int width;           // 地图宽度（多少列）
    public int height;          // 地图高度（多少行）
    private TileData[,] tiles;   // 二维数组，存储所有格子的数据

    // ===== 构造函数：创建新地图 =====
    public MapData(int w, int h)
    {
        width = w;
        height = h;

        // 创建二维数组
        tiles = new TileData[w, h];

        // 遍历每一个格子，创建默认的地面
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 每个格子默认都是"ground"类型
                tiles[x, y] = new TileData(x, y, "ground");
            }
        }

        Debug.Log($"创建了新地图: {width}x{height}");
    }

    // ===== 获取单个格子数据 =====
    public TileData GetTile(int x, int y)
    {
        // 检查坐标是否有效
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return tiles[x, y];
        }

        Debug.LogWarning($"尝试获取无效坐标的格子: ({x}, {y})");
        return null;
    }

    // ===== 修改格子类型 =====
    public void SetTileType(int x, int y, string typeId)
    {
        // 检查坐标是否有效
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            // 创建新的TileData（或者修改现有的）
            tiles[x, y] = new TileData(x, y, typeId);

            //Debug.Log($"格子 ({x},{y}) 类型改为: {typeId}");
        }
        else
        {
            Debug.LogWarning($"尝试修改无效坐标的格子: ({x}, {y})");
        }
    }
}