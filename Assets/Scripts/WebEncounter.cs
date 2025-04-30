using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebEncounter : MonoBehaviour
{    
    public void Enter()
    {
        GlobalEvents.PerformEvent(new OnlineEvents.EncounterBegin(){});
    }

    public void Exit()
    {
        GlobalEvents.PerformEvent(new OnlineEvents.EncounterEnd() { });
    }
}