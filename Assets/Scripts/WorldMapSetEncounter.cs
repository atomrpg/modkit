using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapSetEncounter : MonoBehaviour
{
    public EncountersProtoRef _encounter;
    // Start is called before the first frame update
    void Update()
    {
        var wmv = GetComponentInChildren<Encounter>(true);
        if (wmv != null)
        {
            wmv.Prototype = _encounter;
            Destroy(this);
        }
    }
}
