using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WFCTile
{
    public Material material;
    public List<Material> left;
    public List<Material> right;
    public List<Material> top;
    public List<Material> bottom;
    public float weight;
}
