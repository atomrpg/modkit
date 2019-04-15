using UnityEngine;

public class FileSystemBundle : ResourceManager.Bundle
{
    readonly string dir;

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
            switch (System.IO.Path.GetExtension(name))
            {
                case ".png":
                    obj = LoadSprite(path);
                    break;
                case ".asset":
                    obj = Resources.Load(System.IO.Path.ChangeExtension(name, null), type);
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

    override public void Unload(bool unloadAllLoadedObjects)
    {
        //skip
    }

    override public bool Contains(string name)
    {
        return System.IO.File.Exists(dir + "/" + name.Replace(PATH_MASK, ""));
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

