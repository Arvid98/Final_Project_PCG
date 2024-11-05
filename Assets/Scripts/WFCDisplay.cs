using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WFCDisplay : MonoBehaviour
{
    [SerializeField] public Sprite[] sprites;
    [SerializeField] Color[] colors;
    [SerializeField] Color defaultColor = Color.white;

    public Sprite[] Sprites => sprites;

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

        if (texture == null || texture.width != wfc.Width * sprites[0].textureRect.width || texture.height != wfc.Height * sprites[0].textureRect.height)
        {
            texture = new Texture2D(wfc.Width * (int)sprites[0].textureRect.width, wfc.Height * (int)sprites[0].textureRect.height);
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

    //void OnCellChanged(int x, int y, int id)
    //{
    //    NullCheck();

    //    Color color = defaultColor;

    //    if (id >= 0)
    //        color = colors[id];

    //    Set(x, y, color);
    //}

    void OnCellChanged(int x, int y, int id)
    {
        NullCheck();

        x *= (int)sprites[0].textureRect.width;
        y *= (int)sprites[0].textureRect.height;

        if (id >= 0)
        {
            Sprite sprite = sprites[id];

            for (int i = 0; i < sprite.textureRect.width; i++)
            {
                for (int j = 0; j < sprite.textureRect.height; j++)
                {
                    Set(x + i, y + j, sprite.texture.GetPixel((int)sprite.textureRect.x + i, (int)sprite.textureRect.y + j));
                }
            }
        }
        else
        {
            for (int i = 0; i < sprites[0].textureRect.width; i++)
            {
                for (int j = 0; j < sprites[0].textureRect.height; j++)
                {
                    Set(x + i, y + j, defaultColor);
                }
            }
        }
    }

}