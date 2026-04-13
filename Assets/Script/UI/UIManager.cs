using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI面板")]
    public GameObject tileSelectorPanel;    // 左上角选择面板
    public GameObject tilePropertyPanel;    // 右上角属性面板
    public GameObject EnemyNavmeshPanel;    // 右上角寻路面板

    [Header("画布缩放")]
    public CanvasScaler canvasScaler;

    void Start()
    {
        // 确保UI面板显示
        if (tileSelectorPanel != null)
            tileSelectorPanel.SetActive(true);

        if (tilePropertyPanel != null)
            tilePropertyPanel.SetActive(true);

        if (EnemyNavmeshPanel != null)
            EnemyNavmeshPanel.SetActive(true);
    }

    void Update()
    {
        // 按Tab键切换UI显示/隐藏（方便截图）
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUI();
        }
    }

    void ToggleUI()
    {
        bool isVisible = !tileSelectorPanel.activeSelf;
        tileSelectorPanel.SetActive(isVisible);
        tilePropertyPanel.SetActive(isVisible);
        EnemyNavmeshPanel.SetActive(isVisible);
    }
}