using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;

public class ModEntryPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Mod Init");
        ResourceManager.AddBundle(AssetBundle.LoadFromFile(Application.persistentDataPath + "/Mods/MyMod/resources"));
        GlobalEvents.AddListener<GlobalEvents.GameStart>(GameLoaded);
        GlobalEvents.AddListener<GlobalEvents.LevelLoaded>(LevelLoaded);
    }

    void GameLoaded(GlobalEvents.GameStart evnt)
    {
        Localization.LoadStrings("mymod_strings_");
    }

    void LevelLoaded(GlobalEvents.LevelLoaded evnt)
    {
        Debug.Log(evnt.levelName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
