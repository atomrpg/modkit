using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
class AssetViwerDB
{
    public static Dictionary<Object, string> loadedAssets = new Dictionary<Object, string>();
    static AssetViwerDB()
    {
        Load();
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
                        loadedAssets.Add(bundle.LoadAsset(asset), asset);
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

public class AssetViewer : EditorWindow
{
    [MenuItem("Game/Asset Viewer")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<AssetViewer>();
        window.ShowWindow();
    }

    void ShowWindow()
    {
        base.Show();
    }

    Vector2 scrollPosition = Vector2.zero;

    private GUISkin guiSkin;
    private void OnEnable()
    {
        guiSkin = (GUISkin)Resources.Load("AssetViewer");
    }

    void OnGUI()
    {
        GUISkin lastSkin = GUI.skin;
        GUI.skin = guiSkin;

       if(GUILayout.Button("Reload"))
        {
            AssetViwerDB.Load();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var kv in AssetViwerDB.loadedAssets)
        {
            var obj = kv.Key;

            if(!(obj is EntityProto))
            {
                continue;
            }


            EditorGUILayout.BeginHorizontal();

            Texture2D t = AssetPreview.GetMiniThumbnail(obj);

            if (obj is EntityProto && ((EntityProto)obj).Icon != null)
            {
                t = ((EntityProto)obj).Icon.texture;
            }

            GUILayout.Box(new GUIContent(obj.name, t), GUILayout.Height(100), GUILayout.Width(100));

            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag)
            {
                Rect r = GUILayoutUtility.GetLastRect();
                Vector2 v = Event.current.mousePosition;
                if (r.Contains(v))
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        DragAndDrop.PrepareStartDrag();
                        Object[] objectReferences = new Object[1] { obj };
                        DragAndDrop.objectReferences = objectReferences;
                        DragAndDrop.StartDrag("Dragging Asset");
                    }
                    else
                    {
                        if (Event.current.button == 1)
                        {
                            var menu = new GenericMenu();

                            menu.AddItem(new GUIContent("Download"), false, delegate
                            {
                                string dataPath = Application.dataPath.ToLower().Replace("/assets", "/");
                                string p = Path.GetDirectoryName(dataPath + kv.Value);
                                System.IO.Directory.CreateDirectory(p);
                                AssetDatabase.Refresh();

                                if (obj is TextAsset)
                                {
                                    File.WriteAllText(dataPath + kv.Value, ((TextAsset)obj).text);
                                }
                                else
                                {
                                    AssetDatabase.CreateAsset(Instantiate(obj), kv.Value);
                                }
                            });
                            menu.ShowAsContext();
                        }
                        else
                        {

                            Selection.activeObject = obj;
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        GUI.skin = lastSkin;
    }
}
