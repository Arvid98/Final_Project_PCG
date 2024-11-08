using System;
using UnityEngine;

[Serializable]
public struct Point
{
    public int x;
    public int y;

    public float SquareMagnitude => Mathf.Abs(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
    public float Magnitude => Mathf.Sqrt(SquareMagnitude);

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.x + p2.x, p1.y + p2.y);
    }

    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.x - p2.x, p1.y - p2.y);
    }

    public static Point operator -(Point p)
    {
        return new Point(-p.x, -p.y);
    }

    public static bool operator ==(Point p1, Point p2)
    {
        if (p1.x != p2.x || p1.y != p2.y)
            return false;

        return true;

        //return p1.x == p2.x && p1.y == p2.y;
    }

    public static bool operator !=(Point p1, Point p2)
    {
        if (p1.x == p2.x || p1.y == p2.y)
            return false;

        return true;

        //return !(p1 == p2);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}
