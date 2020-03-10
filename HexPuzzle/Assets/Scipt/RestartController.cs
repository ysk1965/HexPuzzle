using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartController : MonoBehaviour
{
    public GameObject successObject;
    public GameObject failureObject;

    private void OnEnable()
    {
        if (GameController.instance.moveCount == 0)
        {
            failureObject.SetActive(true);
            successObject.SetActive(false);
        }
        else
        {
            failureObject.SetActive(false);
            successObject.SetActive(true);
        }
    }
}
