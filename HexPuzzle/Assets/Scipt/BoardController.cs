using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardController : MonoBehaviour
{
    public GameObject piece;

    static public bool[,] boardArray = new bool[11, 7] {
            { false, false, false, true, false, false, false},
            { false, false, true, false, true, false, false},
            { false, true, false, true, false, true, false},
            { true, false, true, false, true, false, true},
            { false, true, false, true, false, true, false},
            { true, false, true, false, true, false, true},
            { false, true, false, true, false, true, false},
            { true, false, true, false, true, false, true},
            { false, true, false, true, false, true, false},
            { false, false, true, false, true, false, false},
            { false, false, false, true, false, false, false},
        };

    //private void OnEnable()
    //{
        
    //    // OnEnable됐을 때 보드판을 만들어 줘
    //    // 보드판은 임의로 바꿀 수 있도록 하자
    //    for (int i = 0; i< boardArray.GetLength(0); i++)
    //    {
    //        for (int j = 0; j< boardArray.GetLength(1); j++)
    //        {
    //            if (boardArray[i,j])
    //            {
    //                GameObject a = Instantiate(piece, gameObject.transform);
    //                a.transform.position = new Vector3(j * 29, i * 17, 0);
    //                a.AddComponent<PieceController>();
    //            }
    //        }
    //    }
        
    //}
}
