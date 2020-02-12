using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using Harmony;
using System.Reflection;
using System.Runtime.CompilerServices;

//[assembly: AssemblyTitle("My Mod")] // ENTER MOD TITLE


public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    void ScriptsPatch()
    {
        //Debug.Log("!!!Patch begin");
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;

        var harmony = HarmonyInstance.Create("com.atomrpg.mod." + modName);
        harmony.PatchAll();
        //Debug.Log("!!!Patch end");
    }

    [HarmonyPatch(typeof(Weapon))]
    [HarmonyPatch("GetChance")]
    [HarmonyPatch(new System.Type[] { typeof(Character), typeof(ShotMode), typeof(int), typeof(int) })]
    class Patch
    {
        /*
        static bool Prefix(Character c, ShotMode shotMode, int dist, int targetAC)
        {
            //Debug.Log("!!!Prefix");
            //return true; // do original code
            //return false;// skip original code
        }
        */

        static void Postfix(ref int __result)
        {
            //Debug.Log("!!!Postfix");
            __result += 99; // +99% chance to hit
        }
    }

    void Start()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");
        ScriptsPatch();
    }
}

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
public class Editor_Harmony_Patch_All {
    static Editor_Harmony_Patch_All()
    {
        var harmony = HarmonyInstance.Create("com.atomrpg.editor");
        harmony.PatchAll();
        Debug.Log("Editor: Harmony patch all");
    }
}
#endif
