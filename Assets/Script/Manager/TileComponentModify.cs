using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComponentModify : MonoBehaviour
{
    public static TileComponentModify _instance;

    //===========单例模式===========
    public static TileComponentModify Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TileComponentModify>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 添加效果组件
    public void SetFloor(string id, GameObject obj)
    {
        switch (id)
        {
            case "Spawn":
                Debug.Log("添加出怪点路径");
                // 先检查是否已有组件
                Spawn_Floor Spawn = obj.GetComponent<Spawn_Floor>();

                // 如果没有才添加
                if (Spawn == null)
                {
                    obj.AddComponent<Spawn_Floor>();
                    Debug.Log("出怪点组件已添加");
                }
                else
                {
                    Debug.Log("出怪点组件已存在");
                }
                break;
            case "Protect":
                Debug.Log("添加出怪点路径");
                // 先检查是否已有组件
                Protect_Floor Protect = obj.GetComponent<Protect_Floor>();

                // 如果没有才添加
                if (Protect == null)
                {
                    obj.AddComponent<Protect_Floor>();
                    Debug.Log("出怪点组件已添加");
                }
                else
                {
                    Debug.Log("出怪点组件已存在");
                }
                break;
            case "Flame_Floor":
                Debug.Log("添加火焰效果");
                // 先检查是否已有组件
                Flame_Floor flame = obj.GetComponent<Flame_Floor>();

                // 如果没有才添加
                if (flame == null)
                {
                    obj.AddComponent<Flame_Floor>();
                    Debug.Log("火焰效果组件已添加");
                }
                else
                {
                    Debug.Log("火焰效果组件已存在");
                }
                break;

            case "Ice_Floor":
                Debug.Log("添加冰霜效果");
                Ice_Floor ice = obj.GetComponent<Ice_Floor>();
                if (ice == null)
                {
                    obj.AddComponent<Ice_Floor>();
                }
                break;

            case "Poison_Floor":
                Debug.Log("添加中毒效果");
                Poison_Floor poison = obj.GetComponent<Poison_Floor>();
                if (poison == null)
                {
                    obj.AddComponent<Poison_Floor>();
                }
                break;
            case "Recover_Floor":
                Debug.Log("添加治疗效果");
                Recover_Floor recover = obj.GetComponent<Recover_Floor>();
                if (recover == null)
                {
                    obj.AddComponent<Recover_Floor>();
                }
                break;
            default:
                //Debug.Log($"未知的地块类型: {id}，无需添加效果");
                break;
        }
    }

    // 移除效果组件
    public void RemoveFloorBuff(string id, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("要移除效果的对象为 null");
            return;
        }

        switch (id)
        {
            case "Spawn":
                Debug.Log("尝试移除出怪点效果");
                RemoveComponent<Spawn_Floor>(obj);
                break;

            case "Flame_Floor":
                Debug.Log("尝试移除火焰效果");
                RemoveComponent<Flame_Floor>(obj);
                break;

            case "Ice_Floor":
                Debug.Log("尝试移除冰霜效果");
                RemoveComponent<Ice_Floor>(obj);
                break;

            case "Poison_Floor":
                Debug.Log("尝试移除中毒效果");
                RemoveComponent<Poison_Floor>(obj);
                break;

            default:
                Debug.Log($"类型 {id} 没有需要移除的效果");
                break;
        }
    }

    // 通用移除组件方法（可选）
    private void RemoveComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component != null)
        {
            Destroy(component);
            Debug.Log($"已移除 {typeof(T).Name} 组件");
        }
        else
        {
            Debug.Log($"没有找到 {typeof(T).Name} 组件");
        }
    }

    // 移除所有效果组件（当切换地块时调用）
    public void RemoveAllEffects(GameObject obj)
    {
        RemoveComponent<Flame_Floor>(obj);
        RemoveComponent<Ice_Floor>(obj);
        RemoveComponent<Poison_Floor>(obj);
        Debug.Log("已移除所有效果组件");
    }
}