using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    PieceController pieceController;
    Vector2 mouseStart;
    MyPoint newPoint;

    [Header("UI Elements")]
    public RectTransform gameBoard;
    public RectTransform killedBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killedPiece;

    int width = BoardController.boardArray.GetLength(0);
    int height = BoardController.boardArray.GetLength(1);
    int[] fills;
    Piece[,] puzzleBoard;

    List<PieceController> update;
    List<Changed> flipped;
    List<PieceController> dead;
    //List<KilledPiece> killed; //[CHANGE]

    System.Random random;

    int enumLength = System.Enum.GetValues(typeof(eStatus)).Length;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (pieceController != null)
        {
            Vector2 dir = ((Vector2)Input.mousePosition - mouseStart);
            Vector2 nDir = dir.normalized;
            Vector2 aDir = new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            newPoint = MyPoint.clone(pieceController.piecePoint);
            MyPoint add = MyPoint.zero;
            if (dir.magnitude > -32) //If our mouse is 32 pixels away from the starting point of the mouse
            {
                //make add either (1, 0) | (-1, 0) | (0, 1) | (0, -1) depending on the direction of the mouse point
                if (aDir.x > aDir.y)
                    add = (new MyPoint((nDir.x > 0) ? 1 : -1, 0));
                else if (aDir.y > aDir.x)
                    add = (new MyPoint(0, (nDir.y > 0) ? -1 : 1));
            }

            Vector2 pos = getPositionFromPoint(pieceController.piecePoint);
            if (!newPoint.Equals(pieceController.piecePoint)) {
                Debug.Log("NewPoint");
                pos += MyPoint.mult(new MyPoint(add.x, -add.y), 50).ToVector();
            }
            pieceController.MovePositionTo(pos);
        }

        // Update NodePiece
        List<PieceController> finishedUpdating = new List<PieceController>();
        for (int i = 0; i < update.Count; i++)
        {
            Debug.LogError("????");
            PieceController piece = update[i];
            if (!piece.UpdatePiece()) finishedUpdating.Add(piece);
        }
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            Debug.LogError("????");
            PieceController piece = finishedUpdating[i];
            Changed flip = getFlipped(piece);
            PieceController flippedPiece = null;

            int x = (int)piece.piecePoint.x;
            fills[x] = Mathf.Clamp(fills[x] - 1, 0, width);

            List<MyPoint> connected = isConnected(piece.piecePoint, true);
            bool wasFlipped = (flip != null);

            if (wasFlipped) //If we flipped to make this update
            {
                flippedPiece = flip.getOtherPiece(piece);
                AddPoints(ref connected, isConnected(flippedPiece.piecePoint, true));
            }

            if (connected.Count == 0) //If we didn't make a match
            {
                Debug.LogError("didn`t make a match");
                if (wasFlipped) //If we flipped
                    FlipPieces(piece.piecePoint, flippedPiece.piecePoint, false); //Flip back
            }
            else //If we made a match
            {
                foreach (MyPoint pnt in connected) //Remove the node pieces connected
                {
                    //KillPiece(pnt); //[CHANGE]
                    Piece node = getNodeAtPoint(pnt);
                    PieceController nodePiece = node.getPiece();
                    if (nodePiece != null)
                    {
                        nodePiece.gameObject.SetActive(false);
                        dead.Add(nodePiece);
                    }
                    node.SetPiece(null);
                }

                ApplyGravityToBoard();
            }

            flipped.Remove(flip); //Remove the flip after update
            update.Remove(piece);
        }
    }

    public void MovePiece(PieceController piece)
    {
        if (pieceController != null) return;
        pieceController = piece;
        mouseStart = Input.mousePosition;
    }

    public void DropPiece()
    {
        if (pieceController == null) return;
        Debug.Log("Dropped");
        if (!newPoint.Equals(pieceController.piecePoint))
            FlipPieces(pieceController.piecePoint, newPoint, true);
        else
            ResetPiece(pieceController);
        pieceController = null;
    }


    /////////////////////////////////////////////////////////////////
    ///

    void Start()
    {
        StartGame();
    }

    public void ApplyGravityToBoard()
    {
        Debug.LogError("ApplyGravityToBoard");
        for (int x = 0; x < width; x++)
        {
            for (int y = (height - 1); y >= 0; y--) //Start at the bottom and grab the next
            {
                MyPoint p = new MyPoint(x, y);
                Piece node = getNodeAtPoint(p);
                int val = getValueAtPoint(p);
                if (val != 0) continue; //If not a hole, move to the next
                for (int ny = (y - 1); ny >= -1; ny--)
                {
                    MyPoint next = new MyPoint(x, ny);
                    int nextVal = getValueAtPoint(next);
                    if (nextVal == 0)
                        continue;
                    if (nextVal != -1)
                    {
                        Piece gotten = getNodeAtPoint(next);
                        PieceController piece = gotten.getPiece();

                        //Set the hole
                        node.SetPiece(piece);
                        update.Add(piece);

                        //Make a new hole
                        gotten.SetPiece(null);
                    }
                    else//Use dead ones or create new pieces to fill holes (hit a -1) only if we choose to
                    {
                        int newVal = fillPiece();
                        PieceController piece;
                        MyPoint fallPnt = new MyPoint(x, (-1 - fills[x]));
                        if (dead.Count > 0)
                        {
                            PieceController revived = dead[0];
                            revived.gameObject.SetActive(true);
                            piece = revived;

                            dead.RemoveAt(0);
                        }
                        else
                        {
                            GameObject obj = Instantiate(nodePiece, gameBoard);
                            PieceController n = obj.GetComponent<PieceController>();
                            piece = n;
                        }

                        piece.Initialize(newVal, p);
                        piece.rect.anchoredPosition = getPositionFromPoint(fallPnt);

                        Piece hole = getNodeAtPoint(p);
                        hole.SetPiece(piece);
                        ResetPiece(piece);
                        fills[x]++;
                    }
                    break;
                }
            }
        }
    }

    Changed getFlipped(PieceController p)
    {
        Changed flip = null;
        for (int i = 0; i < flipped.Count; i++)
        {
            if (flipped[i].getOtherPiece(p) != null)
            {
                flip = flipped[i];
                break;
            }
        }
        return flip;
    }

    void StartGame()
    {
        fills = new int[width];
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<PieceController>();
        flipped = new List<Changed>();
        dead = new List<PieceController>();
        //killed = new List<KilledPiece>(); [CHANGE]

        initBoard();
        VerifyBoard();
        InstantiateBoard();
    }

    void initBoard()
    {
        puzzleBoard = new Piece[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                puzzleBoard[x, y] = new Piece((BoardController.boardArray[x,y]) ? fillPiece() : -1, new MyPoint(x, y));
            }
        }
    }

    void VerifyBoard()
    {
        List<int> remove;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                MyPoint p = new MyPoint(x, y);
                int val = getValueAtPoint(p);
                if (val <= 0) continue;

                remove = new List<int>();
                while (isConnected(p, true).Count > 0)
                {
                    val = getValueAtPoint(p);
                    if (!remove.Contains(val))
                        remove.Add(val);
                    setValueAtPoint(p, newValue(ref remove));
                }
            }
        }
    }

    void InstantiateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                 Piece node = getNodeAtPoint(new MyPoint(x, y));
                 
                 int val = node.value;
                 if (val <= 0) continue;
                 GameObject p = Instantiate(nodePiece, gameBoard);
                 PieceController piece = p.GetComponent<PieceController>();
                 RectTransform rect = p.GetComponent<RectTransform>();
                 rect.anchoredPosition = new Vector2(-360 + (120 * x),- 400 + (70 * y));
                 piece.Initialize(val, new MyPoint(x, y));
                 node.SetPiece(piece);
            }
        }
    }

    public void ResetPiece(PieceController piece)
    {
        Debug.LogError("Reset");
        piece.ResetPosition();
        update.Add(piece);
    }

    public void FlipPieces(MyPoint one, MyPoint two, bool main)
    {
        if (getValueAtPoint(one) < 0) return;

        Piece nodeOne = getNodeAtPoint(one);
        PieceController pieceOne = nodeOne.getPiece();
        if (getValueAtPoint(two) > 0)
        {
            Piece nodeTwo = getNodeAtPoint(two);
            PieceController pieceTwo = nodeTwo.getPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if (main)
                flipped.Add(new Changed(pieceOne, pieceTwo));

            update.Add(pieceOne);
            update.Add(pieceTwo);
        }
        else
            ResetPiece(pieceOne);
    }

    //void KillPiece(MyPoint p)
    //{
    //    List<KilledPiece> available = new List<KilledPiece>();
    //    for (int i = 0; i < killed.Count; i++)
    //        if (!killed[i].falling) available.Add(killed[i]);

    //    KilledPiece set = null;
    //    if (available.Count > 0)
    //        set = available[0];
    //    else
    //    {
    //        GameObject kill = GameObject.Instantiate(killedPiece, killedBoard);
    //        KilledPiece kPiece = kill.GetComponent<KilledPiece>();
    //        set = kPiece;
    //        killed.Add(kPiece);
    //    }

    //    int val = getValueAtPoint(p) - 1;
    //    if (set != null && val >= 0 && val < pieces.Length)
    //        set.Initialize(pieces[val], getPositionFromPoint(p));
    //}

    List<MyPoint> isConnected(MyPoint p, bool main)
    {
        List<MyPoint> connected = new List<MyPoint>();
        int val = getValueAtPoint(p);
        MyPoint[] directions =
        {
            MyPoint.up,
            MyPoint.right,
            MyPoint.down,
            MyPoint.left
        };

        foreach (MyPoint dir in directions) //Checking if there is 2 or more same shapes in the directions
        {
            List<MyPoint> line = new List<MyPoint>();

            int same = 0;
            for (int i = 1; i < 3; i++)
            {
                MyPoint check = MyPoint.add(p, MyPoint.mult(dir, i));
                if (getValueAtPoint(check) == val)
                {
                    line.Add(check);
                    same++;
                }
            }

            if (same > 1) //If there are more than 1 of the same shape in the direction then we know it is a match
                AddPoints(ref connected, line); //Add these points to the overarching connected list
        }

        for (int i = 0; i < 2; i++) //Checking if we are in the middle of two of the same shapes
        {
            List<MyPoint> line = new List<MyPoint>();

            int same = 0;
            MyPoint[] check = { MyPoint.add(p, directions[i]), MyPoint.add(p, directions[i + 2]) };
            foreach (MyPoint next in check) //Check both sides of the piece, if they are the same value, add them to the list
            {
                if (getValueAtPoint(next) == val)
                {
                    line.Add(next);
                    same++;
                }
            }

            if (same > 1)
                AddPoints(ref connected, line);
        }

        for (int i = 0; i < 4; i++) //Check for a 2x2
        {
            List<MyPoint> square = new List<MyPoint>();

            int same = 0;
            int next = i + 1;
            if (next >= 4)
                next -= 4;

            MyPoint [] check = { MyPoint.add(p, directions[i]), MyPoint.add(p, directions[next]), MyPoint.add(p, MyPoint.add(directions[i], directions[next])) };
            foreach (MyPoint pnt in check) //Check all sides of the piece, if they are the same value, add them to the list
            {
                if (getValueAtPoint(pnt) == val)
                {
                    square.Add(pnt);
                    same++;
                }
            }

            if (same > 2)
                AddPoints(ref connected, square);
        }

        if (main) //Checks for other matches along the current match
        {
            for (int i = 0; i < connected.Count; i++)
                AddPoints(ref connected, isConnected(connected[i], false));
        }

        /* UNNESSASARY | REMOVE THIS!
        if (connected.Count > 0)
            connected.Add(p);
        */

        return connected;
    }

    void AddPoints(ref List<MyPoint> points, List<MyPoint> add)
    {
        foreach (MyPoint p in add)
        {
            bool doAdd = true;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Equals(p))
                {
                    doAdd = false;
                    break;
                }
            }

            if (doAdd) points.Add(p);
        }
    }

    int fillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / enumLength)) + 1;
        return val;
    }

    int getValueAtPoint(MyPoint p)
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return -1;
        return puzzleBoard[p.x, p.y].value;
    }

    void setValueAtPoint(MyPoint p, int v)
    {
        puzzleBoard[p.x, p.y].value = v;
    }

    Piece getNodeAtPoint(MyPoint p)
    {
        return puzzleBoard[p.x, p.y];
    }

    int newValue(ref List<int> remove)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < enumLength; i++)
            available.Add(i + 1);
        foreach (int i in remove)
            available.Remove(i);

        if (available.Count <= 0) return 0;
        return available[random.Next(0, available.Count)];
    }

    string getRandomSeed()
    {
        string seed = "";
        string acceptableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdeghijklmnopqrstuvwxyz1234567890!@#$%^&*()";
        for (int i = 0; i < 20; i++)
            seed += acceptableChars[Random.Range(0, acceptableChars.Length)];
        return seed;
    }

    public Vector2 getPositionFromPoint(MyPoint p)
    {
        return new Vector2(-360 + (120 * p.x), -400 - (70 * p.y));
    }
}

[System.Serializable]
public class Piece
{
    public int value; //0 = blank, 1 = cube, 2 = sphere, 3 = cylinder, 4 = pryamid, 5 = diamond, -1 = hole
    public MyPoint index;
    PieceController piece;

    public Piece(int v, MyPoint i)
    {
        value = v;
        index = i;
    }

    public void SetPiece(PieceController p)
    {
        piece = p;
        value = (piece == null) ? 0 : piece.value;
        if (piece == null) return;
        piece.SetIndex(index);
    }

    public PieceController getPiece()
    {
        return piece;
    }
}

[System.Serializable]
public class Changed
{
    public PieceController one;
    public PieceController two;

    public Changed(PieceController o, PieceController t)
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