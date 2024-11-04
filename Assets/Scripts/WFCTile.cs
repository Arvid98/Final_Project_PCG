using System;
using System.Collections.Generic;

[Serializable]
public class WFCTile
{
    // used to identify the tile
    public int id;

    // connectors of the tile, used to see which pieces can be around it
    public List<ConnectorDef> left;
    public List<ConnectorDef> right;
    public List<ConnectorDef> top;
    public List<ConnectorDef> bottom;

    // used to change the frequency of the tile
    public float weight;
}

[Serializable]
public struct ConnectorDef
{
    public int TileId;
    public float Weight;
}