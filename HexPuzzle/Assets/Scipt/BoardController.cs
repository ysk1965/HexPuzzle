using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardController : MonoBehaviour
{
    public MatchController mc;
    public static Grid[,] puzzleBoard;

    public static int width;
    public static int height;

    // -1 : NULL
    // 0 : HOLE
    // 1 : RED
    // 2 : GREEN
    // 3 : BLUE
    // 4 : MAGENTA
    // 5 : YELLOW
    // 6 : OBSTACLE
    public int[,] boardArray = new int[11, 9] {
            { -1, -1, -1, -1, 3, -1, -1, -1, -1},
            { -1, -1, -1, 6, -1, 6, -1, -1, -1},
            { -1, -1, 6, -1, 2, -1, 6, -1, -1},
            { -1, 5, -1, 2, -1, 3, -1, 5, -1},
            { -1, -1, 4, -1, 1, -1, 4, -1, -1},
            { -1, 2, -1, 1, -1, 2, -1, 2, -1},
            { -1, -1, 4, -1, 6, -1, 4, -1, -1},
            { -1, 3, -1, 5, -1, 1, -1, 3, -1},
            { -1, -1, 6, -1, 4, -1, 6, -1, -1},
            { -1, -1, -1, 6, -1, 6, -1, -1, -1},
            { -1, -1, -1, -1, 6, -1, -1, -1, -1},
        };

    public static int anchor_x = -470;
    public static int anchor_y = 330;

    private void Start()
    {
        width = boardArray.GetLength(1);
        height = boardArray.GetLength(0);
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
                puzzleBoard[x, y] = new Grid(boardArray[y, x], new Point(x, y));
            }
        }

        // Init Piece Setting
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Grid node = mc.getGridAtPoint(new Point(x, y));

                if (node.value <= 0) continue;

                mc.SettingPiece(node);
            }
        }
        mc.SearchConnectFull();
        mc.DeleteConnectedFull();
    }
}
