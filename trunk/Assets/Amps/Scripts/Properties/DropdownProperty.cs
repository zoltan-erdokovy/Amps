using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class DropdownProperty : BaseProperty
	{
		public int constant;

		#if UNITY_EDITOR
		public DropdownData displayData;	// The function which fetches the string array to display in the dropdown control.
											// Since delegates are not serialized it will get it's value right before first
											// UI render.
		#endif

		// GET VALUE //
		//
		public int GetValue()
		{
			int returnValue = 0;
#if UNITY_EDITOR
			CheckReferences();
#endif

			switch (dataMode)
			{
				case eDataMode.Constant:
					returnValue = constant;
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						DropdownProperty theReference = (DropdownProperty)reference.property;
						returnValue = theReference.GetValue();
					}
					break;
			}

			return returnValue;
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(string theName, AmpsBlueprint theOwnerBlueprint)
		{
			Initialize(theName, 0, theOwnerBlueprint);
		}

		// INITIALIZE //
		//
		public void Initialize(string theName, int i, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theName, theOwnerBlueprint);
			SetDataModes(true, false, false, false, true, false);
			constant = i;
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);

			DropdownProperty originalDropdownProperty = originalProperty as DropdownProperty;
			displayData = originalDropdownProperty.displayData;
			constant = originalDropdownProperty.constant;
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
				case eDataMode.Constant:
					GUILayout.BeginHorizontal();
					//GUILayout.Space(8);
					constant = SortedPopup(constant, displayData());
					GUILayout.EndHorizontal();
					break;

				case eDataMode.Reference:
					CheckReferences();
					ShowReferenceControl();
					break;
			}

			GUILayout.EndVertical();

			HandleSelection(ref selectedProperty, this);
		}
		#endregion

#endif
	}
}