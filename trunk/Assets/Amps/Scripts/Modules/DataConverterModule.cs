using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	// This module provides a simple scalar constant.
	[System.Serializable]
	public class DataConverterModule : BaseGenericModule
	{
		public DropdownProperty conversionMode;

		// E CONVERSION MODES //
		//
		public enum eConversionModes
		{
			DirectionToRotation,
			RotationToDirection,
			HSVToRGB,
			RGBToHSV
		}

#if UNITY_EDITOR
		// CONVERSION MODE DISPLAY DATA //
		//
		public static readonly string[] conversionModeDisplayData =
		{
			"DirectionToRotation",
			"RotationToDirection",
			"HSVToRGB",
			"RGBToHSV"
		};
#endif

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			//switch (conversionMode.GetValue())
			//{
			//    case (int)eConversionModes.DirectionToRotation:
			//        break;
			//    case (int)eConversionModes.RotationToDirection:
			//        break;
			//    case (int)eConversionModes.HSVToRGB:
			//        break;
			//    case (int)eConversionModes.RGBToHSV:
			//        break;
			//}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			//foreach (Pool<ParticleMarker>.Node node in ownerEmitter.particleMarkers.ActiveNodes)
			//{
			//    switch (conversionMode.GetValue())
			//    {
			//        case (int)eConversionModes.DirectionToRotation:
			//            break;
			//        case (int)eConversionModes.RotationToDirection:
			//            break;
			//        case (int)eConversionModes.HSVToRGB:
			//            break;
			//        case (int)eConversionModes.RGBToHSV:
			//            break;
			//    }
			//}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				switch (conversionMode.GetValue())
				{
					case (int)eConversionModes.DirectionToRotation:
						// TODO: Choose upvector.
						Vector3 lookRotation = input[particleIndex];
						if (lookRotation == Vector3.zero) lookRotation.x = 0.00001f;
						input[particleIndex] = Quaternion.LookRotation(lookRotation, Vector3.up).eulerAngles;
						break;
					case (int)eConversionModes.RotationToDirection:
						input[particleIndex] = Quaternion.Euler(input[particleIndex]) * Vector3.forward;
						break;
					case (int)eConversionModes.HSVToRGB:
						input[particleIndex] = AmpsHelpers.HSVAtoRGBA(input[particleIndex]);
						break;
					case (int)eConversionModes.RGBToHSV:
						input[particleIndex] = AmpsHelpers.RGBAtoHSVA(input[particleIndex]);
						break;
				}

			}
		}

//============================================================================//
#if UNITY_EDITOR

		// INITALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Math.ToString());
			type = "Converter";
			SetDefaultName();

			conversionMode = ScriptableObject.CreateInstance<DropdownProperty>();
			conversionMode.Initialize("Conversion mode", 0, theOwnerBlueprint);
			AddProperty(conversionMode, true);
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, theOwnerBlueprint);

		//    DataConverterModule om = originalModule as DataConverterModule;
		//    if (om != null)
		//    {
		//        conversionMode.CopyProperty(om.conversionMode, theOwnerBlueprint);
		//    }
		//}

		//// REFERENCE PROPERTIES //
		////
		//override public void ReferenceProperties(BaseModule originalModule)
		//{
		//    base.ReferenceProperties(originalModule);

		//    DataConverterModule om = originalModule as DataConverterModule;
		//    ReferenceAProperty(conversionMode, om, om.conversionMode);
		//}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			//base.ShowProperties(ref shouldRepaint);
			BaseProperty previousSelection = selectedProperty;
			moduleName.ShowProperty(ref selectedProperty, false);
			PropertyGroup("");
			if (conversionMode.displayData == null) conversionMode.displayData = () => conversionModeDisplayData; // We have to do this here because delegates are not serialized.
			conversionMode.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

#endregion

#endif
	}
}