using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class StringProperty : BaseProperty
	{
		public string value;

		// GET VALUE //
		//
		public string GetValue()
		{
			return value;
		}

//============================================================================//
#if UNITY_EDITOR

//============================================================================//
#region GUI

		// INITIALIZE //
		//
		public void Initialize(string theName, string s)
		{
			base.Initialize(theName, null);
			SetDataModes(true, false, false, false, false, false);
			value = s;
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);

			StringProperty originalStringProperty = originalProperty as StringProperty;
			value = originalStringProperty.value;
		}

		// SHOW PROPERTY //
		//
		override public void ShowProperty(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			GUILayout.BeginVertical(GetStyle(selectedProperty));

			base.ShowProperty(ref selectedProperty, isReadOnly);

			GUI.SetNextControlName(AmpsHelpers.stringControlName);
			if (isReadOnly) GUI.enabled = false;
			value = GUILayout.TextField(value);
			if (isReadOnly) GUI.enabled = true;

			GUILayout.EndVertical();

			HandleSelection(ref selectedProperty, this);
		}

#endregion

#endif
	}
}