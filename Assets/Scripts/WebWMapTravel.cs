using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebWMapTravel : MonoBehaviour
{
    public void Travel(int dir)
    {
        GlobalEvents.PerformEvent<OnlineEvents.Travel>(new OnlineEvents.Travel
        {
            dir = dir,
        });
    }
}
