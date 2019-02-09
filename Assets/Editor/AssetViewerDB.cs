using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
internal class AssetViewerDB
{
    public static IReadOnlyDictionary<Object, string> LoadedAssets => loadedAssets;

    private static readonly Dictionary<Object, string> loadedAssets = new Dictionary<Object, string>();

    static AssetViewerDB()
    {
        EditorApplication.update += Init;
    }

    static void Init()
    {
        if (!EditorApplication.isCompiling)
        {
            EditorApplication.update -= Init;
            Load(); 
        }
    }

    public static void Load()
    {
        loadedAssets.Clear();
        AssetBundle.UnloadAllAssetBundles(true);

        ResourceManager.Reset();

        foreach (string f in Directory.GetFiles(Application.streamingAssetsPath))
        {
            try
            {
                if (!Path.HasExtension(f))
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(f);
                    ResourceManager.AddBundle(bundle);

                    string[] allAssetNames = bundle.GetAllAssetNames();
                    int progress = 0;
                    foreach (var asset in allAssetNames)
                    {
                        Object obj = bundle.LoadAsset(asset);
                        if (obj is ScriptableObject || obj is TextAsset)
                        {
                            loadedAssets.Add(obj, asset);
                        }

                        if (EditorUtility.DisplayCancelableProgressBar("Asset bundle", "Load Asset", (float)progress / allAssetNames.Length))
                        {
                            break;
                        }

                        ++progress;
                    }
                }
            }
            catch
            {
                Debug.Log("Bundle skip");
            }
        }
        EditorUtility.ClearProgressBar();
    }
}
