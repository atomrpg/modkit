using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("TR")] // ENTER MOD TITLE


public class ModEntryPoint : MonoBehaviour, ILocalizedText // ModEntryPoint - RESERVED LOOKUP NAME
{
    void ILocalizedText.UpdateText()
    {
        PatchLangList();
    }

    void PatchLangList()
    {
        Localization.LoadStrings("patch_strings_");
    }

    void Start()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");
        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));

        GlobalEvents.AddListener<GlobalEvents.GameStart>(GameLoaded);

        {
            SettingLanguage.AvaibleLanguage.Add("tr");
        }

        /* out of date
        {
            var field = typeof(Localization).GetField("_cultureInfo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            field.SetValue(null, new System.Globalization.CultureInfo("en-US"));
        }
        */
    }

    void GameLoaded(GlobalEvents.GameStart evnt)
    {
        PatchLangList();
    }
}

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
public class Editor_Patch
{
    static Editor_Patch()
    {
        SettingLanguage.AvaibleLanguage.Add("tr");

        Debug.Log("Editor: patch");
    }
}
#endif
