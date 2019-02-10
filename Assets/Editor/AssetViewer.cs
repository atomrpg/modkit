using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

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

    private string searchText = "";

    private void OnGUI()
    {
        GUISkin lastSkin = GUI.skin;
        GUI.skin = guiSkin;

        if (GUILayout.Button("Reload"))
        {
            AssetViewerDB.Load();
            guiSkin = (GUISkin)Resources.Load("AssetViewer");
            GUI.skin = guiSkin;
        }

        searchText = GUILayout.TextField(searchText, 100);

        GUI.Box(actualCanvas, "", "scrollview");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        Rect scrollRect = new Rect(scrollPosition.x, scrollPosition.y, position.width, position.height);

        EditorGUILayout.BeginHorizontal();

        Rect lastRect = new Rect(0, 0, ITEM_WIDTH, ITEM_HEIGHT);

        int hCount = -1;
        int hCountMax = Mathf.FloorToInt(scrollRect.width / ITEM_WIDTH);

        foreach (var kv in AssetViewerDB.LoadedAssets)
        {
            var asset = kv.Key;
            var assetName = kv.Value;

            if (!string.IsNullOrWhiteSpace(searchText) && !asset.name.Contains(searchText))
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

            GUILayout.Box(new GUIContent(asset.name, t), GUILayout.Height(80), GUILayout.Width(100));

            HandleAssetEvents(asset, assetName);
        }

        EditorGUILayout.EndHorizontal();

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
