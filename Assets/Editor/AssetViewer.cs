using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class AssetViewer : EditorWindow
{
    [MenuItem("Game/Asset Viewer")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<AssetViewer>();
        window.ShowWindow();
    }

    private void ShowWindow()
    {
        base.Show();
    }

    private Vector2 scrollPosition = Vector2.zero;

    private GUISkin guiSkin;

    private string searchText = "";

    private int selectedCategoryIdx = -1;
    private string[] categoryNames;

    private void OnEnable()
    {
        guiSkin = (GUISkin)Resources.Load("AssetViewer");
        if (AssetViewerDB.IsLoaded)
        {
            HandleDataUpdated();
        }
        AssetViewerDB.OnUpdated += HandleDataUpdated;
    }

    private void OnDisable()
    {
        AssetViewerDB.OnUpdated -= HandleDataUpdated;
    }

    private void HandleDataUpdated()
    {
        categoryNames = new []{"ALL"}.Concat(AssetViewerDB.AssetCategories).ToArray();
        selectedCategoryIdx = 0;
        searchText = "";
    }

    private Rect actualCanvas
    {
        get { return new Rect(0, 0, position.width, position.height); }
    }

    Texture2D GetThumbnail(Object obj)
    {
        var proto = obj as EntityProto;
        if (proto != null)
        {
            Sprite sp = proto.Icon;
            if (sp)
                return sp.texture;
        }

        return AssetPreview.GetMiniThumbnail(obj);
    }

    const int ITEM_WIDTH = 100;
    const int ITEM_HEIGHT = 80;

    private void OnGUI()
    {
        GUISkin lastSkin = GUI.skin;
        GUI.skin = guiSkin;

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        if (GUI.Button(EditorGUILayout.GetControlRect(false),
            new GUIContent("Reload", "Reload assets from source bundle"),
            lastSkin.button))
        {
            AssetViewerDB.Load();
            guiSkin = (GUISkin)Resources.Load("AssetViewer");
            GUI.skin = guiSkin;
        }

        searchText = EditorGUILayout.TextField(searchText);
        if (categoryNames?.Length > 0)
        {
            selectedCategoryIdx = EditorGUILayout.Popup(selectedCategoryIdx, categoryNames);
        }

        EditorGUILayout.EndHorizontal();

        GUI.Box(actualCanvas, "", "scrollview");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        Rect scrollRect = new Rect(scrollPosition.x, scrollPosition.y, position.width, position.height);

        EditorGUILayout.BeginHorizontal();

        Rect lastRect = new Rect(0, 0, ITEM_WIDTH, ITEM_HEIGHT);

        int hCount = -1;
        int hCountMax = Mathf.FloorToInt(scrollRect.width / ITEM_WIDTH);

        foreach (var loadedAsset in AssetViewerDB.LoadedAssets)
        {
            var asset = loadedAsset.Asset;
            var assetName = loadedAsset.AssetName;

            // Name filter
            if (!string.IsNullOrWhiteSpace(searchText) && !asset.name.ToLower().Contains(searchText.ToLower()))
            {
                continue;
            }

            // Category filter
            if (selectedCategoryIdx > 0 && loadedAsset.AssetCategory != categoryNames[selectedCategoryIdx])
            {
                continue;
            }

            bool isVirtual = false;

            if (++hCount == hCountMax)
            {
                EditorGUILayout.EndHorizontal();
                lastRect.y += ITEM_HEIGHT;
                lastRect.x = 0;
                isVirtual = !scrollRect.Overlaps(lastRect);
                if (isVirtual)
                {
                    GUILayout.Space(ITEM_HEIGHT);
                }
                EditorGUILayout.BeginHorizontal();
                hCount = 0;
            }
            else
            {
                lastRect.x += ITEM_HEIGHT;
                isVirtual = !scrollRect.Overlaps(lastRect);
                if (isVirtual)
                {
                    GUILayout.Space(ITEM_WIDTH);
                }
            }

            if (isVirtual)
            {
                continue;
            }

            Texture2D t = GetThumbnail(asset);

            GUILayout.Box(new GUIContent(asset.name, t, assetName), GUILayout.Height(80), GUILayout.Width(100));

            HandleAssetEvents(asset, assetName);
        }

        if (hCount != hCountMax)
        {
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        GUI.skin = lastSkin;
    }

    private void HandleAssetEvents(Object asset, string assetName)
    {
        if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag)
        {
            Rect r = GUILayoutUtility.GetLastRect();
            Vector2 v = Event.current.mousePosition;
            if (r.Contains(v))
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    DragAndDrop.PrepareStartDrag();
                    Object[] objectReferences = new Object[1] { asset };
                    DragAndDrop.objectReferences = objectReferences;
                    DragAndDrop.StartDrag("Dragging Asset");
                }
                else
                {
                    if (Event.current.button == 1)
                    {
                        var menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Download"), false, () => DownloadAsset(asset, assetName));
                        menu.ShowAsContext();
                    }
                    else
                    {

                        Selection.activeObject = asset;
                    }
                }
            }
        }
    }

    private void DownloadAsset(Object asset, string assetName)
    {
        string dataPath = Application.dataPath.ToLower().Replace("/assets", "/");
        string p = Path.GetDirectoryName(dataPath + assetName);
        System.IO.Directory.CreateDirectory(p);
        AssetDatabase.Refresh();

        var textAsset = asset as TextAsset;
        if (textAsset != null)
        {
            File.WriteAllText(dataPath + assetName, textAsset.text);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<TextAsset>(assetName);
        }
        else
        {
            AssetDatabase.CreateAsset(Instantiate(asset), assetName);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetName);
        }
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
