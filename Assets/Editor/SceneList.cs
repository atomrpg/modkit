//#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.Reflection;

public class SceneList : EditorWindow
{
    [MenuItem("Game/Scene List")]
    static public void Init()
    {
        GetWindow<SceneList>().Show();
    }

    public void Awake()
    {
        Refresh();
    }



    void Refresh()
    {
        sceneList.Clear();

        foreach (var bundle in ResourceManager.bundles)
        {

            if (bundle is ResourceManager.UnityBundle unityBundle)
            {
                //if (unityBundle.modName == "levels")
                {
                    foreach (var prefab in unityBundle.GetAllAssetNames())
                    {
                        if (prefab.Contains("/levels/") && prefab.Contains(".prefab"))
                        {
                            sceneList.Add(prefab, false);
                        }
                    }
                }
            }
        }
    }

    Dictionary<string, bool> sceneList = new Dictionary<string, bool>();

    Vector2 listScrollPos = Vector2.zero;

    string filter = "";


    public void OnGUI()
    {
        if (GUILayout.Button("[Refresh]"))
        {
            Refresh();
        }

        EditorGUILayout.BeginHorizontal();
        filter = EditorGUILayout.TextField(filter);
        if(GUILayout.Button("[X]"))
        {
            filter = "";
        }
        EditorGUILayout.EndHorizontal();
        //GUILayout.Space(20);

        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos);
   
        string filterLow = filter.ToLower();
        foreach (var scene in sceneList)
        {
            if (filter.Length == 0 || scene.Key.ToLower().Contains(filterLow))
            {
                if (GUILayout.Button(scene.Key))
                {
                    var playInEditor = GameObject.FindObjectOfType<PlayInEditor>();

                    if(playInEditor == null)
                    {
                        var goPIE = new GameObject("PlayInEditor");
                        playInEditor = goPIE.AddComponent<PlayInEditor>();
                    }

                    playInEditor.spawnScene = scene.Key;
                    playInEditor.SpawnScene();
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
//#endif