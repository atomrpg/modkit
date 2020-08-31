using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Video;

[assembly: AssemblyTitle("German Translation")]


public class ModEntryPoint : MonoBehaviour, ILocalizedText // ModEntryPoint - RESERVED LOOKUP NAME
{


    void Start()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");

        if (!SettingLanguage.AvaibleLanguage.Contains("de"))
        {
            SettingLanguage.AvaibleLanguage.Add("de");
        }

        ScriptsPatch();
        ResourceManager.StreamingAssetsPath = dir + "/" + modName + "_External";

    }

    void ScriptsPatch()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));
   
    }

    void ILocalizedText.UpdateText()
    {
        PatchLangList();
    }

    void PatchLangList()
    {
        Localization.LoadStrings("patch_strings_");
    }

    void GameLoaded(GlobalEvents.GameStart evnt)
    {
        PatchLangList();
    }
}
