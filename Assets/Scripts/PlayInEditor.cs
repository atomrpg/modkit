using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-999)]
public class PlayInEditor : MonoBehaviour
{
    public Light _sunTestLight = null;

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

        //fallback replace detect
        foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {            
            foreach (EntityComponent r in obj.GetComponentsInChildren<EntityComponent>())
            {
                if (!IsValidEntityObject(r.gameObject))
                {
                    var prefab = r.Entity.Prototype.Prefab;
                    var copy = Instantiate<GameObject>(r.Entity.Prototype.Prefab);
                    copy.name = r.name;
                    copy.GetComponent<EntityComponent>().SetEntity(r.Entity);
                    r.gameObject.SetActive(false);
                    r.gameObject.name = "temp";
                }
            }
        }

        if(_sunTestLight != null)
        {
            _sunTestLight.gameObject.SetActive(false);
        }
    }

    bool IsValidEntityObject(GameObject go)
    {
        return !PrefabUtility.IsPartOfAnyPrefab(go) && go.GetComponentsInChildren<Collider>(true).Length > 0;
    }

    private void OnDestroy()
    {
        ResourceManager.Reset();
        ResourceManager.SetAssetGetPathCallback(null);
    }
}
