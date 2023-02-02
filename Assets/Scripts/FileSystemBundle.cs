using UnityEngine;

public class FileSystemBundle : ResourceManager.Bundle
{
    readonly string dir;
    bool lockBase = false;

    public FileSystemBundle(string path)
    {
        dir = path;
    }

    const string PATH_MASK = "assets/resources/";

    override public UnityEngine.Object LoadAsset(string name, System.Type type)
    {
        UnityEngine.Object obj = null;
        name = name.Replace(PATH_MASK, "");
        string path = dir + "/" + name;

        if (System.IO.File.Exists(path))
        {
            string ext = System.IO.Path.GetExtension(name);
            switch (ext)
            {
                case ".png":
                    obj = LoadSprite(path);
                    break;
                case ".asset":
                    lockBase = true;
                    obj = ResourceManager.Load(System.IO.Path.ChangeExtension(name, null), type, ext);
                    lockBase = false;
                    //Debug.Log(JsonUtility.ToJson(obj)); //dump proprties
                    JsonUtility.FromJsonOverwrite(LoadText(path), obj);
                    break;

            }
        }
        if (!obj)
        {
            return Resources.Load(System.IO.Path.ChangeExtension(name, null), type);
        }
        else
        {
            return obj;
        }
    }

    public override AsyncOperation LoadAssetAsync(string name, System.Type type)
    {
        // @todo add support override
        return Resources.LoadAsync(System.IO.Path.ChangeExtension(name, null), type);
    }

    override public void Unload(bool unloadAllLoadedObjects)
    {
        //skip
    }

    override public bool Contains(string name)
    {
        if (lockBase)
        {
            return false;
        }
        else
        {
            return System.IO.File.Exists(dir + "/" + name.Replace(PATH_MASK, ""));
        }
    }

    override public string[] GetAllAssetNames()
    {
        return System.IO.Directory.GetFiles(dir, "*.*", System.IO.SearchOption.AllDirectories);
    }

    public string LoadText(string path)
    {
        return System.IO.File.ReadAllText(path);
    }

    public Sprite LoadSprite(string path)
    {
        Texture2D tex2D = new Texture2D(2, 2);
        if (tex2D.LoadImage(System.IO.File.ReadAllBytes(path)))
        {
            return Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0, 0));
        }

        return null;
    }
}

