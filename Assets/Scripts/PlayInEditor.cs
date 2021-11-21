//#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[DefaultExecutionOrder(-999)]
public class PlayInEditor : MonoBehaviour
{
    public Light _sunTestLight = null;

    public string spawnScene = null;

    List<GameObject> tempSceneObj = new List<GameObject>();

    void Awake()
    {
        if (EditorApplication.isPlaying)
        {
            ResourceManager.Reset();
            ResourceManager.SetAssetGetPathCallback(null);
            AssetBundle gameBundle = null;

            foreach (var f in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (f != null)
                {
                    ResourceManager.AddBundle(f.name, f);

                    if (f.name.IndexOf("editor") >= 0) //TODO
                    {
                        gameBundle = f;
                    }
                }
            }

            GameObject game = gameBundle.LoadAsset<GameObject>("Game");
            Instantiate(game);

            GameStorage.SetInt("PostImageEffects", 0);

            HBAOPlus.useHBOAPlus = false;

            if (spawnScene.Length > 0)
            {
                SpawnScene();
            }

            //fallback replace detect
            foreach (var obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (EntityComponent r in obj.GetComponentsInChildren<EntityComponent>())
                {
                    if (!IsValidEntityObject(r.gameObject))
                    {
                        var prefab = r.Entity.Prototype.Prefab;

                        var copy = Instantiate<GameObject>(r.Entity.Prototype.Prefab, r.gameObject.transform.position, r.gameObject.transform.rotation);
                        copy.transform.localScale = r.transform.lossyScale;
                        copy.name = r.name;

                        copy.GetComponent<EntityComponent>().SetEntity(r.Entity);
                        r.gameObject.SetActive(false);
                        r.gameObject.name += "_temp";
                    }
                }
            }

            if (_sunTestLight != null)
            {
                _sunTestLight.gameObject.SetActive(false);
            }
        }
        else
        {

        }
    }


    bool _requestShowScene = false;

    void Start()
    {
        if (!EditorApplication.isPlaying)
        {
                _requestShowScene = true;
        }
    }

    private void Update()
    {
        if (_requestShowScene && spawnScene.Length > 0 && ResourceManager.bundles.Count > 0)
        {
            _requestShowScene = false;

            SpawnScene();
        }
    }

    void CleanupTemp()
    {
        Debug.Log("CleanupTemp");

        if (tempSceneObj.Count > 0)
        {
            foreach (var goDel in tempSceneObj)
            {
                if (goDel != null)
                {
                    DestroyImmediate(goDel);
                }
            }

            tempSceneObj.Clear();
        }
    }

    public void SpawnScene()
    {
        CleanupTemp();

        var resPath = RefUtils.ConvertAssetPathToResPath(spawnScene, "");

        var prefab = ResourceManager.Load<GameObject>(resPath, ResourceManager.EXT_PREFAB); 

        var go = Instantiate(prefab);
        go.name = "Level";

        NotEditableTransform(go);

        var items = ResourceManager.Load<TextAsset>(RefUtils.ConvertAssetPathToResPath(spawnScene.Replace("level.prefab", "entities.json"), ""), ResourceManager.EXT_SCRIPT);

        foreach(var goEnt in Game.LoadItems("", items.text))
        {
            NotEditableTransform(goEnt);
        }
    }

    void NotEditableTransform(GameObject go)
    {
        for (int i = go.transform.childCount - 1; i >= 0; --i)
        {
            NotEditableTransform(go.transform.GetChild(i).gameObject);
        }

        tempSceneObj.Add(go);
        go.hideFlags |= HideFlags.NotEditable | HideFlags.DontSaveInEditor;
    }

    bool IsValidEntityObject(GameObject go)
    {
        return !PrefabUtility.IsPartOfAnyPrefab(go) && go.GetComponentsInChildren<Collider>(true).Length > 0;
    }
    /*
    [InitializeOnEnterPlayMode]
    static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
    {
        if (instance != null)
        {
            instance.CleanupTemp();
        }
    }
    */

    private void OnDestroy()
    {
        CleanupTemp();

        if (EditorApplication.isPlaying)
        {
            ResourceManager.Reset();
            ResourceManager.SetAssetGetPathCallback(null);
        }
    }
}
//#endif