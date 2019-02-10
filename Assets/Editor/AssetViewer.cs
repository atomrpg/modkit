using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
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

    static ScriptableObject SaveScriptableAsset(ScriptableObject asset, string path)
    {
        CreateDirByAssetPath(path);
        var obj = ScriptableObject.CreateInstance(asset.GetType().Name);
        EditorUtility.CopySerialized(asset, obj);
        RemapObject(obj, path);
        AssetDatabase.CreateAsset(obj, path);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
    }

    static TextAsset SaveTextAsset(TextAsset textAsset, string path)
    {
        if (textAsset == null || textAsset.Equals(null))
        {
            return null;
        }
        File.WriteAllText(path, textAsset.text);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
    }

    static AudioClip SaveAudioClip(AudioClip ac, string path)
    {
        if (ac == null || ac.Equals(null))
        {
            return null;
        }
        string dir = Path.GetDirectoryName(path);

        Directory.CreateDirectory(dir);

        SavWav.Save(path, ac);
        AssetDatabase.Refresh();
        //EditorUtility.ExtractOggFile(ac, path);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    static Sprite SaveSprite(Sprite sp, string path)
    {
        if (sp == null || sp.Equals(null))
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

    static object Convert(object o, string path)
    {
        if(o == null || o.Equals(null))
        {
            return null;
        }

        switch (o.GetType().Name) //Extract from bundle resources and remap
        {
            case nameof(Sprite):
                return SaveSprite((Sprite)o, Path.GetDirectoryName(path) + "/" + ((Sprite)o).name + ".png");
            case nameof(TextAsset):
                return SaveTextAsset((TextAsset)o, Path.GetDirectoryName(path) + "/" + ((TextAsset)o).name  + ".json");
            case nameof(ItemProto):
                return SaveScriptableAsset((ScriptableObject)o, "assets/resources/entities/item/" + ((ScriptableObject)o).name + ".asset");
            case nameof(WeaponProto):
                return SaveScriptableAsset((ScriptableObject)o, "assets/resources/entities/weapon/" + ((ScriptableObject)o).name + ".asset");
            case nameof(ConsumableProto):
                return SaveScriptableAsset((ScriptableObject)o, "assets/resources/entities/consumable/" + ((ScriptableObject)o).name + ".asset");
            case nameof(AmmoProto):
                return SaveScriptableAsset((ScriptableObject)o, "assets/resources/entities/ammo/" + ((ScriptableObject)o).name + ".asset");
            case nameof(UniformProto):
                return SaveScriptableAsset((ScriptableObject)o, "assets/resources/entities/uniform/" + ((ScriptableObject)o).name + ".asset");
            case nameof(ExplosiveProto):
                return SaveScriptableAsset((ScriptableObject)o, "assets/resources/entities/explosive/" + ((ScriptableObject)o).name + ".asset");
            case nameof(EffectProto):
                return SaveScriptableAsset((ScriptableObject)o, "assets/resources/entities/effect/" + ((ScriptableObject)o).name + ".asset");
            case nameof(AudioClip):
                return SaveAudioClip((AudioClip)o, Path.GetDirectoryName(path) + "/" + ((AudioClip)o).name + ".wav");
        }

        return null;
    }

    public static void RemapObject(object source, string path)
    {
        if(source.GetType().IsArray)
        {
            System.Array array = (System.Array)source;
            for (int i = 0; i < array.Length; i++)
            {
                object newObject = Convert(array.GetValue(i), path);
                if (newObject != null)
                {
                    array.SetValue(newObject, i);
                }
                else
                {
                    RemapObject(array.GetValue(i), path);
                }
            }
        }
        else if(source is Enumerable)
        {
            foreach (var listitem in source as IEnumerable)
            {
                RemapObject(listitem, path);
            }
        }

        System.Reflection.FieldInfo[] ps = source.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var item in ps)
        {
            var o = item.GetValue(source);

            object newObject = Convert(o, path);

            if (newObject == null && o != source && o != null && !o.GetType().IsValueType)
            {
                RemapObject(o, path);
            }

            if (newObject != null)
            {
                item.SetValue(source, newObject);
            }
        }
    }

    static void CreateDirByAssetPath(string assetName)
    {
        string dataPath = Application.dataPath.ToLower().Replace("/assets", "/");
        string p = Path.GetDirectoryName(dataPath + assetName);
        System.IO.Directory.CreateDirectory(p);
        AssetDatabase.Refresh();
    }

    private void DownloadAsset(Object asset, string assetName)
    {
        CreateDirByAssetPath(assetName);

        if (asset is TextAsset)
        {
            Selection.activeObject = SaveTextAsset(asset as TextAsset, assetName);
        }
        else if (asset is ScriptableObject)
        {
            Selection.activeObject = SaveScriptableAsset(asset as ScriptableObject, assetName);
        }
        else
        {
            Debug.LogWarning("TODO " + asset.GetType().Name);
        }
    
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
