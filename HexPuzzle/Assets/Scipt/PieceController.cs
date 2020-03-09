using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PieceController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value;
    public Point point;

    public int cntCrossUp;
    public int cntCrossDown;
    public int cntStraight;
    public int cntArea;

    private int hitCount = 0;

    [HideInInspector]
    public Vector2 pos;
    [HideInInspector]
    public RectTransform rect;

    bool updating;

    public void Update() {
        if (hitCount == 1) gameObject.transform.Rotate(new Vector3(0, 0, 2));
    }

    public int getHitCount()
    {
        return hitCount;
    }

    public void setHitCount(int val)
    {
        hitCount = val;
        return;
    }

    public void Initialize(int v, Point p)
    {
        rect = GetComponent<RectTransform>();

        value = v;
        SetPoint(p);
        SetColor(value);
    }

    public void initCnt()
    {
        cntCrossUp = 0;
        cntCrossDown = 0;
        cntStraight = 0;
    }

    public void SetColor(int val)
    {
        switch (val)
        {
            case (int)eStatus.NULL:
                break;
            case (int)eStatus.RED:
                GetComponent<Image>().color = Color.red;
                break;
            case (int)eStatus.GREEN:
                GetComponent<Image>().color = Color.green;
                break;
            case (int)eStatus.BLUE:
                GetComponent<Image>().color = Color.blue;
                break;
            case (int)eStatus.MAGENTA:
                GetComponent<Image>().color = Color.magenta;
                break;
            case (int)eStatus.YELLOW:
                GetComponent<Image>().color = Color.yellow;
                break;
            case (int)eStatus.SILVER:
                break;
        }
    }

    public void SetPoint(Point p)
    {
        point = p;
        ResetPosition();
        UpdateName();
    }

    public void ResetPosition()
    {
        pos = new Vector2(BoardController.anchor_x + (120 * point.x), BoardController.anchor_y - (70 * point.y));
    }

    public void MovePosition(Vector2 move)
    {
        rect.anchoredPosition += move * Time.deltaTime * 25f;
    }

    public void MovePositionTo(Vector2 move)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 25f);
    }

    public void SetPosition(Vector2 move)
    {
        rect.anchoredPosition = move;
    }

    public bool UpdatePiece()
    {
        if (Vector3.Distance(rect.anchoredPosition, pos) > 1)
        {
            MovePositionTo(pos);
            updating = true;
            return true;
        }
        else
        {
            rect.anchoredPosition = pos;
            updating = false;
            return false;
        }
    }

    void UpdateName()
    {
        transform.name = "[" + point.x + ", " + point.y + "]";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (updating) return;
        if (MatchController.bUpdateLogic) return;
        MoveController.instance.MovePiece(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        MoveController.instance.DropPiece();
    }
}