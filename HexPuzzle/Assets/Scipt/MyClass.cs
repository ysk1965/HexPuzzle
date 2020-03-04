using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eStatus
{
    NULL,
    RED,
    GREEN,
    BLUE,
    MAGENTA,
    YELLOW,
    SILVER
}

[System.Serializable]
public class MyPoint
{
    public int x;
    public int y;

    public MyPoint(int nx, int ny)
    {
        x = nx;
        y = ny;
    }

    public void mult(int m)
    {
        x *= m;
        y *= m;
    }

    public void add(MyPoint p)
    {
        x += p.x;
        y += p.y;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public bool Equals(MyPoint p)
    {
        return (x == p.x && y == p.y);
    }

    public static MyPoint fromVector(Vector2 v)
    {
        return new MyPoint((int)v.x, (int)v.y);
    }

    public static MyPoint fromVector(Vector3 v)
    {
        return new MyPoint((int)v.x, (int)v.y);
    }

    public static MyPoint mult(MyPoint p, int m)
    {
        return new MyPoint(p.x * m, p.y * m);
    }

    public static MyPoint add(MyPoint p, MyPoint o)
    {
        return new MyPoint(p.x + o.x, p.y + o.y);
    }

    public static MyPoint clone(MyPoint p)
    {
        return new MyPoint(p.x, p.y);
    }


    public static MyPoint zero
    {
        get { return new MyPoint(0, 0); }
    }
    public static MyPoint one
    {
        get { return new MyPoint(1, 1); }
    }
    public static MyPoint up
    {
        get { return new MyPoint(0, 1); }
    }
    public static MyPoint down
    {
        get { return new MyPoint(0, -1); }
    }
    public static MyPoint right
    {
        get { return new MyPoint(1, 0); }
    }
    public static MyPoint left
    {
        get { return new MyPoint(-1, 0); }
    }
}