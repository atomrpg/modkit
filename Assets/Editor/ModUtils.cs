using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ModUtils : EditorWindow
{
    [MenuItem("Game/Mod Utils/Selected object to JSON(Log)")]
    static void Init()
    {
        Debug.Log(JsonUtility.ToJson(Selection.activeObject));
    }
}
