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
	public class GameObjectProperty : BaseProperty
	{
		// GET VALUE //
		//
		public GameObject GetValue()
		{
			GameObject returnValue = null;
#if UNITY_EDITOR
			CheckReferences();
#endif

			switch (dataMode)
			{
				case eDataMode.Parameter:
					if (wasParameterQueried == false)
					{
						parameter = ownerBlueprint.ownerEmitter.GetParameter(parameterName, AmpsHelpers.eParameterTypes.GameObject);
						wasParameterQueried = true;
					}
					if (parameter != null)
					{
						returnValue = parameter.GetGameObjectValue();
					}
					break;
			}

			return returnValue;
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		public void Initialize(string theName, GameObject go, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theName, theOwnerBlueprint);
			SetDataModes(false, false, false, false, false, true);
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);
		}

//============================================================================//
#region GUI

		// SHOW PROPERTY //
		//
		override public void ShowProperty(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			GUILayout.BeginVertical(GetStyle(selectedProperty));

			base.ShowProperty(ref selectedProperty, isReadOnly);

			switch (dataMode)
			{
				case eDataMode.Parameter:
					GUILayout.BeginVertical();
					ParameterHeader();
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