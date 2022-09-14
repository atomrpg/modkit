﻿//#define SUPPORT_LEVEL_BUNDLE // Managed via build mod tool.
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

    [HarmonyPatch(typeof(CharacterComponent))]
    [HarmonyPatch("Attack")]
    [HarmonyPatch(new System.Type[] {})]
    class Patch_CharacterComponent_Attack
    {
        static int ammoCount = 0;

        static bool Prefix(CharacterComponent __instance)
        {
            if(__instance.Character.Weapon != null && __instance.Character.Weapon.Ammo != null)
            {
                ammoCount = __instance.Character.Weapon.Ammo.Count;
            }

            return true;
        }

        static void Postfix(CharacterComponent __instance)
        {
            if (ammoCount > 0 && __instance.HasPerk((CharacterStats.Perk)ModCharacterPerk.REFILL_AMMO))
            {
                var ammo = __instance.FindAmmo(__instance.Character.Weapon);
                if (ammo != null)
                {
                    ammo.Count += (ammoCount - __instance.Character.Weapon.Ammo.Count);
                }
            }

            ammoCount = 0;
        }
    }

    void Start()
    {
        var assembly = GetType().Assembly;
        modName = assembly.GetName().Name;
        dir = System.IO.Path.GetDirectoryName(assembly.Location);
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
