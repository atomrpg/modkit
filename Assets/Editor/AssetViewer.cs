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

    private Rect actualCanvas
    {
        get { return new Rect(0, 0, position.width, position.height); }
    }

    Texture2D GetThumbnail(Object obj)
    {
        if (obj is EntityProto)
        {
            Sprite sp = ((EntityProto)obj).Icon;
            if (sp)
                return sp.texture;
        }

        return AssetPreview.GetMiniThumbnail(obj);
    }

    const int ITEM_WIDTH = 100;
    const int ITEM_HEIGHT = 80;

    void OnGUI()
    {
        GUISkin lastSkin = GUI.skin;
        GUI.skin = guiSkin;

       if(GUILayout.Button("Reload"))
        {
            AssetViwerDB.Load();
            guiSkin = (GUISkin)Resources.Load("AssetViewer");
            GUI.skin = guiSkin;
        }

        GUI.Box(actualCanvas, "", "scrollview");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        Rect scrollRect = new Rect(scrollPosition.x, scrollPosition.y, position.width, position.height);

        EditorGUILayout.BeginHorizontal();


        Rect lastRect = new Rect(0, 0, ITEM_WIDTH, ITEM_HEIGHT);

        int hCount = -1;
        int hCountMax = Mathf.FloorToInt(scrollRect.width / ITEM_WIDTH);

        foreach (var kv in AssetViwerDB.loadedAssets)
        {
            var obj = kv.Key;

            bool isvirtual = false;

            if (++hCount == hCountMax)
            {
                EditorGUILayout.EndHorizontal();
                lastRect.y += ITEM_HEIGHT;
                lastRect.x = 0;
                isvirtual = !scrollRect.Overlaps(lastRect);
                if (isvirtual)
                {
                    GUILayout.Space(ITEM_HEIGHT);
                }
                EditorGUILayout.BeginHorizontal();
                hCount = 0;
            }
            else
            {
                lastRect.x += ITEM_HEIGHT;
                isvirtual = !scrollRect.Overlaps(lastRect);
                if (isvirtual)
                {
                    GUILayout.Space(ITEM_WIDTH);
                }
            }

            if(isvirtual)
            {
                continue;
            }

            Texture2D t = GetThumbnail(obj);

            GUILayout.Box(new GUIContent(obj.name, t), GUILayout.Height(80), GUILayout.Width(100));

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
                                    AssetDatabase.Refresh();
                                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(kv.Value);
                                }
                                else
                                {
                                    AssetDatabase.CreateAsset(Instantiate(obj), kv.Value);
                                    AssetDatabase.Refresh();
                                    Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(kv.Value);
                                }
                                EditorGUIUtility.PingObject(Selection.activeObject);
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
           
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
        GUI.skin = lastSkin;
    }
}
