using System;

[Serializable]
public class WFCTile
{
    // used to identify the tile
    public int id;

    // connectors of the tile, used to see which pieces can be around it
    public int left;
    public int right;
    public int top;
    public int bottom;

    // used to change the frequency of the tile
    public float weight;
}
