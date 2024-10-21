using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WFCDisplay : MonoBehaviour
{
    [SerializeField] Color[] colors;
    [SerializeField] Color defaultColor = Color.white;

    Renderer rend;
    Texture2D texture;
    WFC wfc;

    bool textureHasChanged;

    void OnEnable()
    {
        NullCheck();
        wfc.OnCellChanged += OnCellChanged;
    }
    void OnDisable()
    {
        wfc.OnCellChanged -= OnCellChanged;
    }

    void NullCheck()
    {
        if (wfc == null)
            wfc = FindObjectOfType<WFC>();

        if (rend == null)
            rend = GetComponent<Renderer>();

        if (texture == null || texture.width != wfc.Width || texture.height != wfc.Height)
        {
            texture = new Texture2D(wfc.Width, wfc.Height);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;

            for (int x = 0; x < texture.width; x++)
                for (int y = 0; y < texture.height; y++)
                    Set(x, y, defaultColor);

            rend.sharedMaterial.mainTexture = texture;
        }
    }

    void FixedUpdate()
    {
        if (!textureHasChanged)
            return;

        textureHasChanged = false;
        texture.Apply();
    }

    void Set(int x, int y, Color color)
    {
        textureHasChanged = true;
        texture.SetPixel(x, y, color);
    }

    void OnCellChanged(int x, int y, int id)
    {
        NullCheck();

        Color color = defaultColor;

        if (id >= 0)
            color = colors[id];

        Set(x, y, color);
    }
}
