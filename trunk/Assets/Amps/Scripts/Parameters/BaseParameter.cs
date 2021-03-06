﻿// Copyright 2015 Zoltan Erdokovy

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
using System;
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class BaseParameter
	{
		public string name;
		public AmpsHelpers.eParameterTypes type;
		public bool shouldBeRemoved;

		public bool boolParameter;
		public float scalarParameter;
		public Vector4 vectorParameter;
		public GameObject gameObjectParameter;
		public Mesh meshParameter;
		public Material materialParameter;

		public BaseParameter()
		{
			//this.BaseParameter(AmpsHelpers.eParameterTypes.Bool);
		}

		public BaseParameter(AmpsHelpers.eParameterTypes theType)
		{
			System.Random theRandom = new System.Random();
			name = "Parameter" + theRandom.Next(1, 99);
			type = theType;
			shouldBeRemoved = false;
			scalarParameter = 0;
		}

		// GET BOOL VALUE //
		//
		public bool GetBoolValue()
		{
			return boolParameter;
		}

		// GET SCALAR VALUE //
		//
		public float GetScalarValue()
		{
			return scalarParameter;
		}

		// GET VECTOR VALUE //
		//
		public Vector4 GetVectorValue()
		{
			return vectorParameter;
		}

		// GET GAMEOBJECT VALUE //
		//
		public GameObject GetGameObjectValue()
		{
			return gameObjectParameter;
		}

		// GET MESH VALUE //
		//
		public Mesh GetMeshValue()
		{
			return meshParameter;
		}

		// GET MATERIAL VALUE //
		//
		public Material GetMaterialValue()
		{
			return materialParameter;
		}

//============================================================================//
#if UNITY_EDITOR
//============================================================================//
#region GUI

		// SORTED POPUP //
		//
		public int SortedPopup(int selectedIndex, string[] displayedOptions, params GUILayoutOption[] options)
		{
			int sortedSelectedIndex;
			string[] sortedDisplayedOptions = displayedOptions.Clone() as string[];
			int[] unsortedIndices = new int[displayedOptions.Length];
			int[] sortedIndices;

			for (int i = 0; i < unsortedIndices.Length; i++)
			{
				unsortedIndices[i] = i;
			}
			sortedIndices = unsortedIndices.Clone() as int[];

			System.Array.Sort(sortedDisplayedOptions, sortedIndices);
			sortedSelectedIndex = System.Array.IndexOf(sortedIndices, selectedIndex);

			sortedSelectedIndex = EditorGUILayout.Popup(sortedSelectedIndex, sortedDisplayedOptions, "popup", options);

			return unsortedIndices[sortedIndices[sortedSelectedIndex]];
		}

		// SHOW PARAMETER //
		//
		public void ShowParameter()
		{
			GUILayout.BeginHorizontal("propertyHeader");

			shouldBeRemoved = GUILayout.Toggle(shouldBeRemoved, " x", "toggle");	// TODO: Proper icons.

			GUILayout.Space(32);
			GUILayout.Label("Name:");
			name = EditorGUILayout.TextField(name);
			type = (AmpsHelpers.eParameterTypes) SortedPopup((int)type, AmpsHelpers.parameterTypesDisplayData, GUILayout.MaxWidth(80));

			GUILayout.Space(32);
			GUILayout.Label("Value:", "floatFieldLabel");
			GUILayout.BeginHorizontal(GUILayout.MinWidth(128));
			switch (type)
			{
				case AmpsHelpers.eParameterTypes.Bool:
					ShowBoolParameter();
					break;
				case AmpsHelpers.eParameterTypes.Scalar:
					ShowScalarParameter();
					break;
				case AmpsHelpers.eParameterTypes.Vector:
					ShowVectorParameter();
					break;
				case AmpsHelpers.eParameterTypes.GameObject:
					ShowGameObjectParameter();
					break;
				case AmpsHelpers.eParameterTypes.Mesh:
					ShowMeshParameter();
					break;
				case AmpsHelpers.eParameterTypes.Material:
					ShowMaterialParameter();
					break;
				default:
					break;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		// SHOW BOOL PARAMETER //
		//
		public void ShowBoolParameter()
		{
			boolParameter = GUILayout.Toggle(boolParameter, "", "toggle");
		}

		// SHOW SCALAR PARAMETER //
		//
		public void ShowScalarParameter()
		{
			EditorGUIUtility.labelWidth = 40;
			EditorGUIUtility.fieldWidth = 40;

			scalarParameter = MyFloatField("", scalarParameter, GUILayout.ExpandWidth(false));
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
		}

		// SHOW VECTOR PARAMETER //
		//
		public void ShowVectorParameter()
		{
			EditorGUIUtility.labelWidth = 20;
			EditorGUIUtility.fieldWidth = 35;
			vectorParameter.x = MyFloatField("", vectorParameter.x, GUILayout.ExpandWidth(false));
			vectorParameter.y = MyFloatField("", vectorParameter.y, GUILayout.ExpandWidth(false));
			vectorParameter.z = MyFloatField("", vectorParameter.z, GUILayout.ExpandWidth(false));
			vectorParameter.w = MyFloatField("", vectorParameter.w, GUILayout.ExpandWidth(false));
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
		}

		// SHOW GAME OBJECT PARAMETER //
		//
		public void ShowGameObjectParameter()
		{
			EditorGUIUtility.labelWidth = 20;
			EditorGUIUtility.fieldWidth = 192;
			gameObjectParameter = (GameObject)EditorGUILayout.ObjectField(gameObjectParameter, typeof(GameObject), true);
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
		}

		// SHOW MESH PARAMETER //
		//
		public void ShowMeshParameter()
		{
			EditorGUIUtility.labelWidth = 20;
			EditorGUIUtility.fieldWidth = 192;
			meshParameter = (Mesh)EditorGUILayout.ObjectField(meshParameter, typeof(Mesh), false);
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
		}

		// SHOW MATERIAL PARAMETER //
		//
		public void ShowMaterialParameter()
		{
			EditorGUIUtility.labelWidth = 20;
			EditorGUIUtility.fieldWidth = 192;
			materialParameter = (Material)EditorGUILayout.ObjectField(materialParameter, typeof(Material), false);
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
		}

		// MY FLOAT FIELD //
		//
		public float MyFloatField(string label, float value, params GUILayoutOption[] options)
		{
			float returnValue;

			if (EditorGUIUtility.isProSkin)
			{
				GUILayout.Label(label, "floatFieldLabel");
				float.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(40)), out returnValue);
			}
			else
			{
				returnValue = EditorGUILayout.FloatField(label, value, options);
			}

			return returnValue;
		}
#endregion
#endif
	}
}