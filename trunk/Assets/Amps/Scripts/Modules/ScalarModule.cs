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
	public class ScalarModule : BaseGenericModule
	{
		public ScalarProperty value;

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

		// INITALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Basic.ToString());
			type = "Scalar";
			SetDefaultName();

			value = ScriptableObject.CreateInstance<ScalarProperty>();
			value.Initialize("Value", 1f, theOwnerBlueprint);
			AddProperty(value, true);
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
			value.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

#endregion
#endif
	}
}