using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
#endif

namespace Amps
{
	[System.Serializable]
	public class BaseGenericModule : BaseModule
	{

		// E BLEND MODES //
		//
		public enum eBlendModes
		{
			Normal,
			Add,
			Multiply,
			SubtractValue,
			SubtractStack,
			DivideByValue,
			DivideByStack
		};

		#if UNITY_EDITOR
		// The possible submenus in the AddModule popup menu.

		// BLEND MODE DISPLAY DATA //
		//
		public readonly string[] blendModesDisplayData = 
		{
			"Normal",
			"Add",
			"Multiply",
			"Subtract Value",
			"Subtract Stack",
			"Divide by Value",
			"Divide by Stack"
		};

		public enum eCategories
		{
			None,
			Basic,
			Events,
			Math,
			Misc,
			OtherObjects,
			Shapes,
			ParentParticles,
			Vertex,
			_Debug
		};
		#endif

		public ScalarProperty weight;
		public DropdownProperty blendMode;

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// This one shouldn't be called.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float input)
		{
			// Implemented only as a stand-in.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float[] input)
		{
			// Implemented only as a stand-in.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref Vector4[] input)
		{
			// Implemented only as a stand-in.
		}

//============================================================================//
#region Blends

		// BLEND - FLOAT and FLOAT.
		public float Blend(float stackValue, float moduleValue, float weightValue)
		{
			float returnValue = 0;
			float divider = 1;

			if (weightValue == 0) return stackValue;

			switch ((eBlendModes)blendMode.constant)
			{
				case eBlendModes.Normal:
					returnValue = Mathf.Lerp(stackValue, moduleValue, weightValue);
					break;
				case eBlendModes.Add:
					returnValue = stackValue + Mathf.Lerp(0, moduleValue, weightValue);
					break;
				case eBlendModes.Multiply:
					returnValue = stackValue * Mathf.Lerp(1, moduleValue, weightValue);
					break;
				case eBlendModes.SubtractValue:
					returnValue = stackValue - Mathf.Lerp(0, moduleValue, weightValue);
					break;
				case eBlendModes.SubtractStack:
					returnValue = Mathf.Lerp(0, moduleValue, weightValue) - stackValue;
					break;
				case eBlendModes.DivideByValue:
					divider = Mathf.Lerp(1, moduleValue, weightValue);
					if (divider == 0) divider = float.Epsilon;
					returnValue = stackValue / divider;
					break;
				case eBlendModes.DivideByStack:
					divider = stackValue;
					if (divider == 0) divider = float.Epsilon;
					returnValue = Mathf.Lerp(1, moduleValue, weightValue) / divider;
					break;
			}

			return returnValue;
		}

		// BLEND - VECTOR and VECTOR.
		public Vector4 Blend(Vector4 stackValue, Vector4 moduleValue, float weightValue)
		{
			Vector4 returnValue = Vector4.zero;
			Vector4 divider = Vector4.one;
			Vector4 divident = Vector4.one;

			if (weightValue == 0) return stackValue;

			switch ((eBlendModes)blendMode.constant)
			{
				case eBlendModes.Normal:
					if (ownerStack.stackFunction == AmpsHelpers.eStackFunction.Rotation)	// HACK?
					{
						returnValue = Quaternion.Slerp(Quaternion.Euler(stackValue), Quaternion.Euler(moduleValue), weightValue).eulerAngles;
					}
					returnValue = Vector4.Lerp(stackValue, moduleValue, weightValue);
					break;
				case eBlendModes.Add:
					returnValue = stackValue + Vector4.Lerp(Vector4.zero, moduleValue, weightValue);
					break;
				case eBlendModes.Multiply:
					returnValue = Vector4.Scale(stackValue, Vector4.Lerp(Vector4.one, moduleValue, weightValue));
					break;

				case eBlendModes.SubtractValue:
					returnValue = stackValue - Vector4.Lerp(Vector4.zero, moduleValue, weightValue);
					break;
				case eBlendModes.SubtractStack:
					returnValue = Vector4.Lerp(Vector4.zero, moduleValue, weightValue) - stackValue;
					break;
				case eBlendModes.DivideByValue:
					divident = stackValue;
					divider = Vector4.Lerp(Vector4.one, moduleValue, weightValue);
					if (divider.x == 0) divider.x = float.Epsilon;
					if (divider.y == 0) divider.y = float.Epsilon;
					if (divider.z == 0) divider.z = float.Epsilon;
					if (divider.w == 0) divider.w = float.Epsilon;
					returnValue = new Vector4(divident.x / divider.x, divident.y / divider.y, divident.z / divider.z, divident.w / divider.w);
					break;
				case eBlendModes.DivideByStack:
					divident = Vector4.Lerp(Vector4.one, moduleValue, weightValue);
					divider = stackValue;
					if (divider.x == 0) divider.x = float.Epsilon;
					if (divider.y == 0) divider.y = float.Epsilon;
					if (divider.z == 0) divider.z = float.Epsilon;
					if (divider.w == 0) divider.w = float.Epsilon;
					returnValue = new Vector4(divident.x / divider.x, divident.y / divider.y, divident.z / divider.z, divident.w / divider.w);
					break;
			}

			return returnValue;
		}

		// BLEND - FLOAT and VECTOR.
		public float Blend(float stackValue, Vector4 moduleValue, float weightValue)
		{
			return Blend(stackValue, AmpsHelpers.ConvertVectorFloat(moduleValue), weightValue);
		}

		// BLEND - VECTOR and FLOAT.
		public Vector4 Blend(Vector4 stackValue, float moduleValue, float weightValue)
		{
			return Blend(stackValue, AmpsHelpers.ConvertFloatVector4(moduleValue), weightValue);
		}

#endregion

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			weight = ScriptableObject.CreateInstance<ScalarProperty>();
			weight.Initialize("Weight", 1, theOwnerBlueprint);
			weight.SetDataModes(true, true, true, true, false, true);
			AddProperty(weight, false);
			blendMode = ScriptableObject.CreateInstance<DropdownProperty>();
			blendMode.Initialize("Blend mode", 0, theOwnerBlueprint);
			blendMode.SetDataModes(true, false, false, false, false, false);
			AddProperty(blendMode, false);
		}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;

			if (ownerStack.stackFunction != AmpsHelpers.eStackFunction.Shared)
			{
				weight.ShowProperty(ref selectedProperty, false);
				if (blendMode.displayData == null) blendMode.displayData = () => blendModesDisplayData; // We have to do this here because delegates are not serialized.
				blendMode.ShowProperty(ref selectedProperty, false);
			}

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

#endregion

#endif
	}
}