using System.Collections.Generic;
using UnityEngine;
using Harmony;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


[EnumPatch(typeof(CharacterStats.Perk), typeof(ModCharacterPerk))]
enum ModCharacterPerk
{
    SUPER_PERK1 = 150,
    SUPER_PERK2 = 151,
    SUPER_PERK3 = 152,
    REFILL_AMMO = 153,
}

[AttributeUsage(AttributeTargets.Enum, AllowMultiple = true)]
public class EnumPatch : Attribute
{
    public EnumPatch(Type baseType, Type declaringType)
    {
        if (!data.ContainsKey(baseType))
        {
            string[] names = Enum.GetNames(declaringType);
            Array values = Enum.GetValues(declaringType);

            var enumToString = new Dictionary<int, string>();
            var enumToValue = new Dictionary<string, Enum>();

            for (int i = 0, end = names.Length; i != end; ++i)
            {
                enumToString.Add((int)values.GetValue(i), names[i]);
                enumToValue.Add(names[i], (Enum)values.GetValue(i));
            }

            data.Add(baseType, new KeyValuePair<Dictionary<int, string>, Dictionary<string, Enum>>(enumToString, enumToValue));
        }
    }

    private static readonly Dictionary<Type, KeyValuePair<Dictionary<int, string>, Dictionary<string, Enum>>> data = new Dictionary<Type, KeyValuePair<Dictionary<int, string>, Dictionary<string, Enum>>>();


    [HarmonyPatch(typeof(Enum), "ToString", new Type[] { })]
    internal static class Patch_Enum_ToString
    {
        private static bool Prefix(ref Enum __instance, ref string __result)
        {
            if (data.TryGetValue(__instance.GetType(), out var v))
            {
                return !v.Key.TryGetValue(Convert.ToInt32(__instance), out __result);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Enum), "Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) })]
    internal static class Patch_Enum_Parse
    {
        private static bool Prefix(Type enumType, string value, ref Enum __result)
        {
            if (data.TryGetValue(enumType, out var v))
            {
                return !v.Value.TryGetValue(value, out __result);
            }
            return true;
        }
    }

    internal static GUIContent[] TempContent(string[] texts)
    {
        GUIContent[] retval = new GUIContent[texts.Length];
        for (int i = 0; i < texts.Length; i++)
            retval[i] = new GUIContent(texts[i]);
        return retval;
    }

#if UNITY_EDITOR
    [HarmonyPatch(typeof(EditorGUI), "EnumPopupInternal", new Type[] { typeof(Rect), typeof(GUIContent), typeof(Enum), typeof(Type), typeof(Func<Enum, bool>), typeof(bool), typeof(GUIStyle) })]
    internal static class Patch_Enum_Popup_Internal
    {

        private static bool Prefix(Rect position, GUIContent label, Enum selected, Type enumType, Func<Enum, bool> checkEnabled, bool includeObsolete, GUIStyle style, ref object __result)
        {
            if (data.ContainsKey(enumType))
            {
                __result = EnumPopupInternal(position, label, selected, enumType, checkEnabled, includeObsolete, style);
                return false;
            }
            return true;
        }

        private static Enum EnumPopupInternal(Rect position, GUIContent label, Enum selected, Type enumType, Func<Enum, bool> checkEnabled, bool includeObsolete, GUIStyle style)
        {
            System.Type type = enumType;
            if (!type.IsEnum)
                throw new Exception("parameter _enum must be of type System.Enum");
            int[] array = Enum.GetValues(type).Cast<int>().ToArray();
            string[] names = Enum.GetNames(type);
            int selectedIndex = Array.IndexOf(array, Convert.ToInt32(selected));
            int index = EditorGUI.Popup(position, label, selectedIndex, TempContent(names), style);
            if (index < 0 || index >= names.Length)
                return selected;
            return (Enum)Enum.ToObject(enumType, array[index]);
        }
    }

    [HarmonyPatch(typeof(SpecArrayDrawer), "OnGUI", new Type[] { typeof(Rect), typeof(SerializedProperty), typeof(GUIContent) })]
    internal static class Patch_SpecArrayDrawer_OnGUI
    {
        private static bool Prefix(Rect position, SerializedProperty property, GUIContent label)
        {
            var info = property.serializedObject.FindProperty(property.propertyPath + ".perk");
            if (info != null)
            {
                if (info.enumValueIndex >= 0 && info.enumNames.Length > info.enumValueIndex)
                {
                    label = new GUIContent(info.enumNames[info.enumValueIndex]);
                }
            }

            EditorGUI.PropertyField(position, property, label, property.isExpanded);
            return false;
        }
    }

    [HarmonyPatch(typeof(EditorGUI), "EnumPopupInternal", new Type[] { typeof(Rect), typeof(GUIContent), typeof(int), typeof(Type), typeof(Func<Enum, bool>), typeof(bool), typeof(GUIStyle) })]
    internal static class Patch_Enum_Popup_InternalInt
    {
        private static bool Prefix(Rect position, GUIContent label, int flagValue, Type enumType, Func<Enum, bool> checkEnabled, bool includeObsolete, GUIStyle style, ref int __result)
        {
            if (data.ContainsKey(enumType))
            {
                __result = EnumPopupInternal(position, label, flagValue, enumType, checkEnabled, includeObsolete, style);
                return false;
            }
            return true;
        }

        private static int EnumPopupInternal(Rect position, GUIContent label, int selected, Type enumType, Func<Enum, bool> checkEnabled, bool includeObsolete, GUIStyle style)
        {
            System.Type type = enumType;
            if (!type.IsEnum)
                throw new Exception("parameter _enum must be of type System.Enum");
            int[] array = Enum.GetValues(type).Cast<int>().ToArray();
            string[] names = Enum.GetNames(type);
            int selectedIndex = Array.IndexOf(array, selected);
            int index = EditorGUI.Popup(position, label, selectedIndex, TempContent(names), style);
            if (index < 0 || index >= names.Length)
                return selected;

            return array[index];
        }
    }
#endif

    [HarmonyPatch(typeof(Enum), "GetNames", new Type[] { typeof(Type) })]
    private static class Patch_Enum_GetNames
    {
        private static void Postfix(Type enumType, ref string[] __result)
        {
            if (data.TryGetValue(enumType, out var v))
            {
                var res = new List<string>();
                res.AddRange(__result);
                res.AddRange(v.Key.Values);
                __result = res.ToArray();
            }
        }
    }


    [HarmonyPatch(typeof(Enum), "GetValues", new Type[] { typeof(Type) })]
    private static class Patch_Enum_GetValues
    {
        private static void Postfix(Type enumType, ref Array __result)
        {
            if (data.TryGetValue(enumType, out var v))
            {
                var res = new List<int>();
                res.AddRange(__result.Cast<int>().ToArray());
                res.AddRange(v.Key.Keys.Cast<int>().ToArray());
                __result = res.ToArray();
            }
        }
    }
}