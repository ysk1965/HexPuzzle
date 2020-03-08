using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform gameBoard;
    public RectTransform killedBoard;

    [Header("Prefabs")]
    public GameObject nodePiece;
    public GameObject killedPiece;

    int width = BoardController.boardArray.GetLength(1);
    int height = BoardController.boardArray.GetLength(0);
    Grid[,] puzzleBoard;

    List<PieceController> update;
    List<ChangePiece> flipped;
    List<PieceController> dead;

    System.Random random;
    public static bool bUpdateLogic = false;

    int enumLength = System.Enum.GetValues(typeof(eStatus)).Length;

    public static int anchor_x = -460;
    public static int anchor_y = 300;

    void Update()
    {
        for (int i = 0; i < update.Count; i++)
        {
            //bUpdateLogic = true;
            PieceController piece = update[i];
            if (piece) piece.UpdatePiece();
        }

        //if (update.Count == 0) bUpdateLogic = false;
    }

    void Start()
    {
        StartGame();
    }

    public void SearchConnectPoint(Grid currentNode)
    {
        Point[] directions = {
            Point.up,
            Point.rightUp,
            Point.rightDown,
            Point.down,
            Point.leftDown,
            Point.leftUp
        };

        // 방향에 따라서 몇개나 연결되어 있나 확인
        Point currentPoint = currentNode.point;

        if (getValueAtPoint(currentPoint) != -1)
        {
            // cnt가 2이상이면 3개 이상 연결된 상황.
            foreach (Point dir in directions)
            {
                // 첫 칸 이동해서 value가 맞는지 확인
                Point nextPoint = Point.add(currentPoint, Point.mult(dir, 1));
                int connectCnt = 0;
                // 해당 direction으로 value가 틀릴 때까지 찾아봐
                while (currentNode.value == getValueAtPoint(nextPoint))
                {
                    connectCnt += 1;
                    nextPoint = Point.add(currentPoint, Point.mult(dir, connectCnt + 1)); // 카운트를 하나씩 증가하면서 체크
                }

                if (dir.Equals(Point.rightUp) || dir.Equals(Point.leftDown))
                {
                    currentNode.getPiece().cntCrossUp += connectCnt;
                }
                if (dir.Equals(Point.rightDown) || dir.Equals(Point.leftUp))
                {
                    currentNode.getPiece().cntCrossDown += connectCnt;
                }
                if (dir.Equals(Point.up) || dir.Equals(Point.down))
                {
                    currentNode.getPiece().cntStraight += connectCnt;
                }
            }
        }
    }

    public void ApplyGravityToBoard()
    {
        Point[] directions = {
            Point.rightUp,
            Point.leftUp
        };

        // 아래서부터 비어있는 공간 찾아서 중력검사 (빈 칸이 생긴 만큼) 
        // --> Coroutine으로 돌아야지 순차적으로 이동 되는게 보일듯 플립 모두 완료 되면 다음 루틴으로!
        for (int y = (height - 1); y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Grid checkNode = getNodeAtPoint(new Point(x, y));
                if (checkNode.value == -1) continue; // 검사할 필요 없는 Grid

                // 비어있는 애들만 체크 하면 됨.
                if (checkNode.value == 0)
                {
                    // 1. 바로 윗칸을 먼저 체크 (height가 아래서 부터 올라감 (역방향))
                    Point checkDirPoint = Point.add(checkNode.point, Point.mult(Point.up, -1));
                    Grid checkDirNode = getNodeAtPoint(checkDirPoint);

                    if (checkNode.point.y == 0)
                    {
                        Debug.LogError(checkNode.point.x + "_" + checkNode.point.y + "_최초공간!");
                        checkNode.value = fillPiece();
                        settingPiece(checkNode);

                        continue;
                    }

                    Debug.LogError("[" + checkNode.value + "]" + checkNode.point.x + "_" + checkNode.point.y + "_이번에 체크할 Node");
                    if (checkDirNode == null) // Top, 가장 윗부분에 접근 (만들어져야할 곳)
                    {
                        foreach (Point dir in directions)
                        {
                            checkDirPoint = Point.add(checkNode.point, Point.mult(dir, 1));
                            checkDirNode = getNodeAtPoint(checkDirPoint);
                            if (checkDirPoint.y >= 0) Debug.LogError("[" + checkDirNode.value + "]" + "checkDirPoint" + checkDirPoint.x + "," + checkDirPoint.y);

                            if (checkDirNode != null && checkDirNode.value > 0) // 유효한 piece가 있다면
                            {
                                FlipPieces(checkNode.point, checkDirPoint, true);
                                break;
                            }
                        }
                    }
                    else if (checkDirNode.value == -1)
                    {
                        foreach (Point dir in directions)
                        {
                            checkDirPoint = Point.add(checkNode.point, Point.mult(dir, 1));
                            checkDirNode = getNodeAtPoint(checkDirPoint);
                            if (checkDirPoint.y >= 0) Debug.LogError("[" + checkDirNode.value + "]" + "checkDirPoint" + checkDirPoint.x + "," + checkDirPoint.y);

                            if (checkDirNode != null && checkDirNode.value > 0) // 유효한 piece가 있다면
                            {
                                FlipPieces(checkNode.point, checkDirPoint, true);
                                break;
                            }
                        }
                    }
                    else // 당겨야할 곳
                    {
                        // 2. 위 쪽 공간이 비어있으면 pass
                        if (checkDirNode.value > 0)
                        {
                            Debug.LogError("checkDirPoint" + checkDirPoint.x + "," + checkDirPoint.y);
                            Debug.LogError(checkNode.point.x + "_" + checkNode.point.y + "_위에 있으니까 플립!");
                            // 5. 둘 다 아니면 위에 뭔가가 있다는거니까 Flip해서 교체
                            FlipPieces(checkNode.point, checkDirPoint, true);
                        }
                    }
                }
            }
        }

        dead.RemoveAt(dead.Count - 1);
    }

    ChangePiece getFlipped(PieceController p)
    {
        ChangePiece flip = null;
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
        string seed = getRandomSeed();
        random = new System.Random(seed.GetHashCode());
        update = new List<PieceController>();
        flipped = new List<ChangePiece>();
        dead = new List<PieceController>();

        MakeBoard();
    }

    void MakeBoard()
    {
        // Grid Setting
        puzzleBoard = new Grid[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                puzzleBoard[x, y] = new Grid((BoardController.boardArray[y, x]) ? fillPiece() : -1, new Point(x, y));
            }
        }

        // Init Piece Setting
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Grid node = getNodeAtPoint(new Point(x, y));

                if (node.value <= 0) continue;

                settingPiece(node);
            }
        }

        SearchConnectFull();
        DeleteConnectedFull();
    }

    void settingPiece(Grid grid)
    {
        GameObject p = Instantiate(nodePiece, gameBoard); // piece Object 생성
        PieceController piece = p.GetComponent<PieceController>();
        RectTransform rect = p.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(anchor_x + (120 * grid.point.x), anchor_y- (70 * grid.point.y)); // global위치 세팅
        piece.Initialize(grid.value, new Point(grid.point.x, grid.point.y)); // pieceController 값 세팅
        grid.SetPiece(piece); // Grid와 piece 연결
    }

    void DeleteConnectedFull()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Grid tempNode = getNodeAtPoint(new Point(x, y));
                if (tempNode.value != -1)
                {
                    PieceController tempNodePiece = tempNode.getPiece();
                    if (tempNodePiece.cntCrossDown >= 2 || tempNodePiece.cntCrossUp >= 2 || tempNodePiece.cntStraight >= 2)
                    {
                        DeleteConnectedPoint(tempNode.point);
                    }
                }
            }
        }

        // 중력 적용 필요함!!
        IEnumerator enumerator;
        enumerator = applyGravitybyTick();

        // Coroutine으로 진행 만약 삭제된 애들이 있다면!
        if (dead.Count != 0)
        {
            StartCoroutine(enumerator);
        }
    }

    void DeleteConnectedPoint(Point point)
    {
        // 나중에 오브젝트 풀링해도 될듯
        Debug.LogError("[ deleted ] " + point.x + "_" + point.y);
       Grid grid = getNodeAtPoint(point);
        PieceController nodePiece = grid.getPiece();
        if (nodePiece != null)
        {
            dead.Add(nodePiece);
            GameObject.Destroy(nodePiece.gameObject); // piece 삭제
        }
        grid.SetPiece(null); // 그리드 안에 pieceController 정리
    }


    IEnumerator applyGravitybyTick()
    {
        // ApplyGravityToBoard()는 dead.Count가 0이 될 때까지 진행되어야 함.
        while (dead.Count != 0)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.LogError("ApplyGravityToBoard");
            ApplyGravityToBoard();
        }

        // 한 번 진행되면 Flip이 모두 완료되고 다음 단계로 넘어감

        if (dead.Count == 0)
        {
            yield return null;
        }
    }

    void SearchConnectFull()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                SearchConnectPoint(getNodeAtPoint(new Point(x, y)));
            }
        }
    }

    public void ResetPiece(PieceController piece)
    {
        piece.ResetPosition();
        update.Add(piece);
    }

    public void FlipPieces(Point one, Point two, bool main)
    {
        if (getValueAtPoint(one) < 0) return;

        Grid nodeOne = getNodeAtPoint(one);
        PieceController pieceOne = nodeOne.getPiece();
        if (getValueAtPoint(two) > 0)
        {
            Grid nodeTwo = getNodeAtPoint(two);
            PieceController pieceTwo = nodeTwo.getPiece();
            nodeOne.SetPiece(pieceTwo);
            nodeTwo.SetPiece(pieceOne);

            if (main)
                flipped.Add(new ChangePiece(pieceOne, pieceTwo));

            update.Add(pieceOne);
            update.Add(pieceTwo);
        }
        else
            ResetPiece(pieceOne);
    }

    int fillPiece()
    {
        int val = 1;
        val = (random.Next(0, 100) / (100 / enumLength)) + 1;
        return val;
    }

    int getValueAtPoint(Point p)
    {
        if (p.x < 0 || p.x >= width || p.y < 0 || p.y >= height) return -1;
        return puzzleBoard[p.x, p.y].value;
    }

    void setValueAtPoint(Point p, int v)
    {
        puzzleBoard[p.x, p.y].value = v;
    }

    Grid getNodeAtPoint(Point p)
    {
        if (p.y < 0) return null;

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

    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(anchor_x + (120 * p.x), anchor_y - (70 * p.y));
    }
}