using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	// Half way implemented.

	[System.Serializable]
	public class ColorProperty : BaseProperty
	{
		public Vector4 constant;
		public Vector4 randomMin;
		public Vector4 randomMax;
		public VectorCurve curve;
		public VectorCurve curveMin;
		public VectorCurve curveMax;
		public bool usePerComponentRandom;

//============================================================================//
#if UNITY_EDITOR
		private bool[] colorPickerHdrToggles = new bool[] { false, false, false, false }; // R, G, B, V
		private Vector3 tempColorPickerHsv = Vector3.zero;
		private bool[] tempColorPickerUsed = new bool[] { false, false, false };
		private Texture2D dummyTexture; // Empty texture asset for drawing EditorGUI.DrawPreviewTexture().
#endif
//============================================================================//

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
						returnValue.x = Mathf.Lerp(randomMin.x, randomMax.x, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.y = Mathf.Lerp(randomMin.y, randomMax.y, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.z = Mathf.Lerp(randomMin.z, randomMax.z, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.w = Mathf.Lerp(randomMin.w, randomMax.w, (float)theRandom.NextDouble());
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

						returnValue.x = Mathf.Lerp(curveMinResult.x, curveMaxResult.x, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.y = Mathf.Lerp(curveMinResult.y, curveMaxResult.y, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.z = Mathf.Lerp(curveMinResult.z, curveMaxResult.z, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.w = Mathf.Lerp(curveMinResult.w, curveMaxResult.w, (float)theRandom.NextDouble());
					}
					else returnValue = Vector4.Lerp(curveMin.Evaluate(ownerBlueprint.ownerEmitter), curveMax.Evaluate(ownerBlueprint.ownerEmitter), (float)theRandom.NextDouble());
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						ColorProperty theReference = (ColorProperty)reference.property;
						returnValue = theReference.GetValue();
					}
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
						returnValue.x = Mathf.Lerp(randomMin.x, randomMax.x, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.y = Mathf.Lerp(randomMin.y, randomMax.y, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.z = Mathf.Lerp(randomMin.z, randomMax.z, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.w = Mathf.Lerp(randomMin.w, randomMax.w, (float)theRandom.NextDouble());
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

						returnValue.x = Mathf.Lerp(curveMinResult.x, curveMaxResult.x, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.y = Mathf.Lerp(curveMinResult.y, curveMaxResult.y, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.z = Mathf.Lerp(curveMinResult.z, curveMaxResult.z, (float)theRandom.NextDouble());
						//Random.seed++;
						returnValue.w = Mathf.Lerp(curveMinResult.w, curveMaxResult.w, (float)theRandom.NextDouble());
					}
					else returnValue = Vector4.Lerp(curveMin.Evaluate(ownerBlueprint.ownerEmitter, particleIndex), curveMax.Evaluate(ownerBlueprint.ownerEmitter, particleIndex), (float)theRandom.NextDouble());
					break;
				case eDataMode.Reference:
					if (reference != null)
					{
						ColorProperty theReference = (ColorProperty)reference.property;
						returnValue = theReference.GetValue(particleIndex);
					}
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
			usePerComponentRandom = false;
			Randomize();

			dummyTexture = new Texture2D(1, 1);
		}

		// COPY PROPERTY //
		//
		override public void CopyProperty(BaseProperty originalProperty, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperty(originalProperty, theOwnerBlueprint);

			ColorProperty originalColorProperty = originalProperty as ColorProperty;
			constant = originalColorProperty.constant;
			randomMin = originalColorProperty.randomMin;
			randomMax = originalColorProperty.randomMax;
			curve = ScriptableObject.CreateInstance<VectorCurve>();
			curve.Initialize(originalColorProperty.curve);
			curveMin = ScriptableObject.CreateInstance<VectorCurve>();
			curveMin.Initialize(originalColorProperty.curveMin);
			curveMax = ScriptableObject.CreateInstance<VectorCurve>();
			curveMax.Initialize(originalColorProperty.curveMax);
			usePerComponentRandom = originalColorProperty.usePerComponentRandom;
			Randomize();

			dummyTexture = new Texture2D(1, 1);
		}

//============================================================================//
#region GUI Utilities



		// NORMALIZE RGB //
		//
		Color NormalizeRGB(Color rgb)
		{
			float greatestComponent;
			float scaleFactor;

			greatestComponent = Mathf.Max(rgb.r, rgb.g, rgb.b);
			if (greatestComponent < 0 || greatestComponent > 1)
			{
				scaleFactor = 1 / greatestComponent;
				rgb.r *= scaleFactor;
				rgb.g *= scaleFactor;
				rgb.b *= scaleFactor;
				rgb.a = greatestComponent;
			}

			return rgb;
		}

#endregion
//============================================================================//
#region GUI

		// COLOR COMPONENT LDR //
		//
		float ColorComponentLDR(string label, float value)
		{
			float returnValue = value;

			if (returnValue > 1) returnValue = 1;

			GUILayout.BeginHorizontal();
			GUILayout.Label("", GUILayout.Width(21));
			EditorGUIUtility.LookLikeControls(50, 60);
			returnValue = EditorGUILayout.Slider(new GUIContent(label), returnValue, 0, 1, GUILayout.Width(160));
			EditorGUIUtility.LookLikeControls();
			GUILayout.EndHorizontal();

			if (returnValue < 0) returnValue = 0;
			return returnValue;
		}

		// COLOR COMPONENT HDR //
		//
		float ColorComponentHDR(string label, float value, ref bool hdrFlag)
		{
			float returnValue = value;

			GUILayout.BeginHorizontal();
			hdrFlag = GUILayout.Toggle(hdrFlag, "HDR", "hdrToggle");
			if (hdrFlag)
			{
				EditorGUIUtility.LookLikeControls(100, 60);
				returnValue = MyFloatField(label, returnValue, GUILayout.Width(160));
				EditorGUIUtility.LookLikeControls();

				if (returnValue < 0) returnValue = 0;
			}
			else
			{
				if (returnValue > 1) returnValue = 1;

				EditorGUIUtility.LookLikeControls(50, 60);
				returnValue = EditorGUILayout.Slider(new GUIContent(label), returnValue, 0, 1, GUILayout.Width(160));
				EditorGUIUtility.LookLikeControls();
			}
			GUILayout.EndHorizontal();

			return returnValue;
		}

		// SHOW PROPERTY //
		//
		override public void ShowProperty(ref BaseProperty selectedProperty, bool isReadOnly)
		{
			GUILayout.BeginVertical(GetStyle(selectedProperty));

			base.ShowProperty(ref selectedProperty, isReadOnly);

			Color originalColor = GUI.color;
			if (dummyTexture == null) dummyTexture = new Texture2D(1, 1);	// Since we don't serialize this var it will be null after scene load.

			switch (dataMode)
			{
				case eDataMode.Constant:
					Material colorPreviewMaterial = new Material(Resources.LoadAssetAtPath("Assets/Amps/Shaders/ColorPreview.shader", typeof(Shader)) as Shader);
					GUIStyle colorPreviewStyle = GUI.skin.GetStyle("colorPreview");
					Rect colorPreviewRect = new Rect(0, 0, colorPreviewStyle.fixedWidth, colorPreviewStyle.fixedHeight);

					GUILayout.BeginVertical();
					GUILayout.Box("", GUILayout.Height(colorPreviewRect.height), GUILayout.ExpandWidth(true)); // Making room for the preview.
					colorPreviewRect = GUILayoutUtility.GetLastRect();
					colorPreviewMaterial.SetVector("_Color", constant);
					EditorGUI.DrawPreviewTexture(colorPreviewRect, dummyTexture, colorPreviewMaterial);
					GUILayout.EndVertical();
					break;

				case eDataMode.RandomConstant:
					originalColor = GUI.color;
					GUILayout.BeginHorizontal();
					GUI.color = new Color(0.25f, 0.5f, 1f, 1f);
					GUILayout.Button("", "box", GUILayout.ExpandWidth(true));
					GUI.color = new Color(0.5f, 1f, 0.5f, 1f);
					GUILayout.Button("", "box", GUILayout.ExpandWidth(true));
					GUI.color = originalColor;
					GUILayout.EndHorizontal();
					break;

				case eDataMode.Curve:
					originalColor = GUI.color;

					GUILayout.BeginHorizontal();
					GUI.color = new Color(1f, 0.25f, 0.5f, 1f);
					GUILayout.Button("", "box", GUILayout.ExpandWidth(true));
					GUI.color = originalColor;
					GUILayout.EndHorizontal();
					break;

				case eDataMode.RandomCurve:
					originalColor = GUI.color;

					GUILayout.BeginHorizontal();
					GUI.color = new Color(0.25f, 0.25f, 1f, 1f);
					GUILayout.Button("", "box", GUILayout.ExpandWidth(true));
					GUI.color = new Color(0.25f, 1f, 0.25f, 1f);
					GUILayout.Button("", "box", GUILayout.ExpandWidth(true));
					GUI.color = originalColor;
					GUILayout.EndHorizontal();
					break;

				case eDataMode.Reference:
					CheckReferences();
					ShowReferenceControl();
					break;
			}

			if (coordSystemConversionMode != eCoordSystemConversionMode.NoConversion)
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
					ColorPicker(ref constant);
					break;
				case eDataMode.RandomConstant:
					ColorRandomPicker();
					break;
				case eDataMode.Curve:
					ColorCurvePicker();
					break;
				case eDataMode.RandomCurve:
					ColorRandomCurvePicker();
					break;
			}
		}

		// COLOR PICKER //
		//
		public void ColorPicker(ref Vector4 inputColor)
		{
			Vector3 hsv = Vector3.zero;
			Vector3 hsvLDR = Vector3.zero;
			Color rgb = Color.green;
			Vector4 matV = Vector4.one;
			Color colorConstant = new Vector4(inputColor.x, inputColor.y, inputColor.z, inputColor.w);

			shouldRepaintPicker = false;

			Material colorPickerSVMaterial = Resources.LoadAssetAtPath("Assets/Amps/Materials/ColorPicker_SV.mat", typeof(Material)) as Material;
			GUIStyle colorPickerSVStyle = GUI.skin.GetStyle("colorPickerSV");
			Rect colorPickerSVRect = new Rect(colorPickerSVStyle.contentOffset.x,
												colorPickerSVStyle.contentOffset.y,
												colorPickerSVStyle.fixedWidth,
												colorPickerSVStyle.fixedHeight);
			Material colorPickerHMaterial = Resources.LoadAssetAtPath("Assets/Amps/Materials/ColorPicker_H.mat", typeof(Material)) as Material;
			GUIStyle colorPickerHStyle = GUI.skin.GetStyle("colorPickerH");
			Rect colorPickerHRect = new Rect(colorPickerHStyle.contentOffset.x,
												colorPickerHStyle.contentOffset.y,
												colorPickerHStyle.fixedWidth,
												colorPickerHStyle.fixedHeight);
			Material colorPreviewMaterial = Resources.LoadAssetAtPath("Assets/Amps/Materials/ColorPreview.mat", typeof(Material)) as Material;
			GUIStyle colorPreviewStyle = GUI.skin.GetStyle("colorPreview");
			Rect colorPreviewRect = new Rect(colorPreviewStyle.contentOffset.x,
												colorPreviewStyle.contentOffset.y,
												colorPreviewStyle.fixedWidth,
												colorPreviewStyle.fixedHeight);

			GUILayout.BeginVertical("valuePickerBox");

			GUILayout.Box("", GUILayout.Height(colorPickerSVRect.height), GUILayout.ExpandWidth(true)); // Making room for the Saturation/Value picker.
			colorPickerSVRect = GUILayoutUtility.GetLastRect();
			EditorGUI.DrawPreviewTexture(colorPickerSVRect, dummyTexture, colorPickerSVMaterial);
			GUILayout.Space(8);

			GUILayout.Box("", GUILayout.Height(colorPickerHRect.height), GUILayout.ExpandWidth(true)); // Making room for the Hue picker.
			colorPickerHRect = GUILayoutUtility.GetLastRect();
			EditorGUI.DrawPreviewTexture(colorPickerHRect, dummyTexture, colorPickerHMaterial);
			GUILayout.Space(8);

			GUILayout.Box("", GUILayout.Height(colorPreviewRect.height), GUILayout.ExpandWidth(true)); // Making room for the preview.
			colorPreviewRect = GUILayoutUtility.GetLastRect();
			EditorGUI.DrawPreviewTexture(colorPreviewRect, dummyTexture, colorPreviewMaterial);
			GUILayout.Space(8);

			// RGB sliders.
			// Check which component has HDR value.
			if (colorPickerHdrToggles == null) colorPickerHdrToggles = new bool[] { false, false, false, false };
			if (colorConstant.r > 1) colorPickerHdrToggles[0] = true;
			if (colorConstant.g > 1) colorPickerHdrToggles[1] = true;
			if (colorConstant.b > 1) colorPickerHdrToggles[2] = true;

			// Show HDR capable controls for RGB.
			colorConstant.r = ColorComponentHDR("Red", colorConstant.r, ref colorPickerHdrToggles[0]);
			colorConstant.g = ColorComponentHDR("Green", colorConstant.g, ref colorPickerHdrToggles[1]);
			colorConstant.b = ColorComponentHDR("Blue", colorConstant.b, ref colorPickerHdrToggles[2]);
			GUILayout.Space(8);
			if ((colorConstant.r > 1 || colorConstant.g > 1 || colorConstant.b > 1) &&
				colorPickerHdrToggles[3] == false)
			{
				colorPickerHdrToggles[3] = true;	// If necessary we turn on HDR on V.
			}

			// Generating HSV values.
			hsv = AmpsHelpers.RGBtoHSV(colorConstant);
			hsvLDR = AmpsHelpers.RGBtoHSV(NormalizeRGB(colorConstant));

			//Debug.Log("1 hsvldr: " + hsvLDR.x + " , " + hsvLDR.y + " , " + hsvLDR.z);

			// HSV sliders.
			if (hsvLDR.y == 0)															// If the Hue slider is locked due to 0 Saturation...
			{
				if (tempColorPickerUsed[0] == false) tempColorPickerUsed[0] = true;
				tempColorPickerHsv.x = ColorComponentLDR("Hue", tempColorPickerHsv.x);	// ...then we start using a dummy variable for Hue.
			}
			else
			{
				if (tempColorPickerUsed[0] == true)
				{
					hsvLDR.x = tempColorPickerHsv.x;
					tempColorPickerUsed[0] = false;
				}
				hsvLDR.x = ColorComponentLDR("Hue", hsvLDR.x);
				tempColorPickerHsv.x = hsvLDR.x;
			}

			if (hsvLDR.z == 0)															// If the Saturation slider is locked due to 0 Value...
			{
				if (tempColorPickerUsed[1] == false) tempColorPickerUsed[1] = true;
				tempColorPickerHsv.y = ColorComponentLDR("Sat", tempColorPickerHsv.y);	// ...then we start using a dummy variable for Saturation.
			}
			else
			{
				if (tempColorPickerUsed[1] == true)
				{
					hsvLDR.y = tempColorPickerHsv.y;
					tempColorPickerUsed[1] = false;
				}
				hsvLDR.y = ColorComponentLDR("Sat", hsvLDR.y);
				tempColorPickerHsv.y = hsvLDR.y;
			}

			hsv.z = ColorComponentHDR("Value", hsv.z, ref colorPickerHdrToggles[3]);
			GUILayout.Space(8);
			colorConstant.a = ColorComponentLDR("Alpha", colorConstant.a);

			// Hue, Saturation and LDR Value picking.
			if (Event.current.type == EventType.mouseDrag || Event.current.type == EventType.mouseDown)
			{
				if (colorPickerSVRect.Contains(Event.current.mousePosition))
				{

					Vector2 mouseUV = (Event.current.mousePosition - new Vector2(colorPickerSVRect.x, colorPickerSVRect.y));
					mouseUV.x = mouseUV.x / colorPickerSVRect.width;
					mouseUV.y = mouseUV.y / colorPickerSVRect.height;
					hsvLDR.y = mouseUV.y;
					tempColorPickerUsed[1] = false;
					hsvLDR.z = mouseUV.x;
					hsv.z = hsvLDR.z;	// Picking a Value discards existing HDR Value.

					shouldRepaintPicker = true;
				}
				else if (colorPickerHRect.Contains(Event.current.mousePosition))
				{
					Vector2 mouseUV = (Event.current.mousePosition - new Vector2(colorPickerHRect.x, colorPickerHRect.y));
					mouseUV.x = mouseUV.x / colorPickerSVRect.width;
					if (tempColorPickerUsed[0])
					{
						tempColorPickerHsv.x = mouseUV.x;
					}
					else
					{
						hsvLDR.x = mouseUV.x;
					}

					shouldRepaintPicker = true;
				}
			}

			hsv.x = hsvLDR.x;	// Hue...
			hsv.y = hsvLDR.y;	// ...and Saturation will always be LDR.

			if (tempColorPickerUsed[0]) matV.x = tempColorPickerHsv.x;
			else matV.x = hsvLDR.x;
			if (tempColorPickerUsed[1]) matV.y = tempColorPickerHsv.y;
			else matV.y = hsvLDR.y;
			matV.z = hsvLDR.z;

			colorPickerSVMaterial.SetVector("_HSV", matV);
			colorPickerHMaterial.SetFloat("_Hue", matV.x);

			// Converting HSV back to RGB.
			rgb = AmpsHelpers.HSVtoRGB(hsv);
			colorConstant.r = rgb.r;
			colorConstant.g = rgb.g;
			colorConstant.b = rgb.b;

			//colorPickerAMaterial.SetFloat("_Alpha", matV.w);
			colorPreviewMaterial.SetVector("_Color", colorConstant);

			if (hsv.z > 1)
			{
				if (rgb.r > 1 && !colorPickerHdrToggles[0]) colorPickerHdrToggles[0] = true;
				if (rgb.g > 1 && !colorPickerHdrToggles[1]) colorPickerHdrToggles[1] = true;
				if (rgb.b > 1 && !colorPickerHdrToggles[2]) colorPickerHdrToggles[2] = true;
			}

			GUILayout.Space(8);
			GUILayout.EndVertical();

			inputColor = new Vector4(colorConstant.r, colorConstant.g, colorConstant.b, colorConstant.a);
		}

		// COLOR RANDOM PICKER //
		//
		public void ColorRandomPicker()
		{
		}

		// COLOR CURVE PICKER //
		//
		public void ColorCurvePicker()
		{
		}

		// COLOR RANDOM CURVE PICKER //
		//
		public void ColorRandomCurvePicker()
		{
		}

#endregion
#endif
	}
}