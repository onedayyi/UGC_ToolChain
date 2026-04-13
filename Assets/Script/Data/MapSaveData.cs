using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapSaveData
{
    public string mapName;
    public string mapId;
    public string createTime;
    public string lastModified;

    public int width;
    public int height;

    [Serializable]
    public struct TileSaveData
    {
        public int x;
        public int y;
        public string typeId;
    }

    public List<TileSaveData> tiles = new List<TileSaveData>();

    // 从MapData创建
    public MapSaveData(MapData mapData, string name)
    {
        mapName = name;
        mapId = Guid.NewGuid().ToString();
        createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        width = mapData.width;
        height = mapData.height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileData tile = mapData.GetTile(x, y);
                tiles.Add(new TileSaveData
                {
                    x = x,
                    y = y,
                    typeId = tile.Type
                });
            }
        }
    }

    // 转换为MapData
    public MapData ToMapData()
    {
        MapData mapData = new MapData(width, height);
        foreach (var tile in tiles)
        {
            mapData.SetTileType(tile.x, tile.y, tile.typeId);
        }
        return mapData;
    }

    // 更新修改时间
    public void UpdateModifiedTime()
    {
        lastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}