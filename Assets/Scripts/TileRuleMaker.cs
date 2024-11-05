using System;
using System.Collections.Generic;
using UnityEngine;

public class TileRuleMaker : MonoBehaviour
{
    [SerializeField] private Renderer m_Renderer;
    public Renderer Renderer => m_Renderer ? m_Renderer : m_Renderer = GetComponent<Renderer>();


    HashSet<Color[]> colors;
    RuleList ruleList;
    [SerializeField] List<WFCTile> tiles = new();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [MakeButton]
    public void AddAllToList()
    {
        TextureArrayList list = GetComponent<TextureArrayList>();
        TileTextureList tiles = GetComponent<TileTextureList>();

        for (int i = 0; i < tiles.Textures.Count; i++)
        {



            Texture2D tex = CopyTexture(tiles.Textures[i]);

            int n = list.Count;

            list.Add(tex); //add as is
                           //add rotations
            list.Add(ManipulateTexture(tex, Rotate90));
            list.Add(ManipulateTexture(tex, Rotate90));
            list.Add(ManipulateTexture(tex, Rotate90));

            //currently tex is at 270 deg rot

            //flip horizontaly and try its rotations
            tex = CopyTexture(tiles.Textures[i]); //reset
            list.Add(ManipulateTexture(tex, FlipH));
            list.Add(ManipulateTexture(tex, Rotate90));
            list.Add(ManipulateTexture(tex, Rotate90));
            list.Add(ManipulateTexture(tex, Rotate90));

            //flip vertically and try its rotations
            tex = CopyTexture(tiles.Textures[i]); //reset
            list.Add(ManipulateTexture(tex, FlipV));
            list.Add(ManipulateTexture(tex, Rotate90));
            list.Add(ManipulateTexture(tex, Rotate90));
            list.Add(ManipulateTexture(tex, Rotate90));

            int diff = (list.Count - n);

            float weight = 1.0f / (diff > 0 ? diff : 1);

            //this finds all versions of a texture, and weight.. but do not bind it to anything
            //each successfull add should be recorded and edges(rules) added for that id..



        }
    }

    [MakeButton]
    public void InjectRules()
    {
        var m = FindAnyObjectByType<WFC>();
        m.Tiles = tiles.ToArray();
       // m.OnCellChanged
    }

    [MakeButton]
    public void InjectSprites()
    {
        var m = FindAnyObjectByType<WFCDisplay>();
        var ta = FindAnyObjectByType<TextureArrayList>();
        Sprite[] sprites = new Sprite[ta.Count];

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i] = Sprite.Create(ta.List[i], new Rect(0,0,5,5), Vector2.zero);
        }
        m.sprites = sprites;
    }

    [MakeButton]
    public void AddToList()
    {
        TextureArrayList list = GetComponent<TextureArrayList>();



        Texture2D tex = CopyTexture(Renderer.sharedMaterial.mainTexture as Texture2D);

        int n = list.Count;

        list.Add(tex); //add as is
        //add rotations
        list.Add(ManipulateTexture(tex, Rotate90));
        list.Add(ManipulateTexture(tex, Rotate90));
        list.Add(ManipulateTexture(tex, Rotate90));

        //currently tex is at 270 deg rot

        //flip horizontaly and try its rotations
        tex = CopyTexture(Renderer.sharedMaterial.mainTexture as Texture2D); //reset
        list.Add(ManipulateTexture(tex,FlipH));
        list.Add(ManipulateTexture(tex, Rotate90));
        list.Add(ManipulateTexture(tex, Rotate90));
        list.Add(ManipulateTexture(tex, Rotate90));

        //flip vertically and try its rotations
        tex = CopyTexture(Renderer.sharedMaterial.mainTexture as Texture2D); //reset
        list.Add(ManipulateTexture(tex, FlipV));
        list.Add(ManipulateTexture(tex, Rotate90));
        list.Add(ManipulateTexture(tex, Rotate90));
        list.Add(ManipulateTexture(tex, Rotate90));

        int diff = (list.Count - n);

        float weight = 1.0f / (diff > 0 ? diff : 1);

        //this finds all versions of a texture, and weight.. but do not bind it to anything
        //each successfull add should be recorded and edges(rules) added for that id..

       

    }


    [MakeButton]

    public void AddRules()
    {
        TextureArrayList tlist = GetComponent<TextureArrayList>();
        List<Texture2D> list = tlist.List;
        ruleList = new RuleList();
        tiles = new();
        for (int i = 0; i < list.Count; i++)
        {
            WFCTile tile = new WFCTile();
            tile.id = i;
            tile.weight = 1;
            CreateRules(list[i], tile);
            tiles.Add(tile);
        }
    }



    public Texture2D CopyTexture(Texture2D toCopy)
    {
        Texture2D tex = new Texture2D(toCopy.width, toCopy.height, toCopy.format, false);
        tex.wrapMode = toCopy.wrapMode; // TextureWrapMode.Clamp;
        tex.filterMode = toCopy.filterMode; //FilterMode.Point;

        tex.SetPixels32(toCopy.GetPixels32());
        tex.Apply();

        return tex;
    }

    [MakeButton]
    public void RotateTexture()
    {
        Texture2D texture = Renderer.sharedMaterial.mainTexture as Texture2D;

        Debug.Assert(texture != null);
        Debug.Assert(texture.height == texture.width);

        Color[] pixels = texture.GetPixels();
        int size = texture.height;
        texture.SetPixels(Rotate90(pixels, size));
        //texture.filterMode = FilterMode.Point;
        //texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        Renderer.sharedMaterial.mainTexture = texture;
    }

    [MakeButton]
    public void FlipTextureHorizontaly()
    {
        ManipulateTexture(FlipH);
    }

    [MakeButton]
    public void FlipTextureVertically()
    {
        ManipulateTexture(FlipV);
    }

    public void ManipulateTexture(Func<Color[],int, Color[]> action)
    {
        Texture2D texture = Renderer.sharedMaterial.mainTexture as Texture2D;

        Debug.Assert(texture != null);
        Debug.Assert(texture.height == texture.width);

        Color[] pixels = texture.GetPixels();
        int size = texture.height;

        texture.SetPixels(action.Invoke(pixels, size));
        //texture.filterMode = FilterMode.Point;
        //texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        Renderer.sharedMaterial.mainTexture = texture;
    }

    public Texture2D ManipulateTexture(Texture2D texture, Func<Color[], int, Color[]> action)
    {
        Color[] pixels = texture.GetPixels();
        int size = texture.height;
        texture.SetPixels(action.Invoke(pixels, size));
        texture.Apply();
        return texture;
    }

    public Texture2D ManipulateTexture(Texture2D texture, params Func<Color[], int, Color[]>[] actions)
    {
        Color[] pixels = texture.GetPixels();
        int size = texture.height;
        int length = actions.Length;
        Color[] c = actions[0].Invoke(pixels, size);
        for (int i = 1; i < length; i++)
        {
            c = actions[i].Invoke(c, size);
        }

        texture.SetPixels(c);
        texture.Apply();
        return texture;
    }


    public void CreateRules(Texture2D texture, WFCTile tile)
    {
        Color[] bottom = texture.GetPixels(0, 0, texture.width, 1);
        Color[] top = texture.GetPixels(0, texture.height-1, texture.width, 1);
        Color[] left = texture.GetPixels(0, 0, 1, texture.height);
        Color[] right = texture.GetPixels(texture.width - 1, 0, 1, texture.height);

        tile.right = ruleList.GetRuleId(right);
        tile.left = ruleList.GetRuleId(left);
        tile.top = ruleList.GetRuleId(top);
        tile.bottom = ruleList.GetRuleId(bottom);



    }

    public Color[] Rotate90(Color[] pixels, int size)
    {
        Color[] colors = new Color[pixels.Length];
        Rotate90(pixels, size, ref colors);
        return colors;
    }

    public static void Rotate90(Color[] pixels, int size, ref Color[] result)
    {
        Debug.Assert(pixels.Length == result.Length);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i0 = x * size + (size - y - 1);

                result[i0] = pixels[y * size + x];
            }
        }
    }


    public Color[] FlipH(Color[] pixels, int size)
    {
        Color[] colors = new Color[pixels.Length];
        FlipH(pixels, size, ref colors);
        return colors;
    }

    public static void FlipH(Color[] pixels, int size, ref Color[] result)
    {
        Debug.Assert(pixels.Length == result.Length);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i0 = y * size + (size - x - 1);

                result[i0] = pixels[y * size + x];
            }
        }
    }

    public Color[] FlipV(Color[] pixels, int size)
    {
        Color[] colors = new Color[pixels.Length];
        FlipV(pixels, size, ref colors);
        return colors;
    }

    public static void FlipV(Color[] pixels, int size, ref Color[] result)
    {
        Debug.Assert(pixels.Length == result.Length);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int i0 = size*(size - 1) - y * size + x;

                result[i0] = pixels[y * size + x];
            }
        }
    }
}

public class Edge /*: IComparable<Edge>*/
{
    Color[] edgeArray;
    public Edge(Color[] edgeArray)
    {
        this.edgeArray = edgeArray;
    }

    public static bool operator ==(Edge lhs, Edge rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Edge lhs, Edge rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override bool Equals(object obj)
    {
        if(obj is Edge other)
        {
            int len = edgeArray.Length;
            if (len != other.edgeArray.Length)
            {
                return false;
            }
            for (int i = 0; i < len; i++)
            {
                if(other.edgeArray[i] != edgeArray[i])
                {
                    return false;
                }
            }
            return true;
        }
        
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    //public int CompareTo(Edge other)
    //{
    //    int l = edgeArray.Length.CompareTo(other.edgeArray.Length);
    //    if(l != 0) return l;

    //    for (int i = 0; i < edgeArray.Length; i++)
    //    {
    //        edgeArray[i].
    //        int c = edgeArray[i].CompareTo(other.edgeArray[i]);
    //        if (c != 0) return l;
    //    }
    //    return 0;
    //}
}

public readonly struct TileData
{
    public readonly int data;

    public readonly static int vFlipBit = 0x0100;
    public readonly static int hFlipBit = 0x0200;
    public readonly static int rotMask = 0xF000;
    //public readonly static int rot0 = 0x0000;
    //public readonly static int rot90 = 0x1000;
    //public readonly static int rot180 = 0x2000;
    //public readonly static int rot270 = 0x3000;
    public readonly static int texIdMask = 0x00FF;

    public TileData(int texId, bool vFlip, bool hFlip, int rot)
    {
        int i = texId & 0x00FF;
        i |= (vFlip ? vFlipBit : 0);
        i |= (hFlip ? hFlipBit : 0);
        i |= (((rot/90) << 24) & texIdMask);
        data = i;
    }

    public int TexId => data & texIdMask;
    public int Rot => (data >> 24) * 90;
    public bool VFlip => (data & vFlipBit) != 0;
    public bool HFlip => (data & hFlipBit) != 0;
}
