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
	public class TransformSamplerModule : BaseSamplerModule
	{
		public enum eTransformElements
		{
			Position,
			Rotation,
			Scale
		}

		public readonly string[] transformElementsDisplayData =
		{
			"Position",
			"Rotation",
			"Scale"
		};

		public GameObjectProperty sampledObject;			// The object containing the transform to be sampled.
		public DropdownProperty sampledTransformElement;	// Which part of the transform is sampled (pos/rot/scl).
		private Vector4[] vectors;		// The actually generated data for each particle.

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (ownerStack.isParticleStack)
			{
				vectors = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
				for (int i = 0; i < vectors.Length; i++)
				{
					vectors[i] = Vector4.zero;
				}
			}
			else
			{
				vectors = new Vector4[1];
				vectors[0] = Vector4.zero;
			}
		}

		// MANAGE SAMPLING //
		//
		// Does the actual sampling.
		override public void ManageSampling(int particleIndex)
		{
			Vector3 v = Vector3.zero;
			bool shouldIndeedSample = (particleIndex < 0 && ShouldSample()) || (particleIndex >= 0 && ShouldSample(particleIndex));

			// Leave if it's not time yet to sample.
			if (shouldIndeedSample == false) return;

			// INVALID SAMPLE: Referenced GameObject is not available.
			GameObject theGameObject = sampledObject.GetValue();
			if (theGameObject == null)
			{
				isValidSample[particleIndex] = false;
				return;
			}

			switch (sampledTransformElement.GetValue())
			{
				case (int)eTransformElements.Position:
					v = theGameObject.transform.position;		
					break;
				case (int)eTransformElements.Rotation:
					v = theGameObject.transform.rotation.eulerAngles;
					break;
				case (int)eTransformElements.Scale:
					v = theGameObject.transform.localScale;
					break;
			}
			vectors[particleIndex] = new Vector4(v.x, v.y, v.z, 0);

			isValidSample[particleIndex] = true;
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			if (sampledObject.GetValue() != null) ManageSampling(0);

			// We only do anything if there is a valid sample to work with.
			if (isValidSample[0])
			{
				#if UNITY_EDITOR
				exampleInput = new Vector4(input, input, input, input);
				ownerBlueprint.ownerEmitter.exampleInputParticleIndex = -1;
				#endif

				input = Blend(input, vectors[0], weight.GetValue());
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			InitializeNewParticles();	// TODO: Not executing this on each update could cause problems when toggling modules?

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				if (sampledObject.GetValue() != null) ManageSampling(particleIndex);

				if (isValidSample[particleIndex])
				{
					#if UNITY_EDITOR
					if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex != -1)
					{
						exampleInput = new Vector4(input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex],
													input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex],
													input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex],
													input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex]);
					}
					#endif

					input[particleIndex] = Blend(input[particleIndex], vectors[particleIndex], weight.GetValue(particleIndex));
					
				}
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
				if (sampledObject.GetValue() != null) ManageSampling(particleIndex);

				if (isValidSample[particleIndex])
				{
					#if UNITY_EDITOR
					if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex != -1)
					{
						exampleInput = input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex];
					}
					#endif

					input[particleIndex] = Blend(input[particleIndex], vectors[particleIndex], weight.GetValue(particleIndex));
				}
			}
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.OtherObjects.ToString());
			type = "Transform sampler";
			SetDefaultName();

			sampledObject = ScriptableObject.CreateInstance<GameObjectProperty>();
			sampledObject.Initialize("Object", null, theOwnerBlueprint);
			sampledObject.SetDataModes(false, false, false, false, false, true);
			AddProperty(sampledObject, false);
			sampledTransformElement = ScriptableObject.CreateInstance<DropdownProperty>();
			sampledTransformElement.Initialize("Transform", 0, theOwnerBlueprint);
			AddProperty(sampledTransformElement, true);
			implementsVisualization = false;
		}


//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;
			PropertyGroup("Object transform");
			sampledObject.ShowProperty(ref selectedProperty, false);
			if (sampledTransformElement.displayData == null) sampledTransformElement.displayData = () => transformElementsDisplayData; // We have to do this here because delegates are not serialized.
			sampledTransformElement.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
#endregion

#endif
	}
}