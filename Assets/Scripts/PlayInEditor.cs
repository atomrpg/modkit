using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSon;
using System.Reflection;
using System.Runtime.CompilerServices;


public class PlayInEditor : MonoBehaviour
{
    void Start()
    {
        AssetBundle.UnloadAllAssetBundles(true);
        ResourceManager.Reset();
        ResourceManager.SetAssetGetPathCallback(null);
        ResourceManager.AddBundle("game", AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/entities"));

        AssetBundle asb = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/game");
        GameObject gm = asb.LoadAsset<GameObject>("Assets/Game.prefab");
        ResourceManager.AddBundle("game", asb);
        Instantiate(gm);
        GameStorage.SetInt("PostImageEffects", 0);

        HBAOPlus.useHBOAPlus = false;
    }
}
