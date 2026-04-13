using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    public MapEditor mapEditor;
    public int tileX;
    public int tileY;

    // 鼠标悬停时的材质
    private Material originalMaterial;
    private Renderer tileRenderer;

    void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalMaterial = tileRenderer.material;
        }
    }

    public void Initialize(MapEditor editor, int x, int y)
    {
        mapEditor = editor;
        tileX = x;
        tileY = y;
    }
}