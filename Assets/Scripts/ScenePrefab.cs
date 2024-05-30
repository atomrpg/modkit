using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class ScenePrefab : MonoBehaviour
{
    public string sceneName = "";
    public string path = "";

    GameObject _instance = null;

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        if (sceneName.Length > 0 && _instance == null)
        {
            var resPath = RefUtils.ConvertAssetPathToResPath(sceneName, "");

            var prefab = ResourceManager.Load<GameObject>(resPath, ResourceManager.EXT_PREFAB);

            if(prefab == null)
            {
                Debug.Log("ScenePrefab, Prefab not found: " + resPath);
                return;
            }
            var go = prefab.transform.Find(path).gameObject;

            if (go == null)
            {
                Debug.Log("ScenePrefab, GameObject not found: " + path);
                return;
            }

            _instance = Instantiate(go, transform);
            _instance.transform.localPosition = Vector3.zero;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                NotEditableTransform(_instance);
#endif
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        Spawn();
    }

    void NotEditableTransform(GameObject go)
    {
        for (int i = go.transform.childCount - 1; i >= 0; --i)
        {
            NotEditableTransform(go.transform.GetChild(i).gameObject);
        }

        go.hideFlags |= HideFlags.NotEditable | HideFlags.DontSaveInEditor;
    }
#endif
}

