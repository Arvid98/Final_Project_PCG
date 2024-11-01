using System.Collections.Generic;
using UnityEngine;

public class TextureArrayList : MonoBehaviour
{
    [SerializeField] private List<Texture2D> list = new List<Texture2D>();

    public int Count => list.Count;  
    public List<Texture2D> List => list;

    [MakeButton]
    public void ClearList()
    {
        list.Clear();
    }

    public int Add(Texture2D texture)
    {
        list ??= new();
        int i;
        for (i = 0; i < list.Count; i++)
        {
            if(Compare(texture, list[i]))
            {
                //Debug.Log("already exists at " + i);
                return i; //already exists at i
            }
        }



        list.Add(CopyTexture(texture));
        return i;
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

    public static bool Compare(Texture2D t1, Texture2D t2)
    {
        return Compare(t2.GetPixels32(), t1.GetPixels32());
    }

    public static bool Compare(Color32[] c1, Color32[] c2)
    {
        int length = c1.Length;
        if (length != c2.Length) return false;
        for (int i = 0; i < length; i++)
        {
            if (c1[i].r != c2[i].r || c1[i].g != c2[i].g || c1[i].b != c2[i].b) return false;
        }
        return true;
    }
}
