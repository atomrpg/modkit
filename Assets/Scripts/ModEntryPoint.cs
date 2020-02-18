using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using Harmony;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine.Video;

//[assembly: AssemblyTitle("My Mod")] // ENTER MOD TITLE


public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    void ScriptsPatch()
    {
        //Debug.Log("!!!Patch begin");
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));

        var harmony = HarmonyInstance.Create("com.atomrpg.mod." + modName);
        harmony.PatchAll();
        //Debug.Log("!!!Patch end");
    }

    [HarmonyPatch(typeof(IntroMovie))]
    [HarmonyPatch("GetAudioTrackIdByLang")]
    [HarmonyPatch(new System.Type[] { typeof(string) })]
    class Patch_IntroMovie_GetAudioTrackIdByLang
    {
        static void Postfix(string lang, ref int __result)
        {
            //if(lang == "de")
            {
                __result = 255; // skip all audio from video clip
            }
        }
    }

    [HarmonyPatch(typeof(IntroMovie))]
    [HarmonyPatch("Prepared")]
    [HarmonyPatch(new System.Type[] { typeof(VideoPlayer) })]
    class Patch_IntroMovie_Prepared
    {
        static bool Prefix(VideoPlayer __instance)
        {
            Debug.Log("!!!Patch audio track for VideoPlayer");
            var videoPlayer = __instance.GetComponent<VideoPlayer>();
            var audioPlayer = __instance.GetComponent<AudioSource>();
            string clipName = videoPlayer.clip.name;
            Debug.Log("!!!Patch Try to load override audio videotrack from: " + "Movie/" + clipName + ".ogg");
            AudioClip audioClip = ResourceManager.Load<AudioClip>("Movie/" + clipName, ResourceManager.EXT_AUDIO);
            if (audioClip != null) // repleace current audio clip
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

                audioPlayer.clip = ResourceManager.Load<AudioClip>("Movie/" + clipName, ResourceManager.EXT_AUDIO);
                audioPlayer.Play();
            }

            return true;
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
