using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFCDisplayCopy : MonoBehaviour
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
    public static RectInt Union(RectInt a, RectInt b)
    {
        int x1 = Math.Min(a.x, b.x);
        int x2 = Math.Max(a.x + a.width, b.x + b.width);
        int y1 = Math.Min(a.y, b.y);
        int y2 = Math.Max(a.y + a.height, b.y + b.height);
        return new RectInt(x1, y1, x2 - x1, y2 - y1);
    }

    void Update()
    {
        NullCheck();

        if (textureHasChanged)
        {
            textureHasChanged = false;
            texture.Apply();
        }

        if (changedRects.Count > 0)
        {
            RectInt bounds = changedRects[0];
            foreach (RectInt rect in changedRects)
            {
                bounds = Union(bounds, rect);
            }

            RefreshTiles(bounds);
            changedRects.Clear();

            Debug.Log("Refreshing " + bounds);
        }
    }

    void SetPixel(int x, int y, Color color)
    {
        textureHasChanged = true;
        texture.SetPixel(x, y, color);
    }

    List<RectInt> changedRects = new();
    
    void OnRectChanged(RectInt rect)
    {
        changedRects.Add(rect);
    }

    void RefreshTiles(RectInt rect)
    {
        TileBase[] tileBlock = new TileBase[rect.width * rect.height];
        Array.Fill(tileBlock, defaultTile);

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

                        if ((uint)id < (uint)tiles.Length)
                        {
                            TileBase tile = tiles[id];
                            tileBlock[ry * rect.width + rx] = tile;
                        }
                    }
                }

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

        tilemap.SetTilesBlock(new BoundsInt(new Vector3Int(rect.x, rect.y, 0), new Vector3Int(rect.width, rect.height, 1)), tileBlock);
    }
}
