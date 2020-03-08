using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoardController : MonoBehaviour
{
    public GameObject piece;

    static public bool[,] boardArray = new bool[11, 9] {
            { true, false, false, false, true, false, false, false, false},
            { false, false, false, true, false, true, false, false, false},
            { false, false, true, false, true, false, true, false, false},
            { false, true, false, true, false, true, false, true, false},
            { false, false, true, false, true, false, true, false, false},
            { false, true, false, true, false, true, false, true, false},
            { false, false, true, false, true, false, true, false, false},
            { false, true, false, true, false, true, false, true, false},
            { false, false, true, false, true, false, true, false, false},
            { false, false, false, true, false, true, false, false, false},
            { false, false, false, false, true, false, false, false, false},
        };
}
