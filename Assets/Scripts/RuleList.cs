using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RuleList
{
    ColCom comarer;
    HashSet<Color[]> colors;
    List<Edge> colorsList;


    public RuleList()
    {
        comarer = new ColCom();
        colors = new HashSet<Color[]>(comarer);
        colorsList = new List<Edge>();
    }
    public int GetRuleId(Color[] c)
    {
        if (colors.Add(c))
        {
            int pos = colorsList.Count;
            colorsList.Add(new Edge(c));
            return pos;
        }
        else
        {
            return colorsList.IndexOf(new Edge(c)); //stupid and stupid
        }
    }
}


class ColCom : IEqualityComparer<Color[]>
{
    public bool Equals(Color[] x, Color[] y)
    {
        int length = x.Length;
        if (length != y.Length) return false;

        for (int i = 0; i < length; i++)
        {
            if (!Mathf.Approximately(x[i].r, y[i].r) || !Mathf.Approximately(x[i].g, y[i].g) || !Mathf.Approximately(x[i].b, y[i].b)) return false;
        }

        return true;

    }

    public int GetHashCode(Color[] obj)
    {
        int length = obj.Length;
        int hash = 1;
        for (int i = 0; i < length; i++)
        {
            hash += obj[i].GetHashCode() * 31 * i;
        }
        return hash;
    }
}