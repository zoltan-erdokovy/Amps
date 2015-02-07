using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class BoolProperty : BaseProperty
	{
		public bool value;

		// GET VALUE //
		//
		public bool GetValue()
		{
			bool returnValue = false;
#if UNITY_EDITOR
			CheckReferences();
#endif

			switch (dataMode)
			{
				case eDataMode.Constant:
					returnValue = value;
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						BoolProperty theReference = (BoolProperty)reference.property;
						returnValue = theReference.GetValue();
					}
					break;
				case eDataMode.Parameter:
					if (wasParameterQueried == false)
					{
						parameter = ownerBlueprint.ownerEmitter.GetParameter(parameterName, AmpsHelpers.eParameterTypes.Bool);
						wasParameterQueried = true;
					}
					if (parameter != null)
					{
						returnValue = parameter.GetBoolValue();
					}
					else returnValue = value;
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
			Initialize(theName, false, theOwnerBlueprint);
		}

		// INITIALIZE //
		//
		public void Initialize(string theName, bool b, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theName, theOwnerBlueprint);
			SetDataModes(true, false, false, false, true, true);
			value = b;
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);

			BoolProperty originalBoolProperty = originalProperty as BoolProperty;
			value = originalBoolProperty.value;
		}

//============================================================================//
#region GUI

		// SHOW PROPERTY //
		//
		override public void ShowProperty(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			bool shouldShowMiniHeader = isReadOnly == false				// No mini header if rendering a reference.
										&& !allowDataModeRandomConstant
										&& !allowDataModeCurve
										&& !allowDataModeRandomCurve
										&& !allowDataModeReference;

			GUILayout.BeginVertical(GetStyle(selectedProperty));

			if (shouldShowMiniHeader)
			{
				GUILayout.BeginHorizontal();
				ShowMiniHeader(ref selectedProperty, isReadOnly);
			}
			else ShowHeader(ref selectedProperty, isReadOnly);

			switch (dataMode)
			{
				case eDataMode.Constant:
					if (shouldShowMiniHeader) GUILayout.BeginHorizontal("propertyHeader");
					else GUILayout.BeginHorizontal();

					GUILayout.FlexibleSpace();
					value = GUILayout.Toggle(value, "", "toggle");

					GUILayout.EndHorizontal();
					break;

				case eDataMode.Reference:
					CheckReferences();
					ShowReferenceControl();
					break;
				case eDataMode.Parameter:
					GUILayout.BeginVertical();
					ParameterHeader();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Default");
					value = GUILayout.Toggle(value, "", "toggle");
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					break;
			}

			if (shouldShowMiniHeader)
			{
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();

			HandleSelection(ref selectedProperty, this);
		}
#endregion

#endif
	}
}