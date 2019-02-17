using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal class AssetViewerDB
{
    public struct LoadedAsset
    {
        public Object Asset { get; set; }

        public string AssetName { get; set; }

        public string AssetCategory { get; set; }
    }

    public static event Action OnUpdated;

    public static bool IsLoaded { get; private set; }

    public static IReadOnlyList<LoadedAsset> LoadedAssets => loadedAssets;
    private static readonly List<LoadedAsset> loadedAssets = new List<LoadedAsset>();

    public static IReadOnlyList<string> AssetCategories => assetCategories;
    private static readonly List<string> assetCategories = new List<string>();

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

    public static string GetBundleAssetPath(UnityEngine.Object obj)
    {
        foreach(var la in loadedAssets)
        {
            if(la.Asset == obj)
            {
                return la.AssetName;
            }
        }

        return string.Empty;
    }


    public static void Load()
    {
        loadedAssets.Clear();
        assetCategories.Clear();
        AssetBundle.UnloadAllAssetBundles(true);

        ResourceManager.Reset();
        ResourceManager.SetAssetGetPathCallback(GetBundleAssetPath);

        var categoriesSet = new HashSet<string>();

        foreach (string f in Directory.GetFiles(Application.streamingAssetsPath))
        {
            try
            {
                if (!Path.HasExtension(f))
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(f);
                    ResourceManager.AddBundle("", bundle);

                    string[] allAssetNames = bundle.GetAllAssetNames();
                    int progress = 0;
                    foreach (var asset in allAssetNames)
                    {
                        Object obj = bundle.LoadAsset(asset);
                        if (obj is ScriptableObject || obj is TextAsset)
                        {
                            var category = GetCategoryFromAssetName(asset);
                            loadedAssets.Add(new LoadedAsset
                            {
                                Asset = obj,
                                AssetName = asset,
                                AssetCategory = category,
                            });
                            categoriesSet.Add(category);
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

        assetCategories.AddRange(categoriesSet.OrderBy(x => x));
        EditorUtility.ClearProgressBar();

        IsLoaded = true;
        OnUpdated?.Invoke();
    }

    private static string GetCategoryFromAssetName(string assetName)
    {
        var parts = assetName.Split('/');
        var lastFolderName = parts.Length > 1 ? parts[parts.Length - 2] : null;
        return !string.IsNullOrEmpty(lastFolderName)
            ? lastFolderName
            : null;
    }
}
