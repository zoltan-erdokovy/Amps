// Copyright 2015 Zoltan Erdokovy

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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