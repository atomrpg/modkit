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

    static TextAsset SaveTextAsset(TextAsset textAsset, string path)
    {
        if(textAsset == null)
        {
            return null;
        }
        File.WriteAllText(path, textAsset.text);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
    }

    static Sprite SaveSprite(Sprite sp, string path)
    {
        if(sp == null)
        {
            return null;
        }
        string dir = Path.GetDirectoryName(path);

        Directory.CreateDirectory(dir);

        Texture2D img = sp.texture;
        img.filterMode = FilterMode.Point;
        RenderTexture rt = RenderTexture.GetTemporary(img.width, img.height);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(img, rt);
        Texture2D img2 = new Texture2D(img.width, img.height);
        img2.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
        img2.Apply();
        RenderTexture.active = null;
        img = img2;

        File.WriteAllBytes(path, img.EncodeToPNG());
        AssetDatabase.Refresh();
        //AssetDatabase.AddObjectToAsset(sp, path);
        //AssetDatabase.SaveAssets();

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spritePixelsPerUnit = sp.pixelsPerUnit;
        ti.mipmapEnabled = false;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    public static void RemapObject(object source, object dst, string path)
    {
        System.Reflection.FieldInfo[] ps = source.GetType().GetFields();
        foreach (var item in ps)
        {
            var o = item.GetValue(source);
            var p = dst.GetType().GetField(item.Name);

            if (o != null)
            {
                switch (o.GetType().Name) //Extract from bundle resources and remap
                {
                    case nameof(Sprite):
                        o = SaveSprite((Sprite)o, System.IO.Path.ChangeExtension(path, null) + ".png");
                        break;
                    case nameof(TextAsset):
                        o = SaveTextAsset((TextAsset)o, System.IO.Path.ChangeExtension(path, null) + ".json");
                        break;
                }
            }

            if (p != null)
            {
                System.Type t = System.Nullable.GetUnderlyingType(p.FieldType) ?? p.FieldType;
                object safeValue = (o == null) ? null : System.Convert.ChangeType(o, t);
                p.SetValue(dst, safeValue);
            }
        }
    }

    private void DownloadAsset(Object asset, string assetName)
    {
        string dataPath = Application.dataPath.ToLower().Replace("/assets", "/");
        string p = Path.GetDirectoryName(dataPath + assetName);
        System.IO.Directory.CreateDirectory(p);
        AssetDatabase.Refresh();

        if (asset is TextAsset)
        {
            Selection.activeObject = SaveTextAsset(asset as TextAsset, assetName);
        }
        else if (asset is ScriptableObject)
        {

            var obj = ScriptableObject.CreateInstance(asset.GetType().Name);
            RemapObject(asset, obj, assetName);
            AssetDatabase.CreateAsset(obj, assetName);
            AssetDatabase.Refresh();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(assetName);
        }
        else
        {
            Debug.LogWarning("TODO " + asset.GetType().Name);
        }
    
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
