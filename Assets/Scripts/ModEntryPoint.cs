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
        Game.World.console.DeveloperMode();

        

    }

    void FixCamera()
    {
        var inst = Game.World.cameraControl;
        var prop = inst.GetType().GetField("mouseYMin", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        float mouseYMin = 0.1f;
        prop.SetValue(inst, mouseYMin);
    }

    void LevelLoaded(GlobalEvents.LevelLoaded evnt)
    {
        Debug.Log(evnt.levelName);
        FixCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
