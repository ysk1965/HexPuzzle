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
public class Grid
{
    public int value;
    public Point point;
    PieceController piece;

    public Grid(int v, Point i)
    {
        value = v;
        point = i;
    }

    public void SetPiece(PieceController p)
    {
        piece = p;
        if (piece == null)
        {
            // piece는 null, value는 빈칸으로
            value = 0;
        }
        else
        {
            value = piece.value;
            piece.SetPoint(point);
        }
    }

    public PieceController getPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class ChangePiece
{
    public PieceController one;
    public PieceController two;

    public ChangePiece(PieceController o, PieceController t)
    {
        one = o; two = t;
    }

    public PieceController getOtherPiece(PieceController p)
    {
        if (p == one)
            return two;
        else if (p == two)
            return one;
        else
            return null;
    }
}

[System.Serializable]
public class Point
{
    public int x;
    public int y;

    public Point(int nx, int ny)
    {
        x = nx;
        y = ny;
    }

    public void mult(int m)
    {
        x *= m;
        y *= m;
    }

    public void add(Point p)
    {
        x += p.x;
        y += p.y;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public bool Equals(Point p)
    {
        return (x == p.x && y == p.y);
    }

    public static Point fromVector(Vector2 v)
    {
        return new Point((int)v.x, (int)v.y);
    }

    public static Point fromVector(Vector3 v)
    {
        return new Point((int)v.x, (int)v.y);
    }

    public static Point mult(Point p, int m)
    {
        return new Point(p.x * m, p.y * m);
    }

    public static Point add(Point p, Point o)
    {
        return new Point(p.x + o.x, p.y + o.y);
    }

    public static Point clone(Point p)
    {
        return new Point(p.x, p.y);
    }


    public static Point zero
    {
        get { return new Point(0, 0); }
    }
    public static Point one
    {
        get { return new Point(1, 1); }
    }
    public static Point up
    {
        get { return new Point(0, 2); }
    }
    public static Point down
    {
        get { return new Point(0, -2); }
    }
    public static Point rightDown
    {
        get { return new Point(1, -1); }
    }
    public static Point rightUp
    {
        get { return new Point(1, 1); }
    }
    public static Point leftDown
    {
        get { return new Point(-1, -1); }
    }
    public static Point leftUp
    {
        get { return new Point(-1, 1); }
    }
}
