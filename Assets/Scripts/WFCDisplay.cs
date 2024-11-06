using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCDisplay : MonoBehaviour
{
    [SerializeField]
    public TileBase[] tiles;

    [SerializeField]
    public TileBase defaultTile;

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

    void Update()
    {
        NullCheck();

        if (textureHasChanged)
        {
            textureHasChanged = false;
            texture.Apply();
        }
    }

    void SetPixel(int x, int y, Color color)
    {
        textureHasChanged = true;
        texture.SetPixel(x, y, color);
    }

    void OnRectChanged(RectInt rect)
    {
        for (int ry = 0; ry < rect.height; ry++)
        {
            int y = ry + rect.y;
            HashSet<int>[] cellRow = wfc.runner.GetCellRow(y);
            BitArray collapsedRow = wfc.runner.GetCollapsedRow(y);

            for (int rx = 0; rx < rect.width; rx++)
            {
                int x = rx + rect.x;

                int id = -1;

                if (collapsedRow.Get(x))
                {
                    HashSet<int> cell = cellRow[x];
                    if (cell.Count > 0)
                    {
                        var e = cell.GetEnumerator();
                        e.MoveNext();
                        id = e.Current;
                    }
                }

                TileBase tile = defaultTile;
                if ((uint)id < (uint)tiles.Length)
                {
                    tile = tiles[id];
                }
                tilemap.SetTile(new Vector3Int(x, y, 0), tile);

                if (texture != null)
                {
                    Color color = defaultColor;
                    if ((uint)id < (uint)colors.Length)
                    {
                        color = colors[id];
                    }
                    SetPixel(x, y, color);
                }
            }
        }
    }
}
