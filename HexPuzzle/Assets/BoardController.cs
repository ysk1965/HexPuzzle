using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardController : MonoBehaviour
{
    public GameObject piece;

    //public bool[][] a;

    //[SerializeField]
    //public SubArray[] m_mainArray;

    //[System.Serializable]
    //public struct SubArray
    //{
    //    [SerializeField]
    //    public bool[] m_subArray;
    //}
    public bool[,] boardArray = new bool[11, 7] {
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

    private void OnEnable()
    {
        
        // OnEnable됐을 때 보드판을 만들어 줘
        // 보드판은 임의로 바꿀 수 있도록 하자
        for (int i = 0; i< boardArray.GetLength(0); i++)
        {
            for (int j = 0; j< boardArray.GetLength(1); j++)
            {
                if (boardArray[i,j])
                {
                    GameObject a = Instantiate(piece, gameObject.transform);
                    a.transform.position = new Vector3(-150 + j * 116, i * 68, 0);
                    Debug.LogError(i);
                }
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
