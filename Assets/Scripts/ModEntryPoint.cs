using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;

//[assembly: AssemblyTitle("My Mod")] // ENTER MOD TITLE
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    void Start()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");
        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));
        GlobalEvents.AddListener<GlobalEvents.GameStart>(GameLoaded);
        GlobalEvents.AddListener<GlobalEvents.LevelLoaded>(LevelLoaded);
    }

    void GameLoaded(GlobalEvents.GameStart evnt)
    {
        Localization.LoadStrings("mymod_strings_");
        Localization.LoadTexts("mymod_text_");
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
#if UNITY_EDITOR

[InitializeOnLoad]
internal class LocalizationPreviewInEditor
{
    static LocalizationPreviewInEditor()
    {
        EditorApplication.update += Init;
    }


    static void Init()
    {
        if (!EditorApplication.isCompiling && ResourceManager.bundles.Count > 0)
        {
            EditorApplication.update -= Init;
            Localization.Setup("ru", false);
            Localization.LoadStrings("mymod_strings_");
            Localization.LoadTexts("mymod_text_");
        }
    }
}
#endif