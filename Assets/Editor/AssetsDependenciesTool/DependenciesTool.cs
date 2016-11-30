using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class DependenciesTool
{
    [MenuItem("Assets/Select Dependencies Pro/All")]
    private static void SelectDependenciesAll()
    {
        _GetDependencies<UnityEngine.Object>();
    }

	#if UNITY_5
	[MenuItem("Assets/Select Dependencies Pro/All Directly")]
	private static void SelectDirectlyDependenciesAll()
	{
		_GetDependencies<UnityEngine.Object> (true);
	}
	#endif

    [MenuItem("Assets/Select Dependencies Pro/AnimationClip")]
    private static void SelectAnimationClipDependencies()
    {
        _GetDependencies<UnityEngine.AnimationClip>();
    }

    [MenuItem("Assets/Select Dependencies Pro/AnimationContoller")]
    private static void SelectAnimationControllerDependencies()
    {
        _GetDependencies(".contorller");
    }

    [MenuItem("Assets/Select Dependencies Pro/AudioClip")]
    private static void SelectAudioDependencies()
    {
        _GetDependencies<UnityEngine.AudioClip>();
    }

    [MenuItem("Assets/Select Dependencies Pro/Font")]
    private static void SelectFontDependencies()
    {
        _GetDependencies<UnityEngine.Font>();
    }

    [MenuItem("Assets/Select Dependencies Pro/Material")]
    private static void SelectMaterialDependencies()
    {
        _GetDependencies<UnityEngine.Material>();
    }

    [MenuItem("Assets/Select Dependencies Pro/Mesh")]
    private static void SelectMeshDependencies()
    {
        _GetDependencies<UnityEngine.Mesh>();
    }


    [MenuItem("Assets/Select Dependencies Pro/Model")]
    private static void SelectModelDependencies()
    {
        _GetDependencies(".fbx");
    }

    [MenuItem("Assets/Select Dependencies Pro/PhysicMaterial")]
    private static void SelectPhysicMaterialDependencies()
    {
        _GetDependencies<UnityEngine.PhysicMaterial>();
    }

    [MenuItem("Assets/Select Dependencies Pro/Prefab")]
    private static void SelectPrefabDependencies()
    {
        _GetDependencies(".prefab");
    }

    [MenuItem("Assets/Select Dependencies Pro/Script")]
    private static void SelectScriptDependencies()
    {
        _GetDependencies(".cs");
    }

    [MenuItem("Assets/Select Dependencies Pro/Shader")]
    private static void SelectShaderDependencies()
    {
        _GetDependencies<UnityEngine.Shader>();
    }

    [MenuItem("Assets/Select Dependencies Pro/Texture")]
    private static void SelectTextureDependencies()
    {
        _GetDependencies<UnityEngine.Texture>();
    }

    [MenuItem("Assets/Select Dependencies Pro/Sprite")]
    private static void SelectSpriteDependencies()
    {
        _GetDependencies<UnityEngine.Sprite>();
    }

	private static void _GetDependencies<T>(bool directlyDepend = false) where T : UnityEngine.Object
    {
        string[] selections = Selection.assetGUIDs;
        List<string> lstPathNames = new List<string>();
        List<string> assetPathDependenciesBy = new List<string>();
        for (int i = 0; selections != null && i < selections.Length; ++i)
        {
            lstPathNames.Add(AssetDatabase.GUIDToAssetPath(selections[i]));
        }
		#if UNITY_5
		assetPathDependenciesBy.AddRange(AssetDatabase.GetDependencies(lstPathNames.ToArray(), !directlyDepend));
		#else
		assetPathDependenciesBy.AddRange(AssetDatabase.GetDependencies(lstPathNames.ToArray());
		#endif
        ShowSelectedObjectsTool.ShowSelectedObjectsInProjectBrowser<T>(assetPathDependenciesBy);
    }

    private static void _GetDependencies(string type)
    {
        string[] selections = Selection.assetGUIDs;
        List<string> lstPathNames = new List<string>();
        List<string> assetPathDependenciesBy = new List<string>();
        for (int i = 0; selections != null && i < selections.Length; ++i)
        {
            lstPathNames.Add(AssetDatabase.GUIDToAssetPath(selections[i]));
        }
        assetPathDependenciesBy.AddRange(AssetDatabase.GetDependencies(lstPathNames.ToArray()));
        for (int i = assetPathDependenciesBy.Count - 1; i >= 0; --i)
        {
            if(!assetPathDependenciesBy[i].ToLower().EndsWith(type))
            {
                assetPathDependenciesBy.RemoveAt(i);
            }
        }
        ShowSelectedObjectsTool.ShowSelectedObjectsInProjectBrowser<UnityEngine.Object>(assetPathDependenciesBy);
    }
}
