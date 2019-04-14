using UnityEngine;
using System.Reflection;

[assembly: AssemblyTitle("My Avatar")] // ENTER MOD TITLE


public class ModEntryPoint : MonoBehaviour // ModEntryPoint - RESERVED LOOKUP NAME
{
    readonly bool FILE_SYSTEM_LOAD = true;
    void Start()
    {
        var assembly = GetType().Assembly;
        string modName = assembly.GetName().Name;
        string dir = System.IO.Path.GetDirectoryName(assembly.Location);
        Debug.Log("Mod Init: " + modName + "(" + dir + ")");

        if (FILE_SYSTEM_LOAD)
        {
            ResourceManager.AddBundle(modName, new FileSystemBundle(dir + "/" + "Avatar_Res"));
        }
        else
        {
            ResourceManager.AddBundle(modName, AssetBundle.LoadFromFile(dir + "/" + modName + "_resources"));
        }
    }
}
