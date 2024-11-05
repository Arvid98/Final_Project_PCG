using System.Collections.Generic;
using UnityEngine;

public class TileTextureList : MonoBehaviour
{

    [SerializeField] int currentTexture = 0;
    [SerializeField] private Texture2D addTexture;
    [SerializeField] private List<Texture2D> textures;
    private Texture2D textureToRender;

    public List<Texture2D> Textures => textures;

    private void OnValidate()
    {
        if(addTexture != null)
        {
            textures ??= new();
            if (!textures.Contains(addTexture))
            {
                textures.Add(addTexture);
            }    
            addTexture = null;
        }
    }

    [MakeButton]
    public void ViewTexture()
    {
        if(textureToRender == null || textureToRender.format != textures[currentTexture].format)
        {
            textureToRender = new Texture2D(textures[currentTexture].width, textures[currentTexture].height, textures[currentTexture].format,false);
            textureToRender.wrapMode = textures[currentTexture].wrapMode; // TextureWrapMode.Clamp;
            textureToRender.filterMode = textures[currentTexture].filterMode; //FilterMode.Point;
            //textureToRender.minimumMipmapLevel = textures[currentTexture].minimumMipmapLevel;
            //textureToRender.
        }

        textureToRender.SetPixels32(textures[currentTexture].GetPixels32());
        textureToRender.Apply();

        GetComponent<Renderer>().sharedMaterial.mainTexture = textureToRender;
        currentTexture = (currentTexture + 1) % textures.Count;
    }

    public bool ValidateTextures(int size)
    {
        if(Textures == null || Textures.Count == 0) {
            Debug.LogError("No Textures");
            return false; 
        }
        bool pass = true;

        for (int i = 0; i < Textures.Count; i++)
        {
            int w = Textures[i].width;
            int h = Textures[i].height;
            if (w != h)
            {
                Debug.LogError($"Texture[{i}] is not square, width  is {w} height is {h}");
                pass = false;
                continue;
            }
            if (w != size)
            {
                Debug.LogError($"Texture[{i}] differ in width, is {w} expected {size}");
                pass = false;
            }
            if (h != size)
            {
                Debug.LogError($"Texture[{i}] differ in height, is {h} expected {size}");
                pass = false;
            }
        }


        if (pass) Debug.Log($"TextureList validated succesfully, all {Textures.Count} images are {size} by {size}");
        return pass;
    }
}
