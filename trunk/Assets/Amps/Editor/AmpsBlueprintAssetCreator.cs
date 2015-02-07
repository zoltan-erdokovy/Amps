using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using Amps;
using System.Collections.Generic;

public class AmpsBlueprintAssetCreator
{
#if UNITY_EDITOR
	[MenuItem("Assets/Create/AmpsBlueprint")]
	public static void CreateAsset()
	{
		// Solution by yoyo //
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);
			if (File.Exists(path)) path = Path.GetDirectoryName(path);
			break;
		}
		/////////////////////

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/NewAmpsBlueprint.asset");
		Amps.AmpsBlueprint asset = ScriptableObject.CreateInstance<Amps.AmpsBlueprint>();
		asset.Initialize();

		AssetDatabase.CreateAsset(asset, assetPathAndName);
		asset.name = "Blueprint";
		List<Object> subObjects = asset.GetSubObjects();
		foreach (var item in subObjects)
		{
			AssetDatabase.AddObjectToAsset(item, assetPathAndName);
		}
		AssetDatabase.SaveAssets();
	}
#endif
}