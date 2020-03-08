using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    // public 
    public RectTransform gameBoard;
    public GameObject nodePiece;

    List<PieceController> update;
    List<ChangePiece> flipped;
    List<PieceController> dead;
    HashSet<PieceController> hitObstacle;

    public static bool bUpdateLogic = false;

    int enumLength = System.Enum.GetValues(typeof(eStatus)).Length;

    Point[] directions = {
            Point.up,
            Point.rightUp,
            Point.rightDown,
            Point.down,
            Point.leftDown,
            Point.leftUp
        };

    void Update()
    {
        // Gravity Update
        if (bUpdateLogic)
        {
            List<PieceController> finishedUpdating = new List<PieceController>();
            //Debug.LogError("gravity : " + update.Count);
            for (int i = 0; i < update.Count; i++)
            {
                PieceController piece = update[i];
                if (piece)
                {
                    piece.UpdatePiece();
                    if (!piece.UpdatePiece()) finishedUpdating.Add(piece);
                }
            }

            for (int i = 0; i< finishedUpdating.Count; i++)
            {
                PieceController piece = finishedUpdating[i];
                //Debug.LogError("finishing : " + piece.point.x + "_" + piece.point.y);

                update.Remove(piece);
            }
        }
        else // Picking Update
        {
            List<PieceController> finishedUpdating = new List<PieceController>();
            for (int i = 0; i < update.Count; i++)
            {
                SearchAndDeleteConnectFull();
                PieceController piece = update[i];
                if (piece)
                {
                    if (!piece.UpdatePiece()) finishedUpdating.Add(piece);
                }
            }

            // 마우스로 뭘 만들면 동작하도록 만들어야 할듯

            // update가 생기면 FULL검사

            //Debug.LogError("move logic : " + update.Count);
            // 이동 및 검증 로직
            //for (int i = 0; i < update.Count; i++)
            //{
            //    PieceController piece = update[i];
            //    if (piece) piece.UpdatePiece();

            //    SearchConnectFull();
            //    DeleteConnectedFull();
            //}

            //for (int i = 0; i < finishedUpdating.Count; i++)
            //{
            //    PieceController piece = finishedUpdating[i];
            //    ChangePiece change = getFlipped(piece);

            //    PieceController flippedPiece = null;
            //    bool wasChanged = (change != null);

            //    if (wasChanged) //If we flipped to make this update
            //    {
            //        flippedPiece = change.getOtherPiece(piece);
            //        //AddPoints(ref connected, isConnected(flippedPiece.point, true));
            //    }

            //    FlipPieces(piece.point, flippedPiece.point, false);
            //}
        }

        /*
        for (int i = 0; i < finishedUpdating.Count; i++)
        {
            PieceController piece = finishedUpdating[i];
            ChangePiece change = getFlipped(piece);

            PieceController flippedPiece = null;
            bool wasChanged = (change != null);

            if (wasChanged) //If we flipped to make this update
            {
                flippedPiece = change.getOtherPiece(piece);
                //AddPoints(ref connected, isConnected(flippedPiece.point, true));
            }

            FlipPieces(piece.point, flippedPiece.point, false);
        }
        */
        //if (update.Count == 0) bUpdateLogic = false;
    }

    private void Start()
    {
        update = new List<PieceController>();
        flipped = new List<ChangePiece>();
        dead = new List<PieceController>();
        hitObstacle = new HashSet<PieceController>();
    }

    public void SearchConnectPoint(Grid currentNode)
    {
        // 방향에 따라서 몇개나 연결되어 있나 확인
        Point currentPoint = currentNode.point;

        if (currentNode.value == (int)eStatus.SILVER) return;

        if (getValueAtPoint(currentPoint) != -1)
        {
            if (!currentNode.getPiece()) return; 
                
            currentNode.getPiece().initCnt();
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
        Point[] directions_2way = {
            Point.rightUp,
            Point.leftUp
        };

        // 아래서부터 비어있는 공간 찾아서 중력검사 (빈 칸이 생긴 만큼) 
        // --> Coroutine으로 돌아야지 순차적으로 이동 되는게 보일듯 플립 모두 완료 되면 다음 루틴으로!
        for (int y = (BoardController.height - 1); y >= 0; y--)
        {
            for (int x = 0; x < BoardController.width; x++)
            {
                Grid checkNode = getGridAtPoint(new Point(x, y));
                if (checkNode.value == -1) continue; // 검사할 필요 없는 Grid

                // 비어있는 애들만 체크 하면 됨.
                if (checkNode.value == 0)
                {
                    // 1. 바로 윗칸을 먼저 체크 (height가 아래서 부터 올라감 (역방향))
                    Point checkDirPoint = Point.add(checkNode.point, Point.mult(Point.up, -1));
                    Grid checkDirNode = getGridAtPoint(checkDirPoint);

                    if (checkNode.point.y == 0)
                    {
                        Debug.LogError(checkNode.point.x + "_" + checkNode.point.y + "_최초공간!");
                        checkNode.value = setValue();
                        settingPiece(checkNode);

                        continue;
                    }

                    Debug.LogError("[" + checkNode.value + "]" + checkNode.point.x + "_" + checkNode.point.y + "_이번에 체크할 Node");
                    if (checkDirNode == null) // Top, 가장 윗부분에 접근 (만들어져야할 곳)
                    {
                        foreach (Point dir in directions_2way)
                        {
                            checkDirPoint = Point.add(checkNode.point, Point.mult(dir, 1));
                            checkDirNode = getGridAtPoint(checkDirPoint);
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
                        foreach (Point dir in directions_2way)
                        {
                            checkDirPoint = Point.add(checkNode.point, Point.mult(dir, 1));
                            checkDirNode = getGridAtPoint(checkDirPoint);
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

    public void settingPiece(Grid grid)
    {
        GameObject p = Instantiate(nodePiece, gameBoard); // piece Object 생성
        PieceController piece = p.GetComponent<PieceController>();
        RectTransform rect = p.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(BoardController.anchor_x + (120 * grid.point.x), BoardController.anchor_y - (70 * grid.point.y)); // global위치 세팅
        piece.Initialize(grid.value, new Point(grid.point.x, grid.point.y)); // pieceController 값 세팅
        grid.SetPiece(piece); // Grid와 piece 연결
    }

    public void DeleteConnectedFull()
    {
        for (int x = 0; x < BoardController.width; x++)
        {
            for (int y = 0; y < BoardController.height; y++)
            {
                Grid tempNode = getGridAtPoint(new Point(x, y));
                if (tempNode.value != -1)
                {
                    PieceController tempNodePiece = tempNode.getPiece();
                    if ((tempNodePiece.cntCrossDown >= 2 || tempNodePiece.cntCrossUp >= 2 || tempNodePiece.cntStraight >= 2))
                    {
                        DeleteConnectedPoint(tempNode.point);
                    }
                }
            }
        }

        // obstacle에 hit 적용
        foreach(PieceController pc in hitObstacle)
        {
            pc.hitCount++;
        }
        hitObstacle.Clear();

        // 중력 적용 필요함!!
        IEnumerator enumerator;
        enumerator = applyGravitybyTick();

        // Coroutine으로 진행 만약 삭제된 애들이 있다면!
        if (dead.Count != 0)
        {
            bUpdateLogic = true;
            StartCoroutine(enumerator);
        }
    }
    IEnumerator applyGravitybyTick()
    {
        Debug.LogError("ApplyGravityToBoard");
        // ApplyGravityToBoard()는 dead.Count가 0이 될 때까지 진행되어야 함.
        while (dead.Count != 0)
        {
            ApplyGravityToBoard();
            yield return new WaitForSeconds(0.5f);
        }

        update.Clear();
        bUpdateLogic = false;
        yield return null;
    }

    void DeleteConnectedPoint(Point point)
    {
        // 나중에 오브젝트 풀링해도 될듯
        Debug.LogError("[ deleted ] " + point.x + "_" + point.y);
        Grid grid = getGridAtPoint(point);
        PieceController nodePiece = grid.getPiece();
        if (nodePiece != null)
        {
            dead.Add(nodePiece);
            GameObject.Destroy(nodePiece.gameObject); // piece 삭제
        }
        grid.SetPiece(null); // 그리드 안에 pieceController 정리

        // 주변에 silver가 있다면 Cnt를 늘리고 Update시켜주자..!
        //foreach(Point dir in directions)
        //{
        //    Point checkDirPoint = Point.add(point, Point.mult(dir, 1));

        //    if (checkDirPoint.y < 0) continue;
                
        //    Grid tempGrid = getGridAtPoint(checkDirPoint);
        //    if(tempGrid.value == (int)eStatus.SILVER)
        //    {
        //        hitObstacle.Add(tempGrid.getPiece());
        //    }
        //}
    }

    public void SearchAndDeleteConnectFull()
    {
        for (int x = 0; x < BoardController.width; x++)
        {
            for (int y = 0; y < BoardController.height; y++)
            {
                SearchConnectPoint(getGridAtPoint(new Point(x, y)));
            }
        }

        DeleteConnectedFull();
    }

    public void SearchConnectFull()
    {
        for (int x = 0; x < BoardController.width; x++)
        {
            for (int y = 0; y < BoardController.height; y++)
            {
                SearchConnectPoint(getGridAtPoint(new Point(x, y)));
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

        Grid nodeOne = getGridAtPoint(one);
        PieceController pieceOne = nodeOne.getPiece();
        if (getValueAtPoint(two) > 0)
        {
            Grid nodeTwo = getGridAtPoint(two);
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

    int setValue()
    {
        return Random.Range(1, enumLength - 1);
    }

    int getValueAtPoint(Point p)
    {
        if (p.x < 0 || p.x >= BoardController.width || p.y < 0 || p.y >= BoardController.height) return -1;
        return BoardController.puzzleBoard[p.x, p.y].value;
    }

    public Grid getGridAtPoint(Point p)
    {
        if (p.y < 0) return null;

        return BoardController.puzzleBoard[p.x, p.y];
    }

    public Vector2 getPositionFromPoint(Point p)
    {
        return new Vector2(BoardController.anchor_x + (120 * p.x), BoardController.anchor_y - (70 * p.y));
    }
}