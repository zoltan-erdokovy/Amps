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