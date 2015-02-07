using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	// This module provides a simple scalar constant.
	[System.Serializable]
	public class ScalarConditionModule : BaseGenericModule
	{
		// E CONDITIONS //
		//
		public enum eConditions
		{
			Greater,
			Less,
			Equal,
			NotEqual
		}
		// CONDITIONS DISPLAY DATA //
		//
		public static readonly string[] conditionsDisplayData =
		{
			"is greater than",
			"is less than",
			"is equal to",
			"is not equal to"
		};

		public BoolProperty useStackValue;
		public ScalarProperty valueA;
		public DropdownProperty condition;
		public ScalarProperty valueB;
		public ScalarProperty valueFalse;
		public ScalarProperty valueTrue;

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			float output = 0;
			float finalValueA = useStackValue.GetValue() ? input : valueA.GetValue();
			switch (condition.GetValue())
			{
				case (int)eConditions.Greater:
					if (finalValueA > valueB.GetValue()) output = valueTrue.GetValue();
					else output = valueFalse.GetValue();
					break;
				case (int)eConditions.Less:
					if (finalValueA < valueB.GetValue()) output = valueTrue.GetValue();
					else output = valueFalse.GetValue();
					break;
				case (int)eConditions.Equal:
					if (finalValueA == valueB.GetValue()) output = valueTrue.GetValue();
					else output = valueFalse.GetValue();
					break;
				case (int)eConditions.NotEqual:
					if (finalValueA != valueB.GetValue()) output = valueTrue.GetValue();
					else output = valueFalse.GetValue();
					break;
			}

			input = Blend(input, output, weight.GetValue());
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

				float output = 0;
				float finalValueA = useStackValue.GetValue() ? input[particleIndex] : valueA.GetValue(particleIndex);

				switch (condition.GetValue())
				{
					case (int)eConditions.Greater:
						if (finalValueA > valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
					case (int)eConditions.Less:
						if (finalValueA < valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
					case (int)eConditions.Equal:
						if (finalValueA == valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
					case (int)eConditions.NotEqual:
						if (finalValueA != valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
				}

				input[particleIndex] = Blend(input[particleIndex], output, weight.GetValue(particleIndex));
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

				float output = 0;
				// TODO: Allow vector component selection.
				// HACK: Hardwired use of vector.X .
				float finalValueA = useStackValue.GetValue() ? input[particleIndex].x : valueA.GetValue(particleIndex);

				switch (condition.GetValue())
				{
					case (int)eConditions.Greater:
						if (finalValueA > valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
					case (int)eConditions.Less:
						if (finalValueA < valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
					case (int)eConditions.Equal:
						if (finalValueA == valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
					case (int)eConditions.NotEqual:
						if (finalValueA != valueB.GetValue(particleIndex)) output = valueTrue.GetValue(particleIndex);
						else output = valueFalse.GetValue(particleIndex);
						break;
				}

				input[particleIndex] = Blend(input[particleIndex], output, weight.GetValue(particleIndex));
			}
		}

//============================================================================//
#if UNITY_EDITOR

		// INITALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Basic.ToString());
			type = "Scalar condition";
			SetDefaultName();

			useStackValue = ScriptableObject.CreateInstance<BoolProperty>();
			useStackValue.Initialize("Use stack value?", true, theOwnerBlueprint);
			AddProperty(useStackValue, true);

			valueA = ScriptableObject.CreateInstance<ScalarProperty>();
			valueA.Initialize("Value A", 0f, theOwnerBlueprint);
			AddProperty(valueA, true);

			condition = ScriptableObject.CreateInstance<DropdownProperty>();
			condition.Initialize("Condition", 0, theOwnerBlueprint);
			condition.SetDataModes(true, false, false, false, false, false);
			AddProperty(condition, false);

			valueB = ScriptableObject.CreateInstance<ScalarProperty>();
			valueB.Initialize("Value B", 1f, theOwnerBlueprint);
			AddProperty(valueB, true);

			valueFalse = ScriptableObject.CreateInstance<ScalarProperty>();
			valueFalse.Initialize("Output for FALSE", 1f, theOwnerBlueprint);
			AddProperty(valueFalse, true);

			valueTrue = ScriptableObject.CreateInstance<ScalarProperty>();
			valueTrue.Initialize("Output for TRUE", 1f, theOwnerBlueprint);
			AddProperty(valueTrue, true);
		}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;
			PropertyGroup("");
			useStackValue.ShowProperty(ref selectedProperty, false);
			if (useStackValue.GetValue() == false) valueA.ShowProperty(ref selectedProperty, false);

			if (condition.displayData == null) condition.displayData = () => conditionsDisplayData; // We have to do this here because delegates are not serialized.
			condition.ShowProperty(ref selectedProperty, false);

			valueB.ShowProperty(ref selectedProperty, false);
			valueFalse.ShowProperty(ref selectedProperty, false);
			valueTrue.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

#endregion
#endif
	}
}