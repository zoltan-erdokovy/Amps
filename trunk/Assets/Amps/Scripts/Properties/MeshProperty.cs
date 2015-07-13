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
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class MeshProperty : BaseProperty
	{
		public Mesh value;

		// GET VALUE //
		//
		public Mesh GetValue()
		{
			Mesh returnValue = null;

			switch (dataMode)
			{
				case eDataMode.Constant:
					returnValue = value;
					break;

				case eDataMode.Reference:
					if (reference != null)
					{
						MeshProperty theReference = (MeshProperty)reference.property;
						if (theReference != null) returnValue = theReference.GetValue();
					}
					break;

				case eDataMode.Parameter:
					if (wasParameterQueried == false)
					{
						parameter = ownerBlueprint.ownerEmitter.GetParameter(parameterName, AmpsHelpers.eParameterTypes.Mesh);
						wasParameterQueried = true;
					}
					if (parameter != null)
					{
						returnValue = parameter.GetMeshValue();
					}
					else returnValue = value;
					break;
			}

			return returnValue;
		}

//============================================================================//
#if UNITY_EDITOR

//============================================================================//
#region GUI

		// INITIALIZE //
		//
		public void Initialize(string theName)
		{
			base.Initialize(theName, null);
			SetDataModes(true, false, false, false, true, true);
			value = null;
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);

			MeshProperty originalMeshProperty = originalProperty as MeshProperty;
			value = originalMeshProperty.value;
		}

		// SHOW PROPERTY //
		//
		override public void ShowProperty(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			GUILayout.BeginVertical(GetStyle(selectedProperty));

			base.ShowProperty(ref selectedProperty, isReadOnly);

			switch (dataMode)
			{
				case eDataMode.Constant:
					EditorGUIUtility.LookLikeControls(50, 50);
					value = (Mesh)EditorGUILayout.ObjectField("Mesh", value, typeof(Mesh), true);
					EditorGUIUtility.LookLikeControls();
					break;

				case eDataMode.Reference:
					CheckReferences();
					ShowReferenceControl();
					break;

				case eDataMode.Parameter:
					GUILayout.BeginVertical();
					ParameterHeader();
					EditorGUIUtility.LookLikeControls(50, 50);
					value = (Mesh)EditorGUILayout.ObjectField("Mesh", value, typeof(Mesh), true);
					EditorGUIUtility.LookLikeControls();
					GUILayout.EndVertical();
					break;
			}

			GUILayout.EndVertical();

			HandleSelection(ref selectedProperty, this);
		}

#endregion
#endif
	}
}