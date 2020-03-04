using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PieceController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public int value;
    public MyPoint piecePoint;

    [HideInInspector]
    public Vector2 pos;
    [HideInInspector]
    public RectTransform rect;

    bool updating;

    public void Initialize(int v, MyPoint p)
    {
        rect = GetComponent<RectTransform>();

        value = v;
        SetIndex(p);
        SetColor(value);
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

    public void SetIndex(MyPoint p)
    {
        piecePoint = p;
        ResetPosition();
        UpdateName();
    }

    public void ResetPosition()
    {
        pos = new Vector2(-360 + (120 * piecePoint.x), -400 - (70 * piecePoint.y));
    }

    public void MovePosition(Vector2 move)
    {
        rect.anchoredPosition += move * Time.deltaTime * 16f;
    }

    public void MovePositionTo(Vector2 move)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 16f);
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
        transform.name = "Node [" + piecePoint.x + ", " + piecePoint.y + "]";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (updating) return;
        GameManager.instance.MovePiece(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.instance.DropPiece();
    }
}
