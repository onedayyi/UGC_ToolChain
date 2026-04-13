using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("===== 动画设置 =====")]
    public float moveUpSpeed = 2f;      // 上升速度
    public float fadeOutTime = 0.5f;     // 淡出时间
    public float destroyTime = 1f;       // 自动销毁时间

    private TextMeshPro textMesh;
    private float timer = 0f;
    private Color originalColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogError("DamageNumber: 找不到 TextMeshPro 组件！请在预制体上添加");
            return;
        }

        // 保存原始颜色
        originalColor = textMesh.color;
    }

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        if (textMesh == null) return;

        // 向上移动
        transform.Translate(Vector3.up * moveUpSpeed * Time.deltaTime);

        // 面向相机（保持在3D世界但总是面向屏幕）
        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;

        // 淡出效果
        timer += Time.deltaTime;
        if (timer >= destroyTime - fadeOutTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, (timer - (destroyTime - fadeOutTime)) / fadeOutTime);
            Color c = textMesh.color;
            c.a = alpha;
            textMesh.color = c;
        }
    }

    public void SetDamage(float damage, DamageType damageType = DamageType.Physical)
    {
        if (textMesh == null) return;

        // 只设置文本内容和颜色，不设置字号
        textMesh.text = Mathf.RoundToInt(damage).ToString();

        // 根据伤害类型设置颜色
        switch (damageType)
        {
            case DamageType.Physical:
                textMesh.color = Color.white;
                break;
            case DamageType.Arts:
                textMesh.color = new Color(0.8f, 0.4f, 1f); // 紫色
                break;
            case DamageType.True:
                textMesh.color = Color.yellow;
                break;
            case DamageType.Heal:
                textMesh.color = Color.green;
                textMesh.text = "+" + Mathf.RoundToInt(damage).ToString();
                break;
        }
    }

    public void SetCritical(bool isCritical)
    {
        if (isCritical && textMesh != null)
        {
            textMesh.color = Color.red;
            textMesh.text += "!";
            // 如果需要暴击时变大，可以在预制体中设置不同的字号
        }
    }
}