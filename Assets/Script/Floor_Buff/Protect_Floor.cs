using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Protect_Floor : MonoBehaviour
{
    public int maximumTolerable = 3;  //最大可承受数

    public void injured()
    {
        if (maximumTolerable > 0)
        {
            maximumTolerable -= 1;
        }
        else
        {
            //GameOver;
        }
    }
}
