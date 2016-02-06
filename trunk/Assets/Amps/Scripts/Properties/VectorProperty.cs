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
	public class VectorProperty : BaseProperty
	{
		public Vector4 constant;
		public Vector4 randomMin;
		public Vector4 randomMax;

		// TODO: Investigate if curve, curveMin and curveMax should share input settings.
		public VectorCurve curve;
		public VectorCurve curveMin;
		public VectorCurve curveMax;
		public bool usePerComponentRandom;
		public bool useExtremes;
		public bool hideW;

		// GET VALUE //
		//
		// GetValue when particle index is NOT known.
		public Vector4 GetValue()
		{
			Vector4 returnValue = Vector4.zero;
			Vector4 curveMinResult = Vector4.zero;
			Vector4 curveMaxResult = Vector4.zero;
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
					if (usePerComponentRandom)
					{
						if (useExtremes)
						{
							if ((float)theRandom.NextDouble() < 0.5) returnValue.x = randomMin.x;
							else returnValue.x = randomMax.x;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.y = randomMin.y;
							else returnValue.y = randomMax.y;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.z = randomMin.z;
							else returnValue.z = randomMax.z;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.w = randomMin.w;
							else returnValue.w = randomMax.w;
						}
						else
						{
							returnValue.x = Mathf.Lerp(randomMin.x, randomMax.x, (float)theRandom.NextDouble());
							returnValue.y = Mathf.Lerp(randomMin.y, randomMax.y, (float)theRandom.NextDouble());
							returnValue.z = Mathf.Lerp(randomMin.z, randomMax.z, (float)theRandom.NextDouble());
							returnValue.w = Mathf.Lerp(randomMin.w, randomMax.w, (float)theRandom.NextDouble());
						}
					}
					else returnValue = Vector4.Lerp(randomMin, randomMax, (float)theRandom.NextDouble());
					break;
				case eDataMode.Curve:
					returnValue = curve.Evaluate(ownerBlueprint.ownerEmitter);
					break;
				case eDataMode.RandomCurve:
					if (usePerComponentRandom)
					{
						curveMinResult = curveMin.Evaluate(ownerBlueprint.ownerEmitter);
						curveMaxResult = curveMax.Evaluate(ownerBlueprint.ownerEmitter);

						if (useExtremes)
						{
							if ((float)theRandom.NextDouble() < 0.5) returnValue.x = curveMinResult.x;
							else returnValue.x = curveMaxResult.x;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.y = curveMinResult.y;
							else returnValue.y = curveMaxResult.y;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.z = curveMinResult.z;
							else returnValue.z = curveMaxResult.z;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.w = curveMinResult.w;
							else returnValue.w = curveMaxResult.w;
						}
						else
						{
							returnValue.x = Mathf.Lerp(curveMinResult.x, curveMaxResult.x, (float)theRandom.NextDouble());
							returnValue.y = Mathf.Lerp(curveMinResult.y, curveMaxResult.y, (float)theRandom.NextDouble());
							returnValue.z = Mathf.Lerp(curveMinResult.z, curveMaxResult.z, (float)theRandom.NextDouble());
							returnValue.w = Mathf.Lerp(curveMinResult.w, curveMaxResult.w, (float)theRandom.NextDouble());
						}
					}
					else returnValue = Vector4.Lerp(curveMin.Evaluate(ownerBlueprint.ownerEmitter), curveMax.Evaluate(ownerBlueprint.ownerEmitter), (float)theRandom.NextDouble());
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						VectorProperty theReference = (VectorProperty)reference.property;
						returnValue = theReference.GetValue();
					}
					break;
				case eDataMode.Parameter:
					if (wasParameterQueried == false)
					{
						parameter = ownerBlueprint.ownerEmitter.GetParameter(parameterName, AmpsHelpers.eParameterTypes.Vector);
						wasParameterQueried = true;
					}
					if (parameter != null)
					{
						returnValue = parameter.GetVectorValue();
					}
					else returnValue = constant;
					break;
			}

			if (coordSystemConversionMode != BaseProperty.eCoordSystemConversionMode.NoConversion)
			{
				returnValue = ConvertCoordinateSystem(returnValue, -1);
			}

			return returnValue;
		}

		// GET VALUE //
		//
		// GetValue when particle index IS known.
		public Vector4 GetValue(int particleIndex)
		{
			Vector4 returnValue = Vector4.zero;
			Vector4 curveMinResult = Vector4.zero;
			Vector4 curveMaxResult = Vector4.zero;
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
					if (usePerComponentRandom)
					{
						if (useExtremes)
						{
							if ((float)theRandom.NextDouble() < 0.5) returnValue.x = randomMin.x;
							else returnValue.x = randomMax.x;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.y = randomMin.y;
							else returnValue.y = randomMax.y;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.z = randomMin.z;
							else returnValue.z = randomMax.z;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.w = randomMin.w;
							else returnValue.w = randomMax.w;
						}
						else
						{
							returnValue.x = Mathf.Lerp(randomMin.x, randomMax.x, (float)theRandom.NextDouble());
							returnValue.y = Mathf.Lerp(randomMin.y, randomMax.y, (float)theRandom.NextDouble());
							returnValue.z = Mathf.Lerp(randomMin.z, randomMax.z, (float)theRandom.NextDouble());
							returnValue.w = Mathf.Lerp(randomMin.w, randomMax.w, (float)theRandom.NextDouble());
						}
					}
					else returnValue = Vector4.Lerp(randomMin, randomMax, (float)theRandom.NextDouble());
					break;
				case eDataMode.Curve:
					returnValue = curve.Evaluate(ownerBlueprint.ownerEmitter, particleIndex);
					break;
				case eDataMode.RandomCurve:
					if (usePerComponentRandom)
					{
						curveMinResult = curveMin.Evaluate(ownerBlueprint.ownerEmitter, particleIndex);
						curveMaxResult = curveMax.Evaluate(ownerBlueprint.ownerEmitter, particleIndex);

						if (useExtremes)
						{
							if ((float)theRandom.NextDouble() < 0.5) returnValue.x = curveMinResult.x;
							else returnValue.x = curveMaxResult.x;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.y = curveMinResult.y;
							else returnValue.y = curveMaxResult.y;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.z = curveMinResult.z;
							else returnValue.z = curveMaxResult.z;
							if ((float)theRandom.NextDouble() < 0.5) returnValue.w = curveMinResult.w;
							else returnValue.w = curveMaxResult.w;
						}
						else
						{
							returnValue.x = Mathf.Lerp(curveMinResult.x, curveMaxResult.x, (float)theRandom.NextDouble());
							returnValue.y = Mathf.Lerp(curveMinResult.y, curveMaxResult.y, (float)theRandom.NextDouble());
							returnValue.z = Mathf.Lerp(curveMinResult.z, curveMaxResult.z, (float)theRandom.NextDouble());
							returnValue.w = Mathf.Lerp(curveMinResult.w, curveMaxResult.w, (float)theRandom.NextDouble());
						}
					}
					else returnValue = Vector4.Lerp(curveMin.Evaluate(ownerBlueprint.ownerEmitter, particleIndex), curveMax.Evaluate(ownerBlueprint.ownerEmitter, particleIndex), (float)theRandom.NextDouble());
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						VectorProperty theReference = (VectorProperty)reference.property;
						returnValue = theReference.GetValue(particleIndex);
					}
					break;
				case eDataMode.Parameter:
					if (wasParameterQueried == false)
					{
						parameter = ownerBlueprint.ownerEmitter.GetParameter(parameterName, AmpsHelpers.eParameterTypes.Vector);
						wasParameterQueried = true;
					}
					if (parameter != null)
					{
						returnValue = parameter.GetVectorValue();
					}
					else returnValue = constant;
					break;
			}

			if (coordSystemConversionMode != BaseProperty.eCoordSystemConversionMode.NoConversion)
			{
				returnValue = ConvertCoordinateSystem(returnValue, particleIndex);
			}
			
			return returnValue;
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(string theName, AmpsBlueprint theOwnerBlueprint)
		{
			Initialize(theName, Vector4.zero, theOwnerBlueprint);
		}

		// INITIALIZE //
		//
		public void Initialize(string theName, Vector4 v, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theName, theOwnerBlueprint);

			constant = v;
			randomMin = Vector4.zero;
			randomMax = v;
			curve = ScriptableObject.CreateInstance<VectorCurve>();
			curve.Initialize(Vector4.one, theName + "_curve");
			curveMin = ScriptableObject.CreateInstance<VectorCurve>();
			curveMin.Initialize(Vector4.zero, theName + "_curveMin");
			curveMax = ScriptableObject.CreateInstance<VectorCurve>();
			curveMax.Initialize(Vector4.one, theName + "_curveMax");
			usePerComponentRandom = true;
			useExtremes = false;
			hideW = false;
			reference = null;
			Randomize();
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);

			VectorProperty originalVectorProperty = originalProperty as VectorProperty;
			constant = originalVectorProperty.constant;
			randomMin = originalVectorProperty.randomMin;
			randomMax = originalVectorProperty.randomMax;
			curve = ScriptableObject.CreateInstance<VectorCurve>();
			curve.Initialize(originalVectorProperty.curve);
			curveMin = ScriptableObject.CreateInstance<VectorCurve>();
			curveMin.Initialize(originalVectorProperty.curveMin);
			curveMax = ScriptableObject.CreateInstance<VectorCurve>();
			curveMax.Initialize(originalVectorProperty.curveMax);
			usePerComponentRandom = originalVectorProperty.usePerComponentRandom;
			useExtremes = originalVectorProperty.useExtremes;
			hideW = originalVectorProperty.hideW;
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
					EditorGUIUtility.fieldWidth = 35;
					constant.x = MyFloatField("X", constant.x, GUILayout.ExpandWidth(false));
					constant.y = MyFloatField("Y", constant.y, GUILayout.ExpandWidth(false));
					constant.z = MyFloatField("Z", constant.z, GUILayout.ExpandWidth(false));
					EditorGUIUtility.labelWidth = 0;
					EditorGUIUtility.fieldWidth = 0;
					GUILayout.EndHorizontal();

					if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
					{
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUIUtility.labelWidth = 20;
						EditorGUIUtility.fieldWidth = 35;
						constant.w = MyFloatField("W", constant.w, GUILayout.ExpandWidth(false));
						EditorGUIUtility.labelWidth = 0;
						EditorGUIUtility.fieldWidth = 0;
						GUILayout.EndHorizontal();
					}
					break;

				case eDataMode.RandomConstant:
					GUILayout.BeginHorizontal();
					GUILayout.Label("X", "propertyLabel", GUILayout.Width(16));
					GUILayout.FlexibleSpace();
					PropertyMinMax(ref randomMin.x, ref randomMax.x);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Y", "propertyLabel", GUILayout.Width(16));
					GUILayout.FlexibleSpace();
					PropertyMinMax(ref randomMin.y, ref randomMax.y);
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Z", "propertyLabel", GUILayout.Width(16));
					GUILayout.FlexibleSpace();
					PropertyMinMax(ref randomMin.z, ref randomMax.z);
					GUILayout.EndHorizontal();

					if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
					{
						EditorGUILayout.Space();
						GUILayout.BeginHorizontal();
						GUILayout.Label("W", "propertyLabel", GUILayout.Width(16));
						GUILayout.FlexibleSpace();
						PropertyMinMax(ref randomMin.w, ref randomMax.w);
						GUILayout.EndHorizontal();
					}

					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Per component random", "propertyLabel");
					GUILayout.FlexibleSpace();
					usePerComponentRandom = GUILayout.Toggle(usePerComponentRandom, "", "toggle");
					GUILayout.EndHorizontal();

					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Use extreme values", "propertyLabel");
					GUILayout.FlexibleSpace();
					useExtremes = GUILayout.Toggle(useExtremes, "", "toggle");
					GUILayout.EndHorizontal();
					break;

				case eDataMode.Curve:
					GUILayout.BeginHorizontal();
					GUILayout.Label("X", GUILayout.Width(16));
					curve.curveX = EditorGUILayout.CurveField(curve.curveX, Color.red, new Rect(0, 0, 1, 1));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Y", GUILayout.Width(16));
					curve.curveY = EditorGUILayout.CurveField(curve.curveY, Color.green, new Rect(0, 0, 1, 1));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Z", GUILayout.Width(16));
					curve.curveZ = EditorGUILayout.CurveField(curve.curveZ, new Color(0f, 0.75f, 1f, 1f), new Rect(0, 0, 1, 1));
					GUILayout.EndHorizontal();

					if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
					{
						EditorGUILayout.Space();
						GUILayout.BeginHorizontal();
						GUILayout.Label("W", GUILayout.Width(16));
						curve.curveW = EditorGUILayout.CurveField(curve.curveW, Color.white, new Rect(0, 0, 1, 1));
						GUILayout.EndHorizontal();
					}
					break;

				case eDataMode.RandomCurve:
					GUILayout.BeginHorizontal();
					GUILayout.Label("X", GUILayout.Width(16));
					curveMin.curveX = EditorGUILayout.CurveField(curveMin.curveX, new Color(0.9f, 0f, 0f, 1f), new Rect(0, 0, 1, 1));
					curveMax.curveX = EditorGUILayout.CurveField(curveMax.curveX, new Color(1f, 0.6f, 0.6f, 1f), new Rect(0, 0, 1, 1));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Y", GUILayout.Width(16));
					curveMin.curveY = EditorGUILayout.CurveField(curveMin.curveY, new Color(0f, 0.9f, 0f, 1f), new Rect(0, 0, 1, 1));
					curveMax.curveY = EditorGUILayout.CurveField(curveMax.curveY, new Color(0.6f, 1f, 0.6f, 1f), new Rect(0, 0, 1, 1));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("Z", GUILayout.Width(16));
					curveMin.curveZ = EditorGUILayout.CurveField(curveMin.curveZ, new Color(0f, 0.9f, 0.8f, 1f), new Rect(0, 0, 1, 1));
					curveMax.curveZ = EditorGUILayout.CurveField(curveMax.curveZ, new Color(0.6f, 1f, 1f, 1f), new Rect(0, 0, 1, 1));
					GUILayout.EndHorizontal();

					if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
					{
						EditorGUILayout.Space();
						GUILayout.BeginHorizontal();
						GUILayout.Label("W", GUILayout.Width(16));
						curveMin.curveW = EditorGUILayout.CurveField(curveMin.curveW, new Color(0.8f, 0.8f, 0.8f, 1f), new Rect(0, 0, 1, 1));
						curveMax.curveW = EditorGUILayout.CurveField(curveMax.curveW, new Color(1f, 1f, 1f, 1f), new Rect(0, 0, 1, 1));
						GUILayout.EndHorizontal();
					}
					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Per component random");
					GUILayout.FlexibleSpace();
					usePerComponentRandom = GUILayout.Toggle(usePerComponentRandom, "", "toggle");
					GUILayout.EndHorizontal();

					EditorGUILayout.Space();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Use extreme values", "propertyLabel");
					GUILayout.FlexibleSpace();
					useExtremes = GUILayout.Toggle(useExtremes, "", "toggle");
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
					GUILayout.FlexibleSpace();
					EditorGUIUtility.labelWidth = 20;
					EditorGUIUtility.fieldWidth = 35;
					constant.x = MyFloatField("X", constant.x, GUILayout.ExpandWidth(false));
					constant.y = MyFloatField("Y", constant.y, GUILayout.ExpandWidth(false));
					constant.z = MyFloatField("Z", constant.z, GUILayout.ExpandWidth(false));
					EditorGUIUtility.labelWidth = 0;
					EditorGUIUtility.fieldWidth = 0;
					GUILayout.EndHorizontal();

					if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
					{
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						EditorGUIUtility.labelWidth = 20;
						EditorGUIUtility.fieldWidth = 35;
						constant.w = MyFloatField("W", constant.w, GUILayout.ExpandWidth(false));
						EditorGUIUtility.labelWidth = 0;
						EditorGUIUtility.fieldWidth = 0;
						GUILayout.EndHorizontal();
					}

					GUILayout.EndVertical();
					break;
			}

			if (dataMode != eDataMode.Reference && coordSystemConversionMode != eCoordSystemConversionMode.NoConversion)
			{
				EditorGUILayout.Space();
				coordSystem = coordSystemPopup(coordSystem);
			}

			GUILayout.EndVertical();

			HandleSelection(ref selectedProperty, this);
		}

		// SHOW PICKER //
		//
		override public void ShowPicker()
		{
			switch (dataMode)
			{
				case eDataMode.Constant:
					VectorPicker();
					break;
				case eDataMode.RandomConstant:
					VectorRandomPicker();
					break;
				case eDataMode.Curve:
					VectorCurvePicker();
					break;
				case eDataMode.RandomCurve:
					VectorRandomCurvePicker();
					break;
			}
		}

		// VECTOR PICKER //
		//
		void VectorPicker()
		{
			Vector3 vec3 = new Vector3(constant.x, constant.y, constant.z);
			float originalMagnitude = vec3.magnitude;
			float newMagnitude = originalMagnitude;

			GUILayout.BeginVertical("valuePickerBox");

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUIUtility.labelWidth = 30;
			EditorGUIUtility.fieldWidth = 35;
			constant.x = MyFloatField("X", constant.x, GUILayout.ExpandWidth(false));
			constant.y = MyFloatField("Y", constant.y, GUILayout.ExpandWidth(false));
			constant.z = MyFloatField("Z", constant.z, GUILayout.ExpandWidth(false));
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUIUtility.labelWidth = 65;
			EditorGUIUtility.fieldWidth = 35;
			newMagnitude = MyFloatField("Magnitude", newMagnitude, GUILayout.ExpandWidth(false));
			EditorGUIUtility.labelWidth = 0;
			EditorGUIUtility.fieldWidth = 0;
			if (newMagnitude != originalMagnitude && originalMagnitude != 0)
			{
				vec3 *= newMagnitude / originalMagnitude;
				constant.x = vec3.x;
				constant.y = vec3.y;
				constant.z = vec3.z;
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
			{
				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUIUtility.labelWidth = 30;
				EditorGUIUtility.fieldWidth = 35;
				constant.w = MyFloatField("W", constant.w, GUILayout.ExpandWidth(false));
				EditorGUIUtility.labelWidth = 0;
				EditorGUIUtility.fieldWidth = 0;
				GUILayout.EndHorizontal();
				EditorGUILayout.Space();
			}
			GUILayout.EndVertical();
		}

		// VECTOR RANDOM PICKER //
		//
		void VectorRandomPicker()
		{
			ScalarRandomPicker("X", ref randomMin.x, ref randomMax.x);
			ScalarRandomPicker("Y", ref randomMin.y, ref randomMax.y);
			ScalarRandomPicker("Z", ref randomMin.z, ref randomMax.z);
			if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
			{
				EditorGUILayout.Space();
				ScalarRandomPicker("W", ref randomMin.w, ref randomMax.w);
			}
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Per component random", "propertyLabel");
			GUILayout.FlexibleSpace();
			usePerComponentRandom = GUILayout.Toggle(usePerComponentRandom, "", "toggle");
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Use extreme values", "propertyLabel");
			GUILayout.FlexibleSpace();
			useExtremes = GUILayout.Toggle(useExtremes, "", "toggle");
			GUILayout.EndHorizontal();
		}

		// VECTOR CURVE PICKER //
		//
		void VectorCurvePicker()
		{
			GUILayout.BeginVertical("valuePickerBox");

			CurvePickerInput(curve);

			GUILayout.Label("X", GUILayout.Width(16));
			GUILayout.BeginHorizontal(GUILayout.Height(180));
			CurvePickerOutput("Output range", ref curve.outputRangeXMin, ref curve.outputRangeXMax);
			curve.curveX = EditorGUILayout.CurveField(curve.curveX, Color.red, new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.Label("Y", GUILayout.Width(16));
			GUILayout.BeginHorizontal(GUILayout.Height(180));
			CurvePickerOutput("Output range", ref curve.outputRangeYMin, ref curve.outputRangeYMax);
			curve.curveY = EditorGUILayout.CurveField(curve.curveY, Color.green, new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.Label("Z", GUILayout.Width(16));
			GUILayout.BeginHorizontal(GUILayout.Height(180));
			CurvePickerOutput("Output range", ref curve.outputRangeZMin, ref curve.outputRangeZMax);
			curve.curveZ = EditorGUILayout.CurveField(curve.curveZ, new Color(0f, 0.75f, 1f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			GUILayout.EndHorizontal();

			if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
			{
				EditorGUILayout.Space();

				GUILayout.Label("W", GUILayout.Width(16));
				GUILayout.BeginHorizontal(GUILayout.Height(180));
				CurvePickerOutput("Output range", ref curve.outputRangeWMin, ref curve.outputRangeWMax);
				curve.curveW = EditorGUILayout.CurveField(curve.curveW, Color.white, new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
				GUILayout.EndHorizontal();
			}
			EditorGUILayout.Space();
			GUILayout.EndVertical();
		}

		// VECTOR RANDOM CURVE PICKER //
		//
		void VectorRandomCurvePicker()
		{
			GUILayout.BeginVertical("valuePickerBox");

			CurvePickerInput(curveMin);

			GUILayout.Label("X", GUILayout.Width(16));
			GUILayout.BeginHorizontal(GUILayout.Height(180));
			CurvePickerOutput("Output range", ref curve.outputRangeXMin, ref curve.outputRangeXMax);
			GUILayout.BeginVertical(GUILayout.Height(180));
			curveMin.curveX = EditorGUILayout.CurveField(curveMin.curveX, new Color(0.9f, 0f, 0f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			curveMax.curveX = EditorGUILayout.CurveField(curveMax.curveX, new Color(1f, 0.6f, 0.6f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.Label("Y", GUILayout.Width(16));
			GUILayout.BeginHorizontal(GUILayout.Height(180));
			CurvePickerOutput("Output range", ref curve.outputRangeYMin, ref curve.outputRangeYMax);
			GUILayout.BeginVertical(GUILayout.Height(180));
			curveMin.curveY = EditorGUILayout.CurveField(curveMin.curveY, new Color(0f, 0.9f, 0f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			curveMax.curveY = EditorGUILayout.CurveField(curveMax.curveY, new Color(0.6f, 1f, 0.6f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.Label("Z", GUILayout.Width(16));
			GUILayout.BeginHorizontal(GUILayout.Height(180));
			CurvePickerOutput("Output range", ref curve.outputRangeZMin, ref curve.outputRangeZMax);
			GUILayout.BeginVertical(GUILayout.Height(180));
			curveMin.curveZ = EditorGUILayout.CurveField(curveMin.curveZ, new Color(0f, 0.9f, 0.8f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			curveMax.curveZ = EditorGUILayout.CurveField(curveMax.curveZ, new Color(0.6f, 1f, 1f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			if (ownerBlueprint.ownerEmitter.selectedStack.isVector3Stack == false && hideW == false)
			{
				EditorGUILayout.Space();

				GUILayout.Label("W", GUILayout.Width(16));
				GUILayout.BeginHorizontal(GUILayout.Height(180));
				CurvePickerOutput("Output range", ref curve.outputRangeWMin, ref curve.outputRangeWMax);
				GUILayout.BeginVertical(GUILayout.Height(180));
				curveMin.curveW = EditorGUILayout.CurveField(curveMin.curveW, new Color(0.8f, 0.8f, 0.8f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
				curveMax.curveW = EditorGUILayout.CurveField(curveMax.curveW, new Color(1f, 1f, 1f, 1f), new Rect(0, 0, 1, 1), GUILayout.ExpandHeight(true));
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Per component random", "propertyLabel");
			GUILayout.FlexibleSpace();
			usePerComponentRandom = GUILayout.Toggle(usePerComponentRandom, "", "toggle");
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Use extreme values", "propertyLabel");
			GUILayout.FlexibleSpace();
			useExtremes = GUILayout.Toggle(useExtremes, "", "toggle");
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
			GUILayout.EndVertical();

			curveMax.curveInput = curveMin.curveInput;
			curveMax.curveInputColorComponent = curveMin.curveInputColorComponent;
			curveMax.curveInputVectorComponent = curveMin.curveInputVectorComponent;
			curveMax.curveInputCoordSystem = curveMin.curveInputCoordSystem;
			curveMax.inputRangeMin = curveMin.inputRangeMin;
			curveMax.inputRangeMax = curveMin.inputRangeMax;
			curveMax.outputRangeXMin = curveMin.outputRangeXMin;
			curveMax.outputRangeXMax = curveMin.outputRangeXMax;
			curveMax.outputRangeYMin = curveMin.outputRangeYMin;
			curveMax.outputRangeYMax = curveMin.outputRangeYMax;
			curveMax.outputRangeZMin = curveMin.outputRangeZMin;
			curveMax.outputRangeZMax = curveMin.outputRangeZMax;
			curveMax.outputRangeWMin = curveMin.outputRangeWMin;
			curveMax.outputRangeWMax = curveMin.outputRangeWMax;
		}
#endregion

#endif
	}
}