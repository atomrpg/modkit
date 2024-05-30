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
    public class LoadedAsset
    {
        private Object _asset;
        public Object Asset { get { return (_asset == null) ? _asset = Bundle.LoadAsset(AssetPath) : _asset; } }
        
        public AssetBundle Bundle { get; set; }

        public string AssetName { get; set; }
        public string AssetPath { get; set; }

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
        if (!Application.isPlaying)
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange obj)
    {
        if(obj == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.update += Init;
        }
    }

    static void Init()
    {
        if (!EditorApplication.isCompiling)
        {
            EditorApplication.update -= Init;
            Load();

            string modName = typeof(ModEntryPoint).Assembly.GetName().Name;
            LocalizationTools.stringMask = modName + "_" + LocalizationTools.stringMask;
            LocalizationTools.textMask = modName + "_" + LocalizationTools.textMask;
        }
    }

    public static string GetBundleAssetPath(UnityEngine.Object obj)
    {
        foreach(var la in loadedAssets)
        {
            if(la.Asset == obj)
            {
                return la.AssetPath;
            }
        }

        return string.Empty;
    }


    public static void Load()
    {
        if (Application.isPlaying)
        {
            return;
        }

        loadedAssets.Clear();
        assetCategories.Clear();
        AssetBundle.UnloadAllAssetBundles(true);

        ResourceManager.Reset();
        ResourceManager.SetAssetGetPathCallback(GetBundleAssetPath);

        var categoriesSet = new HashSet<string>();

        LoadBundles(Application.streamingAssetsPath, categoriesSet);

        var gdir = PlayerPrefs.GetString("GAME_CONTENT_DIR", "");
        LoadBundles(gdir, categoriesSet);

        assetCategories.AddRange(categoriesSet.OrderBy(x => x));
        EditorUtility.ClearProgressBar();

        IsLoaded = true;
        OnUpdated?.Invoke();
    }

    private static void LoadBundles(string path, HashSet<string> categoriesSet)
    {
        if(path.Length == 0 || !Directory.Exists(path))
        {
            return;
        }

        int progress = 0;

        var files = Directory.GetFiles(path);
        foreach (string f in Directory.GetFiles(path))
        {
            try
            {
                if (EditorUtility.DisplayCancelableProgressBar("Asset bundle", "Load Bundle: " + f, (float)progress / files.Length))
                {
                    break;
                }

                ++progress;

                if (!Path.HasExtension(f) || Path.GetExtension(f) == ".bundle")
                {
                    AssetBundle bundle = AssetBundle.LoadFromFile(f);
                    ResourceManager.AddBundle(bundle.name, bundle);

                    string[] allAssetNames = bundle.GetAllAssetNames();
                    foreach (var assetPath in allAssetNames)
                    {
                        if (IsAsset(assetPath))
                        {
                            //Object obj = bundle.LoadAsset(asset);
                            //if (obj is ScriptableObject || obj is TextAsset)
                            //if(IsAsset(asset))
                            {
                                var category = GetCategoryFromAssetName(assetPath);

                                if(assetPath.Contains("/levels")) // union one category
                                {
                                    category = "levels";
                                }

                                loadedAssets.Add(new LoadedAsset
                                {
                                    Bundle = bundle,
                                    AssetName = Path.GetFileName(assetPath),
                                    AssetPath = assetPath,
                                    AssetCategory = category,
                                });
                                categoriesSet.Add(category);
                            }
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("Bundle skip");
            }
        }
    }

    private static bool IsAsset(string asset)
    {
        return asset.IndexOf(".asset") >= 0 || asset.IndexOf(".json") >= 0;
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
