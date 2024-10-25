using UnityEngine;

public class TileRuleMaker : MonoBehaviour
{
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [MakeButton]
    public void RotateTexture()
    {
        Color[] pixels = (GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D).GetPixels();
        Texture2D texture = new Texture2D(5,5);
        texture.SetPixels(Rotate90(pixels, 5));
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();
        GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
    }


    public void CreateRules(Texture2D texture)
    {
        Color[] top = texture.GetPixels(0, 0, texture.width, 1);
        Color[] bottom = texture.GetPixels(0, texture.height-1, texture.width, 1);
        Color[] left = texture.GetPixels(0, 0, 1, texture.height);
        Color[] right = texture.GetPixels(texture.width - 1, 0, 1, texture.height);
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
}

public class Edge
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
}
