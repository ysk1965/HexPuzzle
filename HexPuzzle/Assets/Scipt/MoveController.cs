using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour
{
    public static MoveController instance;
    MatchController game;

    PieceController moving;
    Point newPoint;
    Vector2 mouseStart;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        game = GetComponent<MatchController>();
    }

    void Update()
    {
        if (moving != null)
        {
            Vector2 dir = ((Vector2)Input.mousePosition - mouseStart);
            Vector2 nDir = dir.normalized;
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            newPoint = Point.clone(moving.point);
            Point add = Point.zero;
            if (dir.magnitude > 32) //If our mouse is 32 pixels away from the starting point of the mouse
            {
                // 6방향 dir
                if (aDir.y > aDir.x)
                    add = (new Point(0, (nDir.y < 0) ? 2 : -2));
                else if (aDir.x > aDir.y)
                    add = (new Point((nDir.x < 0) ? -1 : 1, (nDir.y < 0) ? 1 : -1));
            }
            newPoint.add(add);

            Vector2 pos = game.getPositionFromPoint(moving.point);
            if (!newPoint.Equals(moving.point))
                pos += Point.mult(new Point(add.x, -add.y), 50).ToVector();
            moving.MovePositionTo(pos);
        }
    }

    public void MovePiece(PieceController piece)
    {
        if (moving != null) return;
        moving = piece;
        mouseStart = Input.mousePosition;
    }

    public void DropPiece()
    {
        if (moving == null) return;
        if (!newPoint.Equals(moving.point))
            game.FlipPieces(moving.point, newPoint, true);
        else
            game.ResetPiece(moving);
        moving = null;
    }
}
