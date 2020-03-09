using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("UI Setting")]
    public Text UI_moveCount;
    public Text UI_obstacleCount;
    public Text UI_score;

    public int moveCount;
    public int obstacleCount;
    public int score;

    public static GameController instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        moveCount = 20;
        obstacleCount = 10;
        score = 0;

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
        score += val;
        UI_score.text = score.ToString();
    }

    public void ResultRogic()
    {
        // 승리
        if (obstacleCount == 0)
        {
            // 다시하기 만들기
        }

        // 패배
        if (moveCount == 0)
        {
            // 다시하기 만들기
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
