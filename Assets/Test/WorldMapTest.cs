using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapTest : MonoBehaviour
{
    public ModEntryPoint EntryPoint;
    public GameObject WorldMap;
    // Start is called before the first frame update
    void Start()
    {
        GlobalEvents.AddListener<OnlineEvents.Login>(OnLogin);
    }


    void OnLogin(OnlineEvents.Login login)
    {
        Debug.Log("OnLogin");
        Instantiate(WorldMap, transform);
        EntryPoint.SetMessagesFetch(true);
    }
}
