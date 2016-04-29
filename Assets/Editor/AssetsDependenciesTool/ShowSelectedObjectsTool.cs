using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class ShowSelectedObjectsTool 
{
    public static void ShowSelectedObjectsInProjectBrowser<T>(List<string> objectPaths) where T : UnityEngine.Object
    {
        List<UnityEngine.Object> dependenciesByPaths = new List<UnityEngine.Object>();
        if (objectPaths != null)
        {
            for (int i = 0; i < objectPaths.Count; ++i)
            {
                EditorUtility.DisplayProgressBar("Loading Dependencies", "loading...", (float)i / objectPaths.Count);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(objectPaths[i], typeof(T));
                if(obj != null)
                {
                    dependenciesByPaths.Add(obj);
                }
            }
            EditorUtility.ClearProgressBar();
            Selection.objects = dependenciesByPaths.ToArray();
            ShowSelectionInProjectHierarchy();
        }
    }

    private static void ShowSelectionInProjectHierarchy()
    {
        Type pbType = GetType("UnityEditor.ProjectBrowser");
        MethodInfo meth = pbType.GetMethod("ShowSelectedObjectsInLastInteractedProjectBrowser",
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static);
        meth.Invoke(null, null);
    }

    private static Type GetType(string name)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; assemblies != null && i < assemblies.Length; ++i)
        {
            Type type = assemblies[i].GetType(name);
            if (type != null)
            {
                return type;
            }
        }
        return null;
    }
}
