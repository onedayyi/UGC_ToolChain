using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    [Header("===== 预制体 =====")]
    public GameObject damageNumberPrefab;

    [Header("===== 生成设置 =====")]
    public float offsetRange = 0.5f;

    public static DamageNumberManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDamage(Vector3 position, float damage, DamageType damageType = DamageType.Physical, bool isCritical = false)
    {
        if (damageNumberPrefab == null)
        {
            Debug.LogError("DamageNumberPrefab 未设置！");
            return;
        }

        Vector3 offset = new Vector3(
            Random.Range(-offsetRange, offsetRange),
            Random.Range(0, offsetRange),
            Random.Range(-offsetRange, offsetRange)
        );

        // 直接实例化，不经过对象池
        GameObject damageObj = Instantiate(damageNumberPrefab, position + offset, Quaternion.identity);

        DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.SetDamage(damage, damageType);
            damageNumber.SetCritical(isCritical);
        }

        // 注意：DamageNumber 脚本中已经有 Destroy(gameObject, destroyTime)
        // 所以不需要额外处理
    }
}