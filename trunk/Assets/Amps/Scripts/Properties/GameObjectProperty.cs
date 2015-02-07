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