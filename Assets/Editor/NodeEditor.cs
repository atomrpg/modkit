using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BehaviorEditor;

[NodeEditor(typeof(CharacterHeroNode))]
[CategoryAttribute("Character")]
public class CharacterHero : FindObject
{
    protected override void OnNodeGUI()
    {
        base.OnNodeGUI();
        GUILayout.Label("<i>" + GetState().msg + "</i>");
    }

    private CharacterHeroNode GetState()
    {
        return State as CharacterHeroNode;
    }

    protected override void OnNodeInspectorGUI()
    {
        base.OnNodeInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Data:");
        GetState().msg = EditorGUILayout.TextField(GetState().msg);
        EditorGUILayout.EndHorizontal();
    }
}