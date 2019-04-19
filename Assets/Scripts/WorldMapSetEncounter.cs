using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapSetEncounter : MonoBehaviour
{
    public EncountersProto _encounter;
    // Start is called before the first frame update
    void Start()
    {
        var wmv = GetComponentInChildren<WorldMapView>();

        //TOOD: make public
        var prop = wmv.GetType().GetField("_encounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        prop.SetValue(wmv, _encounter);
    }
}
