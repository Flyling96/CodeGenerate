using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


[Serializable]
public class BaseSingleSetting<T> : ScriptableObject where T : ScriptableObject
{
    static T sInstance = null;

    private static bool alreadySearched = false;

    public static T InstanceIfExists
    {
        get
        {
            if (!alreadySearched)
            {
                Type type = typeof(T);
                alreadySearched = true;
                var guids = AssetDatabase.FindAssets("t:" + type.Name);
                for (int i = 0; i < guids.Length && sInstance == null; ++i)
                    sInstance = AssetDatabase.LoadAssetAtPath<T>(
                        AssetDatabase.GUIDToAssetPath(guids[i]));
            }
            return sInstance;
        }
    }

    public static T Instance
    {
        get
        {
            if (InstanceIfExists == null)
            {
                Type type = typeof(T);
                string newAssetPath = EditorUtility.SaveFilePanelInProject(
                        string.Format("Create {0} asset", type.Name), type.Name, "asset",
                        "");
                if (!string.IsNullOrEmpty(newAssetPath))
                {
                    sInstance = CreateInstance<T>();
                    AssetDatabase.CreateAsset(sInstance, newAssetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            return sInstance;
        }
    }
}

