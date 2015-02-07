using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	[System.Serializable]
	public class BaseProperty : ScriptableObject
	{
		public enum eDataMode
		{
			Constant,
			RandomConstant,
			Curve,
			RandomCurve,
			Reference,
			Parameter
		}

		public enum eCoordSystemConversionMode
		{
			NoConversion,
			AsPosition,
			AsRotation,
			AsVelocity,
			AsScale
		}

		public bool allowDataModeConstant = true;
		public bool allowDataModeRandomConstant = true;
		public bool allowDataModeCurve = true;
		public bool allowDataModeRandomCurve = true;
		public bool allowDataModeReference = true;
		public bool allowDataModeParameter = true;

		public eDataMode dataMode;
		public PropertyReference reference;		// A reference to a property in a shared module.

		public string parameterName;			// The name of the emitter parameter to search for.
		[System.NonSerialized]
		public BaseParameter parameter;			// The emitter parameter object to get data from (can stay null).
		[System.NonSerialized]
		public bool wasParameterQueried;		// Indicates if we altready tried to get the parameter object
												// from the owner emitter. It might not have a matching one in
												// which case 'parameter' stays null.

		#if UNITY_EDITOR
		#endif
		public AmpsBlueprint ownerBlueprint;

		public AmpsHelpers.eCoordSystems coordSystem;
		public eCoordSystemConversionMode coordSystemConversionMode;

		#if UNITY_EDITOR
		public Rect propertyRect;
		public bool shouldSelectThis = false;
		public bool shouldRepaintPicker = false;
		public bool shouldBeRemoved = false;	// Gets set to true when it's owner module gets deleted.
		#endif

		public int randomSeed = 0;				// A property specific random seed;

		// CONVERT COORDINATE SYSTEM //
		//
		// TODO: Move it to AmpsHelpers.
		public Vector4 ConvertCoordinateSystem(Vector4 v, int particleIndex)
		{
			Vector3 returnValue = v;

			switch (coordSystem)
			{
				case AmpsHelpers.eCoordSystems.World:
					//switch (coordSystemConversionMode)
					//{
					//    case eCoordSystemConversionMode.AsPosition:
					//        break;
					//    case eCoordSystemConversionMode.AsRotation:
					//        break;
					//    case eCoordSystemConversionMode.AsVelocity:
					//        break;
					//    case eCoordSystemConversionMode.AsScale:
					//        break;
					//}
					break;
				case AmpsHelpers.eCoordSystems.Emitter:
					switch (coordSystemConversionMode)
					{
						case eCoordSystemConversionMode.AsPosition:
							returnValue += ownerBlueprint.ownerEmitter.transform.position;
							returnValue = AmpsHelpers.RotateAroundPoint(returnValue, ownerBlueprint.ownerEmitter.emitterPosition, ownerBlueprint.ownerEmitter.transform.rotation);
							break;
						case eCoordSystemConversionMode.AsRotation:
							returnValue += ownerBlueprint.ownerEmitter.transform.rotation.eulerAngles;
							break;
						case eCoordSystemConversionMode.AsVelocity:
							returnValue = ownerBlueprint.ownerEmitter.emitterMatrixPositionZero.MultiplyPoint3x4(returnValue);
							break;
						case eCoordSystemConversionMode.AsScale:
							returnValue.x *= ownerBlueprint.ownerEmitter.transform.lossyScale.x;
							returnValue.y *= ownerBlueprint.ownerEmitter.transform.lossyScale.y;
							returnValue.z *= ownerBlueprint.ownerEmitter.transform.lossyScale.z;
							break;
					}
					break;
			}

			return AmpsHelpers.ConvertVector3Vector4(returnValue, 0);			
		}

		// RANDOMIZE //
		//
		public void Randomize()
		{
			System.Random theRandom = new System.Random();
			if (dataMode == eDataMode.Reference && reference != null) reference.property.Randomize();
			else randomSeed = theRandom.Next();
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		virtual public void Initialize(string theName, AmpsBlueprint theOwnerBlueprint)
		{
			name = theName;
			allowDataModeRandomConstant = true;
			allowDataModeCurve = true;
			allowDataModeRandomCurve = true;
			allowDataModeReference = true;
			dataMode = eDataMode.Constant;
			reference = null;
			ownerBlueprint = theOwnerBlueprint;
			coordSystem = AmpsHelpers.eCoordSystems.World;
			coordSystemConversionMode = eCoordSystemConversionMode.NoConversion;
		}

		// INITIALIZE //
		//
		public void Initialize()
		{
			Initialize("", null);
		}

		// COPY PROPERTY //
		//
		virtual public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			// We don't copy non-user-editable properties so their values
			// don't linger, code updates are followed in already placed properties
			// after the next blueprint commit.
			//
			//name = originalProperty.name;
			//allowDataModeRandomConstant = originalProperty.allowDataModeRandomConstant;
			//allowDataModeCurve = originalProperty.allowDataModeCurve;
			//allowDataModeRandomCurve = originalProperty.allowDataModeRandomCurve;
			//allowDataModeReference = originalProperty.allowDataModeReference;
			//allowDataModeParameter = originalProperty.allowDataModeParameter;
			dataMode = originalProperty.dataMode;
			reference = originalProperty.reference;
			parameterName = originalProperty.parameterName;
			parameter = originalProperty.parameter;
			if (theOwnerBlueprint != null) ownerBlueprint = theOwnerBlueprint;
			else ownerBlueprint = originalProperty.ownerBlueprint;
			coordSystem = originalProperty.coordSystem;
			coordSystemConversionMode = originalProperty.coordSystemConversionMode;
		}

		// PARAMETER HEADER //
		//
		public void ParameterHeader()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Parameter", GUILayout.Width(70));
			string previousParameterName = parameterName;
			parameterName = EditorGUILayout.TextField(parameterName);
			if (parameterName != previousParameterName) wasParameterQueried = false;
			GUILayout.EndHorizontal();
		}

		// SET DATA MODES //
		//
		public void SetDataModes(bool allowConstant, bool allowRandomConstant, bool allowCurve, bool allowRandomCurve,
								 bool allowReference, bool allowParameter)
		{
			allowDataModeConstant = allowConstant;
			allowDataModeRandomConstant = allowRandomConstant;
			allowDataModeCurve = allowCurve;
			allowDataModeRandomCurve = allowRandomCurve;
			allowDataModeReference = allowReference;
			allowDataModeParameter = allowParameter;

			if (allowDataModeConstant) dataMode = eDataMode.Constant;
			else if (allowDataModeRandomConstant) dataMode = eDataMode.RandomConstant;
			else if (allowDataModeCurve) dataMode = eDataMode.Curve;
			else if (allowDataModeRandomCurve) dataMode = eDataMode.RandomCurve;
			else if (allowDataModeReference) dataMode = eDataMode.Reference;
			else if (allowDataModeParameter) dataMode = eDataMode.Parameter;
		}

		// SET CONVERSION MODE //
		//
		public void SetConversionMode(AmpsHelpers.eStackFunction stackFunction)
		{
			if (stackFunction == AmpsHelpers.eStackFunction.Position)
				coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsPosition;
			else if (stackFunction == AmpsHelpers.eStackFunction.Rotation
					|| stackFunction == AmpsHelpers.eStackFunction.RotationRate)
				coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsRotation;
			else if (stackFunction == AmpsHelpers.eStackFunction.Velocity
					|| stackFunction == AmpsHelpers.eStackFunction.Acceleration)
				coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsVelocity;
			else if (stackFunction == AmpsHelpers.eStackFunction.Scale)
				coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsScale;
		}

		// SET DEFAULT COORD SYSTEM //
		//
		public void SetDefaultCoordSystem(AmpsHelpers.eStackFunction stackFunction)
		{
			if (stackFunction == AmpsHelpers.eStackFunction.Position
					|| stackFunction == AmpsHelpers.eStackFunction.Rotation
					|| stackFunction == AmpsHelpers.eStackFunction.Scale)
			{
				coordSystem = AmpsHelpers.eCoordSystems.Emitter;
			}
		}

		// CHECK REFERENCES //
		//
		public void CheckReferences()
		{
			if (reference != null && reference.property != null && reference.property.shouldBeRemoved)
			{
				reference.module = null;
				reference.property = null;
				reference = null;
			}
		}

		// SET MODE CONSTANT //
		//
		public void SetModeConstant()
		{
			dataMode = eDataMode.Constant;
		}

		// SET MODE RANDOM //
		//
		public void SetModeRandom()
		{
			dataMode = eDataMode.RandomConstant;
		}

		// SET MODE CURVE //
		//
		public void SetModeCurve()
		{
			dataMode = eDataMode.Curve;
		}

		// SET MODE RANDOM CURVE //
		//
		public void SetModeRandomCurve()
		{
			dataMode = eDataMode.RandomCurve;
		}

		// SET MODE REFERENCE //
		//
		public void SetModeReference()
		{
			dataMode = eDataMode.Reference;
		}

		// SET MODE PARAMETER //
		//
		public void SetModeParameter()
		{
			dataMode = eDataMode.Parameter;
		}

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

		// MY FLOAT FIELD //
		//
		public float MyFloatField(string label, float value, params GUILayoutOption[] options)
		{
			float returnValue;

			if (EditorGUIUtility.isProSkin)
			{
				GUILayout.BeginHorizontal(options);
				GUILayout.Label(label, "floatFieldLabel");
				//GUI.SetNextControlName(AmpsHelpers.propertyControlName);
				float.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(40)), out returnValue);
				GUILayout.EndHorizontal();
			}
			else
			{
				//GUI.SetNextControlName(AmpsHelpers.propertyControlName);
				returnValue = EditorGUILayout.FloatField(label, value, options);
			}

			return returnValue;
		}

		// MY INT FIELD //
		//
		public int MyIntField(string label, int value, params GUILayoutOption[] options)
		{
			int returnValue;

			if (EditorGUIUtility.isProSkin)
			{
				GUILayout.BeginHorizontal(options);
				GUILayout.Label(label, "floatFieldLabel");
				//GUI.SetNextControlName(AmpsHelpers.propertyControlName);
				int.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(40)), out returnValue);
				GUILayout.EndHorizontal();
			}
			else
			{
				//GUI.SetNextControlName(AmpsHelpers.propertyControlName);
				returnValue = EditorGUILayout.IntField(label, value, options);
			}

			return returnValue;
		}

		// PROPERTY MIN MAX //
		//
		public void PropertyMinMax(ref float min, ref float max)
		{
			EditorGUIUtility.LookLikeControls(30, 40);
			min = MyFloatField("Min", min, GUILayout.ExpandWidth(false));
			max = MyFloatField("Max", max, GUILayout.ExpandWidth(false));
			EditorGUIUtility.LookLikeControls();
		}

		// COORD SYSTEM POPUP //
		//
		public AmpsHelpers.eCoordSystems coordSystemPopup(AmpsHelpers.eCoordSystems theCoordSystem)
		{
			AmpsHelpers.eCoordSystems returnValue;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Relative to");
			returnValue = (AmpsHelpers.eCoordSystems)SortedPopup((int)theCoordSystem,
																	AmpsHelpers.coordSystemsDisplayData,
																	GUILayout.Width(100));

			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			return returnValue;
		}

		// CURVE PICKER INPUT //
		//
		public void CurvePickerInput(ScalarCurve theCurve)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Input");
			GUILayout.FlexibleSpace();

			theCurve.curveInput = (AmpsHelpers.eCurveInputs)SortedPopup((int)theCurve.curveInput,
																			AmpsHelpers.curveInputDisplayData,
																			GUILayout.Width(120));

			if (AmpsHelpers.isFloatInput(theCurve.curveInput) == false)
			{
				theCurve.curveInputVectorComponent = (AmpsHelpers.eVectorComponents)SortedPopup((int)theCurve.curveInputVectorComponent,
																								AmpsHelpers.vectorComponentsDisplayData,
																								GUILayout.Width(40));
			}
			else
			{
				GUILayout.Space(40);
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if (theCurve.curveInput == AmpsHelpers.eCurveInputs.Acceleration ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.Position ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.Rotation ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.RotationRate ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.Velocity)
			{
				theCurve.curveInputCoordSystem = coordSystemPopup(theCurve.curveInputCoordSystem);
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Random range?");
			GUILayout.FlexibleSpace();
			theCurve.isInputRangeRandom = GUILayout.Toggle(theCurve.isInputRangeRandom, "", "toggle");
			GUILayout.EndHorizontal();
			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
			EditorGUIUtility.LookLikeControls(30, 5);

			GUILayout.Space(50);
			GUILayout.BeginVertical();
			//GUILayout.Label("From");
			if (theCurve.isInputRangeRandom)
			{
				theCurve.inputRangeMin = MyFloatField("", theCurve.inputRangeMin, GUILayout.ExpandWidth(false));
				theCurve.inputRangeRandomMin = MyFloatField("", theCurve.inputRangeRandomMin, GUILayout.ExpandWidth(false));
			}
			else theCurve.inputRangeMin = MyFloatField("", theCurve.inputRangeMin, GUILayout.ExpandWidth(false));
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical();
			//GUILayout.Label("To");
			if (theCurve.isInputRangeRandom)
			{
				theCurve.inputRangeMax = MyFloatField("", theCurve.inputRangeMax, GUILayout.ExpandWidth(false));
				theCurve.inputRangeRandomMax = MyFloatField("", theCurve.inputRangeRandomMax, GUILayout.ExpandWidth(false));
			}
			else theCurve.inputRangeMax = MyFloatField("", theCurve.inputRangeMax, GUILayout.ExpandWidth(false));
			GUILayout.EndVertical();

			EditorGUIUtility.LookLikeControls();
			GUILayout.EndHorizontal();
		}

		// CURVE PICKER INPUT //
		//
		// HACK: It's exactly the same as the previous one, except the passed param type.
		public void CurvePickerInput(VectorCurve theCurve)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Input");
			GUILayout.FlexibleSpace();

			theCurve.curveInput = (AmpsHelpers.eCurveInputs)SortedPopup((int)theCurve.curveInput,
																			AmpsHelpers.curveInputDisplayData,
																			GUILayout.Width(120));
			if (AmpsHelpers.isFloatInput(theCurve.curveInput) == false)
			{
				theCurve.curveInputVectorComponent = (AmpsHelpers.eVectorComponents)SortedPopup((int)theCurve.curveInputVectorComponent,
																					AmpsHelpers.vectorComponentsDisplayData,
																					GUILayout.Width(40));
			}
			else
			{
				GUILayout.Space(40);
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if (theCurve.curveInput == AmpsHelpers.eCurveInputs.Acceleration ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.Position ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.Rotation ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.RotationRate ||
				theCurve.curveInput == AmpsHelpers.eCurveInputs.Velocity)
			{
				theCurve.curveInputCoordSystem = coordSystemPopup(theCurve.curveInputCoordSystem);
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label("Random range?");
			GUILayout.FlexibleSpace();
			theCurve.isInputRangeRandom = GUILayout.Toggle(theCurve.isInputRangeRandom, "", "toggle");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUIUtility.LookLikeControls(30, 40);
			theCurve.inputRangeMin = MyFloatField("    ", theCurve.inputRangeMin, GUILayout.ExpandWidth(false));
			GUILayout.FlexibleSpace();
			theCurve.inputRangeMax = MyFloatField("    ", theCurve.inputRangeMax, GUILayout.ExpandWidth(false));
			EditorGUIUtility.LookLikeControls();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (theCurve.isInputRangeRandom)
			{
				EditorGUIUtility.LookLikeControls(30, 40);
				theCurve.inputRangeRandomMin = MyFloatField("    ", theCurve.inputRangeRandomMin, GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				theCurve.inputRangeRandomMax = MyFloatField("    ", theCurve.inputRangeRandomMax, GUILayout.ExpandWidth(false));
				EditorGUIUtility.LookLikeControls();
			}
			GUILayout.EndHorizontal();
		}

		// CURVE PICKER OUTPUT //
		//
		public void CurvePickerOutput(string groupName, ref float rangeMin, ref float rangeMax)
		{
			GUILayout.BeginVertical();
			//GUILayout.Label(groupName);
			//GUILayout.FlexibleSpace();
			EditorGUIUtility.LookLikeControls(30, 5);
			rangeMax = MyFloatField("", rangeMax, GUILayout.ExpandWidth(false));
			GUILayout.Space(144);
			rangeMin = MyFloatField("", rangeMin, GUILayout.ExpandWidth(false));
			EditorGUIUtility.LookLikeControls();
			GUILayout.EndVertical();
		}

		// SCALAR RANDOM PICKER //
		//
		public void ScalarRandomPicker(string groupName, ref float min, ref float max)
		{
			float mean, deviance;

			mean = (min + max) / 2;
			deviance = mean - min;

			GUILayout.BeginHorizontal("valuePickerBox");

			GUILayout.BeginVertical();
			GUILayout.Space(16);
			GUILayout.Label(groupName, "valuePickerGroupLabel");
			GUILayout.Space(16);
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal(GUILayout.Width(170));
			GUILayout.FlexibleSpace();
			EditorGUIUtility.LookLikeControls(30, 35);
			min = MyFloatField("Min", min, GUILayout.ExpandWidth(false));
			
			max = MyFloatField("Max", max, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginVertical(GUILayout.Width(170));
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUIUtility.LookLikeControls(104, 35);
			deviance = MyFloatField("Deviance", deviance);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			mean = MyFloatField("Mean", mean);
			GUILayout.EndHorizontal();
			EditorGUIUtility.LookLikeControls();
			GUILayout.EndVertical();

			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			min = mean - deviance;
			max = mean + deviance;
		}

		// GET STYLE //
		//
		public GUIStyle GetStyle(BaseProperty selectedProperty)
		{
			if (selectedProperty == this) { return GUI.skin.GetStyle("propertyBoxSelected"); }
			else { return GUI.skin.GetStyle("propertyBoxNormal"); }
		}

		// SHOW HEADER //
		//
		public void ShowHeader(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			if (isReadOnly == false)
			{
				GUILayout.BeginHorizontal("propertyHeader");
				GUILayout.Label(name, "propertyName");
				GUILayout.FlexibleSpace();

				if ((allowDataModeConstant ? 1 : 0) +
					(allowDataModeRandomConstant ? 1 : 0) +
					(allowDataModeCurve ? 1 : 0) +
					(allowDataModeRandomCurve ? 1 : 0) +
					(allowDataModeReference ? 1 : 0) +
					(allowDataModeParameter ? 1 : 0) > 1)
				{
					if (GUILayout.Button("", "optionButton")) ShowOptions();
				}
				GUILayout.EndHorizontal();
			}
		}

		// SHOW MINI HEADER //
		//
		public void ShowMiniHeader(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			if (isReadOnly == false)
			{
				GUILayout.BeginHorizontal("propertyHeader");
				GUILayout.Label(name, "propertyName");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
		}

		// SHOW PROPERTY //
		//
		virtual public void ShowProperty(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			ShowHeader(ref selectedProperty, isReadOnly);
		}

		// HANDLE SELECTION //
		//
		public void HandleSelection(ref BaseProperty currentSelection, BaseProperty currentProperty)
		{
			if (Event.current.type == EventType.Repaint)
			{
				propertyRect = GUILayoutUtility.GetLastRect();
			}

			if (Event.current.type == EventType.MouseDown &&
				 propertyRect.Contains(Event.current.mousePosition))
			{
				shouldSelectThis = true;
			}

			if (Event.current.type == EventType.Layout && shouldSelectThis)
			{
				currentSelection = currentProperty;
				shouldSelectThis = false;
			}
		}

		// SHOW PICKER //
		//
		virtual public void ShowPicker()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("No value-picker available", "propertyGroupLabel");
			GUILayout.EndHorizontal();
		}

		// SHOW OPTIONS //
		//
		public void ShowOptions()
		{
			GenericMenu propertyOptionsMenu = new GenericMenu();
			propertyOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.ePropertyOperations.Constant.ToString())),
									false,
									SetModeConstant);

			if (allowDataModeRandomConstant)
			{
				propertyOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.ePropertyOperations.Random.ToString())),
										false,
										SetModeRandom);
			}

			if (allowDataModeCurve)
			{
				propertyOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.ePropertyOperations.Curve.ToString())),
										false,
										SetModeCurve);
			}

			if (allowDataModeRandomCurve)
			{
				propertyOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.ePropertyOperations.RandomCurve.ToString())),
										false,
										SetModeRandomCurve);
			}

			if (allowDataModeReference)
			{
				propertyOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.ePropertyOperations.Reference.ToString())),
										false,
										SetModeReference);
			}

			if (allowDataModeParameter)
			{
				propertyOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.ePropertyOperations.Parameter.ToString())),
										false,
										SetModeParameter);
			}
			propertyOptionsMenu.ShowAsContext();
		}

		// SHOW REFERENCE CONTROL //
		//
		public void ShowReferenceControl()
		{
			GUILayout.BeginHorizontal();
			if (reference != null) GUILayout.Label(reference.module.moduleName.GetValue() + "." + reference.property.name);
			else GUILayout.Label("No reference");
			if (GUILayout.Button("", "optionButton")) ShowReferences();
			GUILayout.EndHorizontal();
			if (reference != null)
			{
				GUI.enabled = false;
				BaseProperty dummy = this;
				reference.property.ShowProperty(ref dummy, true);
				GUI.enabled = true;
			}
		}

		// SHOW REFERENCES CALLBACK //
		//
		public void ShowReferencesCallback(object item)
		{
			reference = item as PropertyReference;
		}

		// SHOW REFERENCES //
		//
		public void ShowReferences()
		{
			GenericMenu propertyReferencesMenu = new GenericMenu();

			ownerBlueprint.ownerEmitter.UpdateSharedPropertyList();

			propertyReferencesMenu.AddItem(new GUIContent("No reference"),
													false,
													ShowReferencesCallback,
													null);
			propertyReferencesMenu.AddSeparator("");

			foreach (PropertyReference pr in ownerBlueprint.ownerEmitter.sharedProperties)
			{

				if (pr.property.GetType() == this.GetType()/* &&
				((pr.property.GetType() != typeof(DropdownProperty) ||
				 (pr.property.GetType() == typeof(DropdownProperty) && pr.property.name == this.name)))*/)
				{
					propertyReferencesMenu.AddItem(new GUIContent(pr.module.moduleName.GetValue() + "." + pr.property.name),
													false,
													ShowReferencesCallback,
													pr);
				}
			}

			propertyReferencesMenu.ShowAsContext();
		}
#endregion

#endif
	}
}