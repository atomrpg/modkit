using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Machine Translation")] // ENTER MOD TITLE


public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    void Start()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");
        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));

        //todo:AtomTeam make public
        var field = typeof(SettingLanguage).GetField("_lang", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        List<string> langList = field.GetValue(null) as List<string>;
        langList.Add("ja");
        langList.Add("ch");
    }
}
