using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;

//[assembly: AssemblyTitle("My Mod")] // ENTER MOD TITLE


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
        Localization.LoadStrings("car_string_");
        Localization.LoadTexts("car_text_");
        Game.World.console.DeveloperMode();
    }

    void LevelLoaded(GlobalEvents.LevelLoaded evnt)
    {
        Debug.Log(evnt.levelName);
    }

    void Update()
    {
        if(Game.World && Game.World.Vehicle != null)
        {
            Game.World.Vehicle.maxFuel = 77; //77l
            Game.World.Vehicle.fuelConsumption = 0.015f; //+17% fuel consumption
        }
    }
}
