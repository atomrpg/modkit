using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModEntryPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Mod Init");
        ResourceManager.AddBundle(AssetBundle.LoadFromFile(Application.persistentDataPath + "/Mods/MyMod/resources"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
