using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTextureList : MonoBehaviour
{

    [SerializeField] int currentTexture = 0;
    [SerializeField] private Texture2D addTexture;
    [SerializeField] private List<Texture2D> textures;

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
        GetComponent<Renderer>().sharedMaterial.mainTexture = textures[currentTexture];
        currentTexture = (currentTexture + 1) % textures.Count;
    }
}
