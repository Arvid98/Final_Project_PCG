using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Renderer))]
public class WFCDisplay : MonoBehaviour
{
    [SerializeField]
    TileBase[] tiles;

    [SerializeField]
    TileBase defaultTile;

    [SerializeField] Color[] colors;
    [SerializeField] Color defaultColor = Color.white;

    Renderer rend;
    Texture2D texture;

    [SerializeField]
    WFC wfc;

    [SerializeField]
    Tilemap tilemap;

    bool textureHasChanged;

    void OnEnable()
    {
        NullCheck();
        wfc.OnRectChanged += OnRectChanged;
    }

    void OnDisable()
    {
        wfc.OnRectChanged -= OnRectChanged;
    }

    void NullCheck()
    {
        if (wfc == null)
            wfc = FindObjectOfType<WFC>();

        if (tilemap == null)
            tilemap = FindObjectOfType<Tilemap>();

        if (rend == null)
            rend = GetComponent<Renderer>();

        if (rend.enabled)
        {
            if (texture == null ||
                texture.width != wfc.Width ||
                texture.height != wfc.Height)
            {
                RecreateTexture();
            }
        }
    }

    void RecreateTexture()
    {
        texture = new Texture2D(wfc.Width, wfc.Height);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < texture.width; x++)
            for (int y = 0; y < texture.height; y++)
                SetPixel(x, y, defaultColor);

        rend.material.mainTexture = texture;
    }

    void FixedUpdate()
    {
        if (!textureHasChanged)
            return;

        textureHasChanged = false;
        texture.Apply();
    }

    void SetPixel(int x, int y, Color color)
    {
        textureHasChanged = true;
        texture.SetPixel(x, y, color);
    }

    void OnRectChanged(RectInt rect)
    {
        NullCheck();

        for (int ry = 0; ry < rect.height; ry++)
        {
            int y = ry + rect.y;
            HashSet<int>[] row = wfc.runner.GetRow(y);

            for (int rx = 0; rx < rect.width; rx++)
            {
                int x = rx + rect.x;

                HashSet<int> cell = row[x];
                int id = cell.Count == 1 ? cell.First() : -1;

                if (texture != null)
                {
                    Color color = defaultColor;
                    if ((uint)id < (uint)colors.Length)
                    {
                        color = colors[id];
                    }
                    SetPixel(x, y, color);
                }

                TileBase tile = defaultTile;
                if ((uint)id < (uint)tiles.Length)
                {
                    tile = tiles[id];
                }
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}
