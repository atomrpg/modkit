using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ModBuilder : EditorWindow
{
    string _modName = "MyMod";
    [MenuItem("Game/Build Mod")]
    static void BuildMod()
    {
        var window = EditorWindow.GetWindow<ModBuilder>();
        window.Show();
    }

    private void OnGUI()
    {
        _modName = EditorGUILayout.TextField("Mod Name", _modName);

        if (GUILayout.Button("BUILD"))
        {
            if (_modName.Length > 0)
            {   
                string assetBundleDirectory = "Temp/ModBuild";
                if (!Directory.Exists(assetBundleDirectory))
                {
                    Directory.CreateDirectory(assetBundleDirectory);
                }
                BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.DisableWriteTypeTree, BuildTarget.StandaloneWindows64);

                //copy dll
                string modsFolder = Application.persistentDataPath + "/../../AtomTeam/Atom/Mods";
                string dllName = typeof(ModEntryPoint).Assembly.GetName().Name;
                Debug.Log(dllName);

                if (!Directory.Exists(modsFolder))
                {
                    Directory.CreateDirectory(modsFolder);
                }

                Copy("Library/ScriptAssemblies/" + dllName + ".dll", modsFolder + "/" + dllName + ".dll");

                //copy res
                string modResFolder = modsFolder + "/" + _modName;

                if (!Directory.Exists(modResFolder))
                {
                    Directory.CreateDirectory(modResFolder);
                }

                string dataAsset = Application.dataPath;
                const string PATH_TO_ASSETS = "/assets";
            
                int index = dataAsset.ToLower().IndexOf(PATH_TO_ASSETS);
                dataAsset = dataAsset.Remove(index, PATH_TO_ASSETS.Length);
                Copy(dataAsset + "/Temp/ModBuild/resources", modResFolder + "/resources");
                Copy(dataAsset + "/Temp/ModBuild/resources.manifest", modResFolder + "/resources.manifest");

                EditorUtility.RevealInFinder(modResFolder);
            }
        }
    }

    void Copy(string src, string dst)
    {
        Debug.Log("Copy " + src + " -> " + dst);
        File.Copy(src, dst, true);
    }
}
