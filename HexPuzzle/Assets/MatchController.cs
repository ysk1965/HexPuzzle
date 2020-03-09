using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    // public 
    [Header("NEED LINK")]
    public RectTransform gameBoard;
    public GameObject nodePiece;

    public float logicSpeed = 0.15f;

    List<PieceController> update;
    List<ChangePiece> changed;
    List<PieceController> blankList;
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

    private void Start()
    {
        update = new List<PieceController>();
        changed = new List<ChangePiece>();
        blankList = new List<PieceController>();
        hitObstacle = new HashSet<PieceController>();
    }
    void Update()
    {
        // Gravity Update
        if (bUpdateLogic)
        {
            //Debug.LogError("bUpdateLogic : " + update.Count);
            List<PieceController> finishedUpdating = new List<PieceController>();
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

                update.Remove(piece);
                updateClear();
            }
        }
        else // Picking Update
        {
            // 승패로직
            if (GameController.instance.obstacleCount == 0) {
                GameController.instance.gameEnding(true);
            }
            if (GameController.instance.moveCount == 0)
            {
                GameController.instance.gameEnding(false);
            }

            //Debug.LogError("Picking : " + update.Count);
            List<PieceController> finishedUpdating = new List<PieceController>();
            for (int i = 0; i < update.Count; i++)
            {
                PieceController piece = update[i];
                if (piece)
                {
                    if (!piece.UpdatePiece()) finishedUpdating.Add(piece);
                }
            }

            for (int i = 0; i < finishedUpdating.Count; i++)
            {
                PieceController piece = finishedUpdating[i];
                ChangePiece change = GetChanged(piece);
                PieceController flippedPiece = null;
                bool wasChanged = (change != null);
                
                // false면 다시스왑
                if (!IsConnectPoint(getGridAtPoint(piece.point)))
                {
                    if (wasChanged)
                    {
                        flippedPiece = change.getOtherPiece(piece);
                        FlipPieces(piece.point, flippedPiece.point, false);
                    }
                }
                else
                {
                    GameController.instance.MinusMoveCountvalue(1);
                    SearchAndDeleteConnectFull();
                    Debug.LogError("Update");
                }

                changed.Remove(change);
                update.Remove(piece);
                updateClear();
            }
        }
    }

    void updateClear()
    {
        bool isEmptyUpdate = false;
        foreach (PieceController pc in update)
        {
            if (pc != null) isEmptyUpdate = true;
        }
        if(isEmptyUpdate == false)
        {
            update.Clear();
        }
    }

    public void SearchConnectPoint(Grid currentNode)
    {
        Point[] directions_2way = {
            Point.up,
            Point.rightUp,
        };

        Point currentPoint = currentNode.point;

        if (currentNode.value == (int)eStatus.SILVER) return;

        if (getValueAtPoint(currentPoint) != -1)
        {
            if (!currentNode.getPiece()) return; 

            currentNode.getPiece().initCnt();
            int linkedCnt = 0;
            int areaCnt = 0;
            Point savePoint1 = Point.zero;
            Point savePoint2 = Point.zero;
            Point savePriorPoint = Point.zero;

            // cnt가 2이상이면 3개 이상 연결된 상황.
            foreach (Point dir in directions)
            {
                Point nextPoint = Point.add(currentPoint, Point.mult(dir, 1));
                int connectCnt = 0;

                // 6방향 area 검색
                if (currentNode.value == getValueAtPoint(nextPoint))
                {
                    linkedCnt++;
                    if (areaCnt < linkedCnt)
                    {
                        savePoint1 = dir;
                        savePoint2 = savePriorPoint;
                        areaCnt = linkedCnt;
                    }
                    savePriorPoint = dir;
                }
                else
                {
                    linkedCnt = 0;
                }

                // 해당 direction으로 value가 틀릴 때까지 찾기
                while (currentNode.value == getValueAtPoint(nextPoint))
                {
                    connectCnt += 1;
                    nextPoint = Point.add(currentPoint, Point.mult(dir, connectCnt + 1));
                }

                if (dir.Equals(Point.rightUp) || dir.Equals(Point.leftDown))
                {
                    currentNode.getPiece().cntCrossDown += connectCnt;
                }
                if (dir.Equals(Point.rightDown) || dir.Equals(Point.leftUp))
                {
                    currentNode.getPiece().cntCrossUp += connectCnt;
                }
                if (dir.Equals(Point.up) || dir.Equals(Point.down))
                {
                    currentNode.getPiece().cntStraight += connectCnt;
                }
            }

            // 남은 2방향 area 검색
            foreach (Point dir in directions_2way)
            {
                Point nextPoint = Point.add(currentPoint, Point.mult(dir, 1));

                if (currentNode.value == getValueAtPoint(nextPoint))
                {
                    linkedCnt++;
                    if (areaCnt < linkedCnt)
                    {
                        savePoint1 = dir;
                        savePoint2 = savePriorPoint;
                        areaCnt = linkedCnt;
                    }
                    else
                    {
                        linkedCnt = 0;
                    }
                }
                savePriorPoint = dir;
            }

            // 건너편도 계산
            if (areaCnt == 2)
            {
                Point tempPoint = Point.add(savePoint1, savePoint2);
                Point nextPoint = Point.add(currentPoint, tempPoint);
                if (currentNode.value == getValueAtPoint(nextPoint))
                {
                    areaCnt++;
                }
            }
            currentNode.getPiece().cntArea = areaCnt;
        }
    }

    // Piece Change 조건 확인을 위한 Func
    bool IsConnectPoint(Grid currentNode)
    {
        Point[] directions_2way = {
            Point.up,
            Point.rightUp,
        };

        int cntCrossUp = 0;
        int cntCrossDown = 0;
        int cntStraight = 0;
        int cntArea = 0;
        int linkedCnt = 0;
        Point savePoint1 = Point.zero;
        Point savePoint2 = Point.zero;
        Point savePriorPoint = Point.zero;
        Point currentPoint = currentNode.point;

        if (currentNode.value == (int)eStatus.SILVER) return false;


        if (getValueAtPoint(currentPoint) != -1)
        {
            if (!currentNode.getPiece()) return false;

            currentNode.getPiece().initCnt();
            foreach (Point dir in directions)
            {
                Point nextPoint = Point.add(currentPoint, Point.mult(dir, 1));
                int connectCnt = 0;

                // 6방향 area 검색
                if (currentNode.value == getValueAtPoint(nextPoint))
                {
                    linkedCnt++;
                    if (cntArea < linkedCnt)
                    {
                        savePoint1 = dir;
                        savePoint2 = savePriorPoint;
                        cntArea = linkedCnt;
                    }
                    savePriorPoint = dir;
                }
                else
                {
                    linkedCnt = 0;
                }

                while (currentNode.value == getValueAtPoint(nextPoint))
                {
                    connectCnt += 1;
                    nextPoint = Point.add(currentPoint, Point.mult(dir, connectCnt + 1));
                }

                if (dir.Equals(Point.rightUp) || dir.Equals(Point.leftDown))
                {
                    cntCrossUp += connectCnt;
                }
                if (dir.Equals(Point.rightDown) || dir.Equals(Point.leftUp))
                {
                   cntCrossDown += connectCnt;
                }
                if (dir.Equals(Point.up) || dir.Equals(Point.down))
                {
                    cntStraight += connectCnt;
                }
            }
        }

        // 남은 2방향 area 검색
        foreach (Point dir in directions_2way)
        {
            Point nextPoint = Point.add(currentPoint, Point.mult(dir, 1));

            if (currentNode.value == getValueAtPoint(nextPoint))
            {
                linkedCnt++;
                if (cntArea < linkedCnt)
                {
                    savePoint1 = dir;
                    savePoint2 = savePriorPoint;
                    cntArea = linkedCnt;
                }
                else
                {
                    linkedCnt = 0;
                }
            }
            savePriorPoint = dir;
        }

        // 건너편도 계산
        if (cntArea == 2)
        {
            Point tempPoint = Point.add(savePoint1, savePoint2);
            Point nextPoint = Point.add(currentPoint, tempPoint);
            if (currentNode.value == getValueAtPoint(nextPoint))
            {
                cntArea++;
            }
        }

        if (cntCrossDown >= 2 || cntCrossUp >=2 || cntStraight >= 2 || cntArea >= 3)
        {
            return true;
        }
        return false;
    }

    public void ApplyGravityToBoard()
    {
        Point[] directions_2way = {
            Point.rightDown,
            Point.leftDown
        };

        // 아래서부터 비어있는 공간 찾아서 중력검사 (빈 칸이 생긴 만큼) 
        for (int y = (BoardController.height - 1); y >= 0; y--)
        {
            for (int x = 0; x < BoardController.width; x++)
            {
                Grid checkNode = getGridAtPoint(new Point(x, y));
                if (checkNode.value == -1) continue; // 검사할 필요 없는 Grid

                // 비어있는 Piece만 검사
                if (checkNode.value == 0)
                {
                    // 바로 윗칸을 먼저 체크 (height가 아래서 부터 올라감 (역방향))
                    Point checkDirPoint = Point.add(checkNode.point, Point.mult(Point.up, -1));
                    Grid checkDirNode = getGridAtPoint(checkDirPoint);

                    if (checkNode.point.y == 0)
                    {
                        //Debug.LogError(checkNode.point.x + "_" + checkNode.point.y + "_최초공간!");
                        checkNode.value = setValue();
                        SettingPiece(checkNode);

                        continue;
                    }

                    //Debug.LogError("[" + checkNode.value + "]" + checkNode.point.x + "_" + checkNode.point.y + "_이번에 체크할 Node");
                    if (checkDirNode == null)
                    {
                        foreach (Point dir in directions_2way) // 좌상향, 우상향 체크
                        {
                            checkDirPoint = Point.add(checkNode.point, Point.mult(dir, 1));
                            checkDirNode = getGridAtPoint(checkDirPoint);
                            //if (checkDirPoint.y >= 0) Debug.LogError("[" + checkDirNode.value + "]" + "checkDirPoint" + checkDirPoint.x + "," + checkDirPoint.y);

                            if (checkDirNode != null && checkDirNode.value > 0)
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
                            //if (checkDirPoint.y >= 0) Debug.LogError("[" + checkDirNode.value + "]" + "checkDirPoint" + checkDirPoint.x + "," + checkDirPoint.y);

                            if (checkDirNode != null && checkDirNode.value > 0)
                            {
                                FlipPieces(checkNode.point, checkDirPoint, true);
                                break;
                            }
                        }
                    }
                    else // 당겨야할 곳
                    {
                        // 위 쪽 공간이 비어있으면 pass
                        if (checkDirNode.value > 0)
                        {
                            //Debug.LogError("checkDirPoint" + checkDirPoint.x + "," + checkDirPoint.y);
                            //Debug.LogError(checkNode.point.x + "_" + checkNode.point.y + "_위에 있으니까 플립!");
                            FlipPieces(checkNode.point, checkDirPoint, true);
                        }
                    }
                }
            }
        }

        blankList.RemoveAt(blankList.Count - 1);
    }

    ChangePiece GetChanged(PieceController p)
    {
        ChangePiece flip = null;
        for (int i = 0; i < changed.Count; i++)
        {
            if (changed[i].getOtherPiece(p) != null)
            {
                flip = changed[i];
                break;
            }
        }
        return flip;
    }

    public void SettingPiece(Grid grid)
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
                    if (tempNodePiece && (tempNodePiece.cntCrossDown >= 2 || tempNodePiece.cntCrossUp >= 2 || tempNodePiece.cntStraight >= 2 || tempNodePiece.cntArea >= 3))
                    {
                        DeleteConnectedPoint(tempNode.point);
                    }
                }
            }
        }

        // obstacle 적용
        foreach(PieceController pc in hitObstacle)
        {
            int hitcount = pc.getHitCount() + 1;
            pc.setHitCount(hitcount);
            if (hitcount == 2)
            {
                if (pc.value == (int)eStatus.SILVER)
                {
                    GameController.instance.MinusObstacleCountvalue();
                }
                SetGameScore(pc.value);

                blankList.Add(pc);
                getGridAtPoint(pc.point).SetPiece(null);
                Destroy(pc.gameObject);
            }
        }
        hitObstacle.Clear();

        if (blankList.Count != 0)
        {
            bUpdateLogic = true;
            StartCoroutine(applyGravitybyTick());
        }
    }

    IEnumerator applyGravitybyTick()
    {
        //Debug.LogError("ApplyGravityToBoard");
        // ApplyGravityToBoard()는 dead.Count가 0이 될 때까지 진행되어야 함.(Tick마다)
        while (blankList.Count != 0)
        {
            //Debug.LogError("blankList.Count : " + blankList.Count);
            ApplyGravityToBoard();
            yield return new WaitForSeconds(logicSpeed);
        }

        yield return new WaitUntil(() => update.Count == 0);

        SearchAndDeleteConnectFull();

        bUpdateLogic = false;
    }

    void DeleteConnectedPoint(Point point)
    {
        //Debug.LogError("[ deleted ] " + point.x + "_" + point.y);
        Grid grid = getGridAtPoint(point);
        PieceController nodePiece = grid.getPiece();

        if (nodePiece != null)
        {
            SetGameScore(nodePiece.value);

            blankList.Add(nodePiece);
            GameObject.Destroy(nodePiece.gameObject); // piece 삭제
        }
        grid.SetPiece(null); // 그리드 안에 pieceController 정리

        // 주변에 silver가 있다면 Cnt를 늘리고 Update시켜주자..!
        foreach (Point dir in directions)
        {
            Point checkDirPoint = Point.add(point, Point.mult(dir, 1));

            if (!(checkDirPoint.y > 0 && checkDirPoint.y < BoardController.height) || !(checkDirPoint.x > 0 && checkDirPoint.x < BoardController.width)) continue;

            Grid tempGrid = getGridAtPoint(checkDirPoint);
            if (tempGrid.value == (int)eStatus.SILVER)
            {
                hitObstacle.Add(tempGrid.getPiece());
            }
        }
    }

    void SetGameScore(int val)
    {
        switch (val)
        {
            case 0:
                break;
            case 1:
                GameController.instance.AddScoreCountvalue(10);
                break;
            case 2:
                GameController.instance.AddScoreCountvalue(20);
                break;
            case 3:
                GameController.instance.AddScoreCountvalue(30);
                break;
            case 4:
                GameController.instance.AddScoreCountvalue(40);
                break;
            case 5:
                GameController.instance.AddScoreCountvalue(50);
                break;
            case 6:
                GameController.instance.AddScoreCountvalue(200);
                break;
        }
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
                changed.Add(new ChangePiece(pieceOne, pieceTwo));

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