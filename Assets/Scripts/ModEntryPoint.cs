//#define SUPPORT_LEVEL_BUNDLE // Managed via build mod tool.
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

    void Start()
    {
        var assembly = GetType().Assembly;
        modName = assembly.GetName().Name;
        dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");

        GlobalEvents.AddListener<GlobalEvents.GameStart>(GameLoaded);
        GlobalEvents.AddListener<GlobalEvents.LevelLoaded>(LevelLoaded);
		
		LoadModBundle();
    }

    void LoadModBundle()
    {
#if UNITY_EDITOR
        // skip bundle loading in PIE mode
#else
        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));
#if SUPPORT_LEVEL_BUNDLE
        AssetBundle assetBundle = AssetBundle.LoadFromFile(dir + "/" + modName);
        if (assetBundle != null)
        {
            manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        GlobalEvents.AddListener<GlobalEvents.PrepareNextLevel>(PrepareNextLevel);
#endif
#endif
    }

#if SUPPORT_LEVEL_BUNDLE
    AssetBundleManifest manifest;
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

                Debug.Log("Load shared level bundle: " + bundle);

                b = AssetBundle.LoadFromFile(dir + "/" + bundle);
                if (b != null)
                {
                    ResourceManager.AddBundle(bundle, b);
                    lastLevelBundle.Add(b);
                }
            }

            Debug.Log("Load level bundle: " + evnt.levelName);
            b = AssetBundle.LoadFromFile(dir + "/" + modName + "_" + evnt.levelName);
            if (b != null)
            {
                ResourceManager.AddBundle(modName, b);
                lastLevelBundle.Add(b);
            }
        }
    }
#endif

    void GameLoaded(GlobalEvents.GameStart evnt)
    {
        Localization.LoadStrings("mymod_strings_");
        Game.World.console.DeveloperMode();
    }

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
