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
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	// This module provides a simple scalar constant.
	[System.Serializable]
	public class ScalarAccumulatorModule : BaseGenericModule
	{
		public BoolProperty useStackValue;		// If true then the current stack value will be used as per second change.
		public ScalarProperty changePerSecond;
		private float[] accumulatedValues;		// The accumulator floats for each particle.

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			InitializeNewParticles();

			if (useStackValue.GetValue()) accumulatedValues[0] += input * ownerBlueprint.ownerEmitter.deltaTime;
			else accumulatedValues[0] += changePerSecond.GetValue() * ownerBlueprint.ownerEmitter.deltaTime;

			input = Blend(input, accumulatedValues[0], weight.GetValue());
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			InitializeNewParticles();

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];

				if (useStackValue.GetValue()) accumulatedValues[particleIndex] += input[particleIndex] * ownerBlueprint.ownerEmitter.deltaTime;
				else accumulatedValues[particleIndex] += changePerSecond.GetValue(particleIndex) * ownerBlueprint.ownerEmitter.deltaTime;

				input[particleIndex] = Blend(input[particleIndex], accumulatedValues[particleIndex], weight.GetValue(particleIndex));
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			InitializeNewParticles();

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];

				// TODO: Allow vector component selection.
				// HACK: Hardwired use of vector.X .
				if (useStackValue.GetValue()) accumulatedValues[particleIndex] += input[particleIndex].x * ownerBlueprint.ownerEmitter.deltaTime;
				else accumulatedValues[particleIndex] += changePerSecond.GetValue(particleIndex) * ownerBlueprint.ownerEmitter.deltaTime;

				input[particleIndex] = Blend(input[particleIndex], accumulatedValues[particleIndex], weight.GetValue(particleIndex));
			}
		}

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			if (ownerStack.isParticleStack)
			{
				accumulatedValues = new float[ownerBlueprint.ownerEmitter.particleIds.Length];

				for (int i = 0; i < accumulatedValues.Length; i++)
				{
					accumulatedValues[i] = 0;
				}
			}
			else
			{
				accumulatedValues = new float[1];
				accumulatedValues[0] = 0;
			}
		}

		// INITIALIZE NEW PARTICLES //
		//
		override public void InitializeNewParticles()
		{
			if (ownerBlueprint.ownerEmitter.newParticleIndices.Count > 0)
			{
				for (int i = 0; i < ownerBlueprint.ownerEmitter.newParticleIndices.Count; i++)
				{
					accumulatedValues[ownerBlueprint.ownerEmitter.newParticleIndices[i]] = 0;
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

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Basic.ToString());
			type = "Scalar accumulator";
			SetDefaultName();

			useStackValue = ScriptableObject.CreateInstance<BoolProperty>();
			useStackValue.Initialize("Use stack value?", true, theOwnerBlueprint);
			AddProperty(useStackValue, true);

			changePerSecond = ScriptableObject.CreateInstance<ScalarProperty>();
			changePerSecond.Initialize("Change per second", 1f, theOwnerBlueprint);
			AddProperty(changePerSecond, true);
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
			if (useStackValue.GetValue() == false) changePerSecond.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

#endregion
#endif
	}
}