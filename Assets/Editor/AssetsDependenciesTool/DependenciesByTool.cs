using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DependenciesByTool : AssetPostprocessor
{
	private static ObjectRepo repo;
	private static bool initialized = false;

    static DependenciesByTool()
	{
		Initialize ();
	}

	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
	                                           string[] movedAssets, string[] movedFromAssetPaths)
	{
        bool needSerialize = false;
		Initialize ();
		for (int i=0; importedAssets != null && i < importedAssets.Length; ++i) 
		{
			if(repo.ImportAsset(importedAssets[i]))
            {
                needSerialize = true;
            }
		}
		for (int i=0; deletedAssets != null && i < deletedAssets.Length; ++i) 
		{
			repo.DeleteAsset(deletedAssets[i]);
            needSerialize = true;
		}
        if(needSerialize)
        {
            repo.Finialize();
        }
	}
		
	private static void Initialize()
	{
		if (false == initialized)
		{
			repo = new ObjectRepo ();
			repo.Initialize ();
			initialized = true;
		}
	}
	/// <summary>
	/// Select Objects which depend by selected Objects
	/// </summary>
	[MenuItem("Assets/Select Dependencies By/All")]
	private static void SelectDependenciesByAll()
	{
        _GetDependenciesBy<UnityEngine.Object>();
	}

	#if UNITY_5
	[MenuItem("Assets/Select Dependencies By/All Directly")]
	private static void SelectDirectlyDependenciesByAll()
	{
		_GetDependenciesBy<UnityEngine.Object> (true);
	}
	#endif

    [MenuItem("Assets/Select Dependencies By/AnimationController")]
    private static void SelectDependenciesByAnimationController()
    {
        _GetDependenciesBy(".controller");
    }

    [MenuItem("Assets/Select Dependencies By/Material")]
    private static void SelectDependenciesByMaterial()
    {
        _GetDependenciesBy<UnityEngine.Material>();
    }

    [MenuItem("Assets/Select Dependencies By/Mesh")]
    private static void SelectDependenciesByMesh()
    {
        _GetDependenciesBy<UnityEngine.Mesh>();
    }

    [MenuItem("Assets/Select Dependencies By/Model")]
    private static void SelectDependenciesByModel()
    {
        _GetDependenciesBy(".fbx");
    }

    [MenuItem("Assets/Select Dependencies By/Prefab")]
    private static void SelectDependenciesByPrefab()
    {
        _GetDependenciesBy(".prefab");
    }

    [MenuItem("Assets/Select Dependencies By/Scene")]
    private static void SelectDependenciesByScene()
    {
        _GetDependenciesBy(".unity");
    }

    [MenuItem("Assets/Select Dependencies By/PhysicMaterial")]
    private static void SelectDependenciesByPhysicMaterial()
    {
        _GetDependenciesBy<UnityEngine.PhysicMaterial>();
    }

    [MenuItem("Assets/DependencyTool/Initialize")]
    private static void ShowDependencyInHierarchy()
    {
        repo = new ObjectRepo ();
        repo.Initialize (true);
        initialized = true;
    }

    /// <summary>
    /// Gets the dependencies by assetPathName
    /// </summary>
    /// <returns>The dependencies by.</returns>
    /// <param name="assetPathName">Asset path name.</param>
    /// <param name="directlyDepend">If set to <c>true</c> directly depend.</param>
    public static List<string> GetDependenciesBy(string assetPathName, bool directlyDepend)
    {
        List<string> assetPathDependenciesBy = new List<string>();
        string guid = AssetDatabase.AssetPathToGUID(assetPathName);

        List<string> assetGuidDependenciesBy  = repo.GetDepencenciesBy(guid);
        for (int i = 0; i < assetGuidDependenciesBy.Count; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(assetGuidDependenciesBy[i]);
            assetPathDependenciesBy.Add(path);
        }

        for (int i = assetPathDependenciesBy.Count-1; i >=0 ; --i)
        {
            List<string> dependencies = new List<string> ();
            #if UNITY_5
            dependencies.AddRange (AssetDatabase.GetDependencies (new string[]{ assetPathDependenciesBy [i] }, !directlyDepend));
            #else
            dependencies.AddRange (AssetDatabase.GetDependencies (new string[]{ assetPathDependenciesBy [i] });
            #endif
            string path = AssetDatabase.GUIDToAssetPath (guid);
            if (!dependencies.Contains (path))
            {
                assetPathDependenciesBy.RemoveAt (i);
            }
        }

        return assetPathDependenciesBy;
    }

    /// <summary>
    /// Gets the dependencies by assetPathName
    /// </summary>
    /// <returns>The dependencies by.</returns>
    /// <param name="assetPathName">Asset path name.</param>
    /// <param name="typeSuffix">Type suffix.</param>
    /// <param name="directlyDepend">If set to <c>true</c> directly depend.</param>
    public static List<string> GetDependenciesBy(string assetPathName, string typeSuffix, bool directlyDepend)
    {
        List<string> assetPathDependenciesBy = GetDependenciesBy(assetPathName, directlyDepend);
        for (int i = assetPathDependenciesBy.Count - 1; i >= 0; --i)
        {
            if (!assetPathDependenciesBy[i].ToLower().EndsWith(typeSuffix))
            {
                assetPathDependenciesBy.RemoveAt(i);
            }
        }
        return assetPathDependenciesBy;
    }

	private static void _GetDependenciesBy<T>(bool directlyDepend = false) where T : UnityEngine.Object
    {
        string[] selections = Selection.assetGUIDs;
        List<string> assetPathDependenciesBy = new List<string>();
        for (int i = 0; selections != null && i < selections.Length; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(selections[i]);
            assetPathDependenciesBy.AddRange(GetDependenciesBy(path, directlyDepend));
        }
        ShowSelectedObjectsTool.ShowSelectedObjectsInProjectBrowser<T>(assetPathDependenciesBy);
    }

    private static void _GetDependenciesBy(string type, bool directlyDepend = false)
    {
        string[] selections = Selection.assetGUIDs;
        List<string> assetPathDependenciesBy = new List<string>();
        for (int i = 0; selections != null && i < selections.Length; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(selections[i]);
            assetPathDependenciesBy.AddRange(GetDependenciesBy(path, type, directlyDepend));
        }
        ShowSelectedObjectsTool.ShowSelectedObjectsInProjectBrowser<UnityEngine.Object>(assetPathDependenciesBy);
    }
}

public class ObjectRepo
{
	private Dictionary<string, List<string>> dicDepBy;
	private Dictionary<string, List<string>> dicDep;
	private static string configFilePath = Application.dataPath + "/../Library/DependenciesBy.config";
    private static string tmpConfigFilePath = Application.dataPath + "/../Library/DependenciesBy_tmp.config";
    private string contentStr;
    private List<string> lstChanged = new List<string>();
    private List<string> lstMoved = new List<string>();
    private List<string> lstAdded = new List<string>();
    private static string usrOptionKey = "USER_OPTION";

    public void Initialize(bool forceReimport = false)
	{
        if (File.Exists (configFilePath) && !forceReimport)
		{
			Deserialize ();
		}
		else 
		{
            bool option = false;
            if (forceReimport)
            {
                option = true;
            }
            else if (PlayerPrefs.HasKey(usrOptionKey))
            {
                option = false;
            }
            else
            {
                option = EditorUtility.DisplayDialog ("Info", "Dependency tool need to walk through your working repository. " +
                    "This may take a few minutes(depending on the assets num). You can ignore this and initialize it manual from Menu(Assets/DependencyTool/Initialize). " +
                    "Generate Now?", "OK", "Cancel");
            }
            if (option)
            {
                PlayerPrefs.DeleteKey(usrOptionKey);
                PlayerPrefs.Save();
                ParseFromRepo();
            }
            else
            {
                PlayerPrefs.SetInt(usrOptionKey, 1);
                PlayerPrefs.Save();
                dicDepBy = new Dictionary<string, List<string>>();
                dicDep = new Dictionary<string, List<string>>();
            }
		}
	}

	public void Finialize()
	{
		Serialize ();
	}

	public List<string> GetDepencenciesBy(string guid)
	{
		if (dicDepBy.ContainsKey (guid)) {
			return dicDepBy [guid];
		} else {
			return null;
		}
	}

    public void ParseFromRepo()
    {
		string[] assets = AssetDatabase.GetAllAssetPaths();
		dicDepBy = new Dictionary<string, List<string>>(assets.Length);
		dicDep = new Dictionary<string, List<string>>(assets.Length);

        for (int i = 0; assets != null && i < assets.Length; ++i)
        {
            EditorUtility.DisplayProgressBar("Dependencies Tool Initializing",
                "this may take a few minutes, progress:" + i + "/" + assets.Length,
                (float)i / assets.Length);
            ImportAsset(assets[i]);
        }
        if (assets != null && assets.Length > 0)
        {
            EditorUtility.ClearProgressBar();
        }

        Serialize();
    }

	public bool ImportAsset(string assetPath)
	{
        if(!assetPath.StartsWith("Assets/") || 
            Directory.Exists(Application.dataPath + "/../" + assetPath))
            return false;

		string guid = AssetDatabase.AssetPathToGUID (assetPath);
		string[] dependencies = AssetDatabase.GetDependencies(new string[]{assetPath});

        if (dependencies == null || dependencies.Length == 0)
            return false;
        
		List<string> lstDep = null;
        bool assetModified = false;

		if (false == dicDep.TryGetValue(guid, out lstDep)) 
		{
			lstDep = new List<string>();
            dicDep[guid] = lstDep;
            if (!lstAdded.Contains(guid))
            {
                lstAdded.Add(guid);
            }
		}
		for (int i=0; dependencies != null && i < dependencies.Length; ++i)
		{
			//if(dependencies[i] == assetPath) continue;	// Ignore myself

			// add dicDepBy
			string depGuid = AssetDatabase.AssetPathToGUID(dependencies[i]);
			List<string> lstDepBy = null;
			dicDepBy.TryGetValue(depGuid, out lstDepBy);
			if(null == lstDepBy)
			{
				lstDepBy = new List<string>();
				dicDepBy[depGuid] = lstDepBy;
			}
			if(false == lstDepBy.Contains(guid))
			{
				lstDepBy.Add(guid);
                if(!lstChanged.Contains(depGuid))
                {
                    lstChanged.Add(depGuid);
                }
			}

			// add dicDep
			if(false == lstDep.Contains(depGuid))
			{
				lstDep.Add(depGuid);
                assetModified = true;
			}

            if (lstDep.Count < 1)
            {
                Debug.LogError(string.Format("Parse Asset {0} Dependency error", assetPath));
                if (dicDep.ContainsKey(guid))
                {
                    dicDep.Remove(guid);
                }
            }
		}

        if (assetModified && !lstChanged.Contains(guid))
        {
            lstChanged.Add(guid);
        }

        return assetModified;
	}

	public void DeleteAsset(string assetPath)
	{
		string guid = AssetDatabase.AssetPathToGUID (assetPath);
		List<string> lstDep = null;
		List<string> lstDepBy = null;

		dicDep.TryGetValue (guid, out lstDep);
		dicDepBy.TryGetValue (guid, out lstDepBy);
		if (!lstMoved.Contains(guid))
		{
			lstMoved.Add(guid);
		}
		for (int i=0; lstDep != null && i<lstDep.Count; ++i)
		{
			if(dicDep.ContainsKey(lstDep[i]))
				dicDep.Remove(lstDep[i]);
			lstDep[i] = AssetDatabase.GUIDToAssetPath(lstDep[i]);
		}
		for (int i=0; lstDepBy != null && i<lstDepBy.Count; ++i)
		{
			if(dicDepBy.ContainsKey(lstDepBy[i]))
				dicDepBy.Remove(lstDepBy[i]);
			lstDepBy[i] = AssetDatabase.GUIDToAssetPath(lstDepBy[i]);
		}
		if(lstDep != null ) lstDep.Remove (assetPath);
        if (lstDepBy != null) lstDepBy.Remove(assetPath);
		for (int i=0; lstDep != null && i<lstDep.Count; ++i)
		{
			ImportAsset(lstDep[i]);
		}
		for (int i=0; lstDepBy != null && i<lstDepBy.Count; ++i)
		{
			ImportAsset(lstDepBy[i]);
		}
	}

	private void Deserialize()
	{
		dicDepBy = new Dictionary<string, List<string>>();
		dicDep = new Dictionary<string, List<string>> ();
		string[] allLines = File.ReadAllLines (configFilePath);
		for (int index = 0; allLines != null && index < allLines.Length; ++index) 
        {
			string[] objectAndDep = allLines[index].Split(new char[]{':'}, System.StringSplitOptions.RemoveEmptyEntries);
            if(objectAndDep.Length == 2)
            {
                List<string> lstDepBy = new List<string>();
                string[] dependencies = objectAndDep[1].Split(new char[]{';'}, System.StringSplitOptions.RemoveEmptyEntries);
                if(dependencies != null)
                {
                    lstDepBy.AddRange(dependencies);
                }
                dicDepBy[objectAndDep[0]] = lstDepBy;

                for(int i=0; dependencies != null && i<dependencies.Length; ++i)
                {
                    List<string> lstDep = null;
                    dicDep.TryGetValue(dependencies[i], out lstDep);
                    if(null == lstDep)
                    {
                        lstDep = new List<string>();
                        dicDep.Add(dependencies[i], lstDep);
                    }
                    lstDep.Add(objectAndDep[0]);
                }
            }
            else if(objectAndDep.Length > 0)
            {
                File.Delete (configFilePath);
                bool option = false;
                if (PlayerPrefs.HasKey(usrOptionKey))
                {
                    option = false;
                }
                else
                {
                    option = EditorUtility.DisplayDialog ("Warning", "Dependency tool find the config file is broken. " +
                        "Suggest you regenerate the config file." +
                        "This may take a few minutes(depending on the assets num). You can ignore this and initialize it manual from Menu(Assets/DependencyTool/Initialize). " +
                        "Generate Now?", "Generate", "Next time");
                }
                if (option)
                {
                    PlayerPrefs.DeleteKey(usrOptionKey);
                    PlayerPrefs.Save();
                    ParseFromRepo();
                }
                else
                {
                    PlayerPrefs.SetInt(usrOptionKey, 1);
                    PlayerPrefs.Save();
                    dicDepBy = new Dictionary<string, List<string>>();
                    dicDep = new Dictionary<string, List<string>>();
                }

                Debug.LogError(string.Format("Config file is broken at line {0}, suggests rebuild it when needed or free", index+1));
            }
		}
	}

	private void Serialize()
	{
        if(string.IsNullOrEmpty(contentStr))
        {
            StringBuilder content = new StringBuilder();

            if (null == dicDepBy)
            {
                Debug.LogError("Need to Initialze First");
                return;
            }
            int num = 0;
            foreach (KeyValuePair<string, List<string>> keyValue in dicDepBy)
            {
                num++;
                List<string> lstDepBy = keyValue.Value;
                content.Append(keyValue.Key);
                content.Append(":");
                //EditorUtility.DisplayProgressBar("Dependencies Tool Initializing", num + "/" + dicDepBy.Count, (float)num / dicDepBy.Count);
                StringBuilder builder = new StringBuilder();
                for (int index = 0; lstDepBy != null && index < lstDepBy.Count; ++index)
                {
                    builder.Append(lstDepBy[index]);
                    builder.Append(";");
                }
                content.Append(builder);
                content.Append("\r\n");
            }
            //EditorUtility.ClearProgressBar();

            contentStr = content.ToString();
        }
        else
        {
            // move the deleted assets
            for(int i=0; i<lstMoved.Count; ++i)
            {
                StringBuilder patternBuilder = new StringBuilder(lstMoved[i]);
				patternBuilder.Append(@":.*((\r\n)+|\s+)");
				contentStr = Regex.Replace(contentStr, patternBuilder.ToString(), string.Empty);
            }
            // add the added assets
            for (int i = 0; i < lstAdded.Count; ++i)
            {
                StringBuilder addedBuilder = new StringBuilder(lstAdded[i]);
                List<string> lstDepBy = null;
				addedBuilder.Append(":");
                if (dicDepBy.TryGetValue(lstAdded[i], out lstDepBy))
                {
                    for (int index = 0; lstDepBy != null && index < lstDepBy.Count; ++index)
                    {
                        addedBuilder.Append(lstDepBy[index]);
                        addedBuilder.Append(";");
                    }
                }
                addedBuilder.Append("\r\n");
                contentStr += addedBuilder.ToString();
            }
            // modify the changes assets
            for(int i=0; i<lstChanged.Count;++i)
            {
                StringBuilder patternBuilder = new StringBuilder(lstChanged[i]);
				patternBuilder.Append(@":.*((\r\n)+|\s+)");
				StringBuilder replacementBuilder = new StringBuilder(lstChanged[i]);
                List<string> lstDepBy = null;
				replacementBuilder.Append(":");
                if(dicDepBy.TryGetValue(lstChanged[i], out lstDepBy))
                {
                    for (int index = 0; lstDepBy != null && index < lstDepBy.Count; ++index)
                    {
                        replacementBuilder.Append(lstDepBy[index]);
                        replacementBuilder.Append(";");
                    }
                }
                replacementBuilder.Append("\r\n");
				contentStr = Regex.Replace(contentStr, patternBuilder.ToString(), replacementBuilder.ToString());
            }
        }
        lstAdded.Clear();
        lstMoved.Clear();
        lstChanged.Clear();

        using (FileStream fileStream = new FileStream(tmpConfigFilePath, 
            File.Exists(tmpConfigFilePath) ? FileMode.Truncate : FileMode.OpenOrCreate))
		{
            byte[] bytes = System.Text.Encoding.Default.GetBytes(contentStr.ToString());
			fileStream.Write(bytes, 0, bytes.Length);
			fileStream.Close();
			fileStream.Dispose();

            if (File.Exists(configFilePath))
            {
                File.Delete(configFilePath);
            }
            File.Move(tmpConfigFilePath, configFilePath);
		}
	}
}