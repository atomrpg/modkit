using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class PlayInEditor : MonoBehaviour
{
    void Awake()
    {
        //AssetBundle.UnloadAllAssetBundles(true);
        ResourceManager.Reset();
        ResourceManager.SetAssetGetPathCallback(null);
        AssetBundle gameBundle = null;
        
        foreach (var f in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (f != null)
            {
                ResourceManager.AddBundle("", f);
            }
        }

        foreach (string f in Directory.GetFiles(Application.streamingAssetsPath + "/PIE"))
        {
            if (!Path.HasExtension(f))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(f);
                ResourceManager.AddBundle("", bundle);

                if (f.IndexOf("game") > 0)
                {
                    gameBundle = bundle;
                }
            }
        }

        GameObject game = gameBundle.LoadAsset<GameObject>("Game");
        Instantiate(game);

        GameStorage.SetInt("PostImageEffects", 0);

        HBAOPlus.useHBOAPlus = false;
    }
}
