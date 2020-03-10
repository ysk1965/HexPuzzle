using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("UI Setting")]
    public BoardController bc;
    public GameObject resultObject;

    public Text UI_moveCount;
    public Text UI_obstacleCount;
    public Text UI_score;

    public int moveCount;
    public int obstacleCount;
    public int initScore;

    public static GameController instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        moveCount = 20;
        obstacleCount = 10;
        initScore = 0;

        SetmoveCountvalue(moveCount);
        SetObstacleCountvalue(obstacleCount);
    }

    public void SetmoveCountvalue(int val)
    {
        if (!UI_moveCount) return;
        moveCount = val;
        UI_moveCount.text = moveCount.ToString();
    }

    public void MinusMoveCountvalue(int val)
    {
        if (!UI_moveCount) return;
        moveCount -= val;
        UI_moveCount.text = moveCount.ToString();
    }

    public void SetObstacleCountvalue(int val)
    {
        if (!UI_obstacleCount) return;
        obstacleCount = val;
        UI_obstacleCount.text = obstacleCount.ToString();
    }

    public void MinusObstacleCountvalue()
    {
        if (!UI_obstacleCount) return;
        obstacleCount--;
        UI_obstacleCount.text = obstacleCount.ToString();
    }

    public void AddScoreCountvalue(int val)
    {
        if (!UI_score) return;
        initScore += val;
        UI_score.text = initScore.ToString();
    }

    public void SetScoreCountvalue(int val)
    {
        if (!UI_score) return;
        initScore = val;
        UI_score.text = initScore.ToString();
    }

    public void ResultRogic()
    {
        // 승리
        if (obstacleCount == 0)
        {
            resultObject.SetActive(true);
        }

        // 패배
        if (moveCount == 0)
        {
            resultObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        Debug.LogError("RESTART");
        moveCount = 20;
        obstacleCount = 10;
        initScore = 0;
        SetmoveCountvalue(moveCount);
        SetObstacleCountvalue(obstacleCount);
        SetScoreCountvalue(initScore);

        bc.MakeBoard();

        resultObject.SetActive(false);
    }
}
