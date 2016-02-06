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
	public class ScalarProperty : BaseProperty
	{
		public float constant = 0;
		public float randomMin = 0;
		public float randomMax = 1;
		public ScalarCurve curve;
		public ScalarCurve curveMin;
		public ScalarCurve curveMax;
		public bool isInteger = false;

		// GET VALUE //
		//
		// GetValue when particle index is NOT known.
		public float GetValue()
		{
			float returnValue = 0;
			System.Random theRandom = new System.Random(ownerBlueprint.ownerEmitter.randomSeed + randomSeed);

#if UNITY_EDITOR
			CheckReferences();
#endif

			switch (dataMode)
			{
				case eDataMode.Constant:
					returnValue = constant;
					break;
				case eDataMode.RandomConstant:
					returnValue = Mathf.Lerp(randomMin, randomMax, (float)theRandom.NextDouble());
					break;
				case eDataMode.Curve:
					returnValue = curve.Evaluate(ownerBlueprint.ownerEmitter);
					break;
				case eDataMode.RandomCurve:
					returnValue = Mathf.Lerp(curveMin.Evaluate(ownerBlueprint.ownerEmitter), curveMax.Evaluate(ownerBlueprint.ownerEmitter), (float)theRandom.NextDouble());
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						ScalarProperty theReference = (ScalarProperty)reference.property;
						returnValue = theReference.GetValue();
					}
					break;
				case eDataMode.Parameter:
					if (wasParameterQueried == false)
					{
						parameter = ownerBlueprint.ownerEmitter.GetParameter(parameterName, AmpsHelpers.eParameterTypes.Scalar);
						wasParameterQueried = true;
					}
					if (parameter != null)
					{
						returnValue = parameter.GetScalarValue();
					}
					else returnValue = constant;
					break;
			}

			if (isInteger) returnValue = (int)returnValue;	// Round toward zero.

			return returnValue;
		}

		// GET VALUE //
		//
		// GetValue when particle index IS known.
		public float GetValue(int particleIndex)
		{
			float returnValue = 0;

			System.Random theRandom = new System.Random(ownerBlueprint.ownerEmitter.randomSeed + randomSeed + ownerBlueprint.ownerEmitter.particleIds[particleIndex]);

#if UNITY_EDITOR
			CheckReferences();
#endif

			switch (dataMode)
			{
				case eDataMode.Constant:
					returnValue = constant;
					break;
				case eDataMode.RandomConstant:
					returnValue = Mathf.Lerp(randomMin, randomMax, (float)theRandom.NextDouble());
					break;
				case eDataMode.Curve:
					returnValue = curve.Evaluate(ownerBlueprint.ownerEmitter, particleIndex);
					break;
				case eDataMode.RandomCurve:
					returnValue = Mathf.Lerp(curveMin.Evaluate(ownerBlueprint.ownerEmitter, particleIndex), curveMax.Evaluate(ownerBlueprint.ownerEmitter, particleIndex), (float)theRandom.NextDouble());
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						ScalarProperty theReference = (ScalarProperty)reference.property;
						returnValue = theReference.GetValue(particleIndex);
					}
					break;
				case eDataMode.Parameter:
					if (wasParameterQueried == false)
					{
						parameter = ownerBlueprint.ownerEmitter.GetParameter(parameterName, AmpsHelpers.eParameterTypes.Scalar);
						wasParameterQueried = true;
					}
					if (parameter != null)
					{
						returnValue = parameter.GetScalarValue();
					}
					else returnValue = constant;
					break;
			}

			if (isInteger) returnValue = (int)returnValue;	// Round toward zero.

			return returnValue;
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(string theName, AmpsBlueprint theOwnerBlueprint)
		{
			Initialize(theName, 0f, theOwnerBlueprint);
		}

		// INITIALIZE //
		//
		public void Initialize(string theName, float f, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theName, theOwnerBlueprint);

			constant = f;
			randomMax = f;
			curve = ScriptableObject.CreateInstance<ScalarCurve>();
			curve.Initialize(f, theName + "_curve");
			curveMin = ScriptableObject.CreateInstance<ScalarCurve>();
			curveMin.Initialize(f, theName + "_curveMin");
			curveMax = ScriptableObject.CreateInstance<ScalarCurve>();
			curveMax.Initialize(f, theName + "_curveMax");
			isInteger = false;
			Randomize();
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);

			ScalarProperty originalScalarProperty = originalProperty as ScalarProperty;
			constant = originalScalarProperty.constant;
			randomMin = originalScalarProperty.randomMin;
			randomMax = originalScalarProperty.randomMax;
			curve = ScriptableObject.CreateInstance<ScalarCurve>();
			curve.Initialize(originalScalarProperty.curve);
			curveMin = ScriptableObject.CreateInstance<ScalarCurve>();
			curveMin.Initialize(originalScalarProperty.curveMin);
			curveMax = ScriptableObject.CreateInstance<ScalarCurve>();
			curveMax.Initialize(originalScalarProperty.curveMax);
			isInteger = originalScalarProperty.isInteger;
			Randomize();
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
					GUILayout.FlexibleSpace();
					EditorGUIUtility.labelWidth = 20;
					EditorGUIUtility.fieldWidth = 40;
					if (isInteger) constant = MyIntField("=", (int)constant);
					else constant = MyFloatField("=", constant);
					EditorGUIUtility.labelWidth = 0;
					EditorGUIUtility.fieldWidth = 0;
					GUILayout.EndHorizontal();
					break;
				case eDataMode.RandomConstant:
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUIUtility.labelWidth = 30;
					EditorGUIUtility.fieldWidth = 40;
					if (isInteger)
					{
						randomMin = MyIntField("Min", (int)randomMin, GUILayout.ExpandWidth(false));
						randomMax = MyIntField("Max", (int)randomMax, GUILayout.ExpandWidth(false));

					}
					else
					{
						randomMin = MyFloatField("Min", randomMin, GUILayout.ExpandWidth(false));
						randomMax = MyFloatField("Max", randomMax, GUILayout.ExpandWidth(false));
					}
					EditorGUIUtility.labelWidth = 0;
					EditorGUIUtility.fieldWidth = 0;
					GUILayout.EndHorizontal();
					break;
				case eDataMode.Curve:
					curve.curve = EditorGUILayout.CurveField(curve.curve, new Color(1f, 1f, 1f, 1f), new Rect(0, 0, 1, 1));
					break;
				case eDataMode.RandomCurve:
					GUILayout.BeginHorizontal();
					curveMin.curve = EditorGUILayout.CurveField(curveMin.curve, new Color(0.8f, 0.8f, 0.8f, 1f), new Rect(0, 0, 1, 1));
					curveMax.curve = EditorGUILayout.CurveField(curveMax.curve, new Color(1f, 1f, 1f, 1f), new Rect(0, 0, 1, 1));
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
					EditorGUIUtility.labelWidth = 74;
					EditorGUIUtility.fieldWidth = 40;
					constant = MyFloatField("Default", constant);
					EditorGUIUtility.labelWidth = 0;
					EditorGUIUtility.fieldWidth = 0;
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
					break;
			}

			GUILayout.EndVertical();

			HandleSelection(ref selectedProperty, this);
		}

		// SHOW PICKER //
		//
		override public void ShowPicker()
		{
			if (isInteger) base.ShowPicker();
			else
			{
				switch (dataMode)
				{
					case eDataMode.Constant:
						base.ShowPicker();
						break;
					case eDataMode.RandomConstant:
						ScalarRandomPicker("", ref randomMin, ref randomMax);
						break;
					case eDataMode.Curve:
						ScalarCurvePicker();
						break;
					case eDataMode.RandomCurve:
						ScalarRandomCurvePicker();
						break;
				}
			}
		}

		// SCALAR CURVE PICKER //
		//
		void ScalarCurvePicker()
		{
			GUILayout.BeginVertical("valuePickerBox");

			CurvePickerInput(curve);

			GUILayout.BeginHorizontal();
			CurvePickerOutput("Output range", ref curve.outputRangeMin, ref curve.outputRangeMax);
			curve.curve = EditorGUILayout.CurveField(curve.curve, new Color(1f, 1f, 1f), new Rect(0, 0, 1, 1), GUILayout.Height(180));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		// SCALAR RANDOM CURVE PICKER //
		//
		void ScalarRandomCurvePicker()
		{

			GUILayout.BeginVertical("valuePickerBox");

			CurvePickerInput(curveMin);

			GUILayout.BeginHorizontal();
			CurvePickerOutput("Output range", ref curveMin.outputRangeMin, ref curveMin.outputRangeMax);
			GUILayout.BeginVertical();
			curveMin.curve = EditorGUILayout.CurveField(curveMin.curve, new Color(0.5f, 0.5f, 1f), new Rect(0, 0, 1, 1), GUILayout.Height(90));
			EditorGUILayout.Space();
			curveMax.curve = EditorGUILayout.CurveField(curveMax.curve, new Color(1f, 0f, 0f), new Rect(0, 0, 1, 1), GUILayout.Height(90));
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			curveMax.curveInput = curveMin.curveInput;
			curveMax.curveInputColorComponent = curveMin.curveInputColorComponent;
			curveMax.curveInputVectorComponent = curveMin.curveInputVectorComponent;
			curveMax.curveInputCoordSystem = curveMin.curveInputCoordSystem;
			curveMax.inputRangeMin = curveMin.inputRangeMin;
			curveMax.inputRangeMax = curveMin.inputRangeMax;
			curveMax.outputRangeMin = curveMin.outputRangeMin;
			curveMax.outputRangeMax = curveMin.outputRangeMax;
		}
#endregion

#endif
	}
}