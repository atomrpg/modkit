#define SUPPORT_LEVEL_BUNDLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;

//[assembly: AssemblyTitle("My Mod")] // ENTER MOD TITLE


public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    string modName;
    string dir;


    AssetBundleManifest manifest;

    void Start()
    {
        var assembly = GetType().Assembly;
        modName = assembly.GetName().Name;
        dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");

        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));
        GlobalEvents.AddListener<GlobalEvents.GameStart>(GameLoaded);
        GlobalEvents.AddListener<GlobalEvents.LevelLoaded>(LevelLoaded);
#if SUPPORT_LEVEL_BUNDLE
        AssetBundle assetBundle = AssetBundle.LoadFromFile(dir + "/" + modName);
        if (assetBundle != null)
        {
            manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        GlobalEvents.AddListener<GlobalEvents.PrepareNextLevel>(PrepareNextLevel);
#endif
    }

    void GameLoaded(GlobalEvents.GameStart evnt)
    {
        Localization.LoadStrings("mymod_strings_");
        Game.World.console.DeveloperMode();
    }

#if SUPPORT_LEVEL_BUNDLE
    List<AssetBundle> lastLevelBundle = new List<AssetBundle>();
    void PrepareNextLevel(GlobalEvents.PrepareNextLevel evnt)
    {
        if (manifest != null)
        {
            if (lastLevelBundle.Count > 0)
            {
                Debug.Log("Unload last level bundle: " + evnt.levelName);

                foreach (var bundle in lastLevelBundle)
                {
                    ResourceManager.RemoveBundle(bundle, true);
                }
            }


            AssetBundle b;

            foreach (var bundle in manifest.GetAllDependencies(modName + "_" + evnt.levelName))
            {
                if(bundle.Contains("_resources"))
                {
                    continue; // skip default resources pack
                }

                Debug.Log("Load shared level bundle: " + evnt.levelName);

                ResourceManager.AddBundle(modName, b = AssetBundle.LoadFromFile(dir + "/" + bundle));
                lastLevelBundle.Add(b);
            }

            Debug.Log("Load level bundle: " + evnt.levelName);
            ResourceManager.AddBundle(modName, b = AssetBundle.LoadFromFile(dir + "/" + modName + "_" + evnt.levelName));
            lastLevelBundle.Add(b);
        }
    }
#endif

    void LevelLoaded(GlobalEvents.LevelLoaded evnt)
    {
        Debug.Log(evnt.levelName);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F2))
        {
            Game.World.NextLevel("MyMod", "EnterPoint", true, false);
        }
    }
}
