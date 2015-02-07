using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	// This module provides a simple vector constant.
	[System.Serializable]
	public class ColorModule : BaseGenericModule
	{
		public ColorProperty value;

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			input = Blend(input, value.GetValue(), weight.GetValue());
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				input[particleIndex] = Blend(input[particleIndex], value.GetValue(particleIndex), weight.GetValue(particleIndex));
			}
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
				input[particleIndex] = Blend(input[particleIndex], value.GetValue(particleIndex), weight.GetValue(particleIndex));
			}
		}

		//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Basic.ToString());
			type = "Color";
			SetDefaultName();

			value = ScriptableObject.CreateInstance<ColorProperty>();
			value.Initialize("Color", Vector4.one, theOwnerBlueprint);
			AddProperty(value, true);
			implementsVisualization = false;
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, theOwnerBlueprint);

		//    ColorModule om = originalModule as ColorModule;
		//    if (om != null)
		//    {
		//        value.CopyProperty(om.value, theOwnerBlueprint);
		//    }
		//}

		//// REFERENCE PROPERTIES //
		////
		//override public void ReferenceProperties(BaseModule originalModule)
		//{
		//    base.ReferenceProperties(originalModule);

		//    ColorModule om = originalModule as ColorModule;
		//    ReferenceAProperty(value, om, om.value);
		//}

		//============================================================================//
		#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;
			PropertyGroup("");
			value.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

		// SHOW VISUALIZATION //
		//
		override public void ShowVisualization()
		{
			if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex != -1)
			{
				if (ownerStack.stackFunction == AmpsHelpers.eStackFunction.Position)
				{
					ShowPositionVisualization();
				}
				else { ShowGenericVisualization(); }
			}
		}

		// SHOW GENERIC VISUALIZATION //
		//
		private void ShowGenericVisualization()
		{
			// TODO: Generic, text based data visualization for non position stacks.
		}

		// SHOW POSITION VISUALIZATION //
		//
		private void ShowPositionVisualization()
		{
			Vector4 v;
			BaseProperty.eDataMode dm;

			if (value.dataMode == BaseProperty.eDataMode.Reference) dm = value.reference.property.dataMode;
			else dm = value.dataMode;

			switch (dm)
			{
				case ColorProperty.eDataMode.Constant:
					Handles.color = new Color(1f, 1f, 1f, 1f);
					v = value.GetValue(ownerBlueprint.ownerEmitter.exampleInputParticleIndex);
					AmpsHelpers.DrawPositionHandle(v, 0.125f, weight.GetValue(ownerBlueprint.ownerEmitter.exampleInputParticleIndex));
					break;

				case ColorProperty.eDataMode.RandomConstant:
					if (value.coordSystemConversionMode != BaseProperty.eCoordSystemConversionMode.NoConversion)
					{
						v = value.ConvertCoordinateSystem(value.randomMin, ownerBlueprint.ownerEmitter.exampleInputParticleIndex);
					}
					else v = value.randomMin;
					Handles.color = new Color(1f, 1f, 1f, 1f);
					AmpsHelpers.DrawPositionHandle(v, 0.0625f, weight.GetValue(ownerBlueprint.ownerEmitter.exampleInputParticleIndex));

					v = value.ConvertCoordinateSystem(value.randomMax, ownerBlueprint.ownerEmitter.exampleInputParticleIndex);
					Handles.color = new Color(1f, 1f, 1f, 1f);
					AmpsHelpers.DrawPositionHandle(v, 0.125f, weight.GetValue(ownerBlueprint.ownerEmitter.exampleInputParticleIndex));
					break;

				case ColorProperty.eDataMode.Curve:
					//Handles.color = Color.white;
					//v = value.GetValue(ownerEmitter.exampleInputParticleIndex);
					//AmpsHelpers.DrawCrossHandle(v, 0.125f);
					break;
				case ColorProperty.eDataMode.RandomCurve:
					// TODO: RandomCurve vis.
					break;
			}
		}
		#endregion

#endif
	}
}