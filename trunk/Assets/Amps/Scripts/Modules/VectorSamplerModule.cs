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
	// A point is sampled in different coordinate systems.
	[System.Serializable]
	public class VectorSamplerModule : BaseSamplerModule
	{
		public VectorProperty vector;	// The basis for the sampling.
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

		// DO SAMPLE //
		//
		// Does the actual sampling.
		override public void ManageSampling(int particleIndex)
		{
			if (particleIndex < 0 && ShouldSample())
			{
				vector.Randomize();
				vectors[0] = vector.GetValue();
			}
			else if (particleIndex >= 0 && ShouldSample(particleIndex))
			{
				vector.Randomize();
				vectors[particleIndex] = vector.GetValue(particleIndex);
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			ManageSampling(-1);

			// We only do anything if there is a valid sample to work with.
			if (lastSampleTimes[0] != -1)
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
			InitializeNewParticles();

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				ManageSampling(particleIndex);

				if (lastSampleTimes[particleIndex] != -1)
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
				ManageSampling(particleIndex);

				if (lastSampleTimes[particleIndex] != -1)
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

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Basic.ToString());
			type = "Vector sampler";
			SetDefaultName();

			vector = ScriptableObject.CreateInstance<VectorProperty>();
			vector.Initialize("Vector", Vector4.zero, theOwnerBlueprint);
			vector.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.NoConversion;
			AddProperty(vector, true);
		}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;
			vector.ShowProperty(ref selectedProperty, false);

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
			VectorProperty actualPoint;

			float timeSinceLastSample = ownerBlueprint.ownerEmitter.particleTimes[ownerBlueprint.ownerEmitter.exampleInputParticleIndex] - (lastSampleTimes[ownerBlueprint.ownerEmitter.exampleInputParticleIndex] + intervalOffset.GetValue());

			if (vector.dataMode == BaseProperty.eDataMode.Reference) actualPoint = (VectorProperty)vector.reference.property;
			else actualPoint = vector;

			switch (actualPoint.dataMode)
			{
				case VectorProperty.eDataMode.Constant:
					Handles.color = new Color(1f, 1f, 1f, 1f);
					if (actualPoint.coordSystemConversionMode != BaseProperty.eCoordSystemConversionMode.NoConversion)
					{
						v = actualPoint.ConvertCoordinateSystem(actualPoint.constant, ownerBlueprint.ownerEmitter.exampleInputParticleIndex);
					}
					else v = actualPoint.constant;
					AmpsHelpers.DrawPositionHandle(v, 0.125f, weight.GetValue(ownerBlueprint.ownerEmitter.exampleInputParticleIndex));
					AmpsHelpers.DrawSamplingIndicator(v, 0.0625f, timeSinceLastSample);
					break;

				case VectorProperty.eDataMode.RandomConstant:
					if (actualPoint.coordSystemConversionMode != BaseProperty.eCoordSystemConversionMode.NoConversion)
					{
						v = actualPoint.ConvertCoordinateSystem(actualPoint.randomMin, ownerBlueprint.ownerEmitter.exampleInputParticleIndex);
					}
					else v = actualPoint.randomMin;
					Handles.color = new Color(1f, 1f, 1f, 1f);
					AmpsHelpers.DrawPositionHandle(v, 0.0625f, weight.GetValue(ownerBlueprint.ownerEmitter.exampleInputParticleIndex));
					AmpsHelpers.DrawSamplingIndicator(v, 0.0625f, timeSinceLastSample);

					if (actualPoint.coordSystemConversionMode != BaseProperty.eCoordSystemConversionMode.NoConversion)
					{
						v = actualPoint.ConvertCoordinateSystem(actualPoint.randomMax, ownerBlueprint.ownerEmitter.exampleInputParticleIndex);
					}
					else v = actualPoint.randomMax;

					Handles.color = new Color(1f, 1f, 1f, 1f);
					AmpsHelpers.DrawPositionHandle(v, 0.125f, weight.GetValue(ownerBlueprint.ownerEmitter.exampleInputParticleIndex));
					AmpsHelpers.DrawSamplingIndicator(v, 0.0625f, timeSinceLastSample);
					break;

				case VectorProperty.eDataMode.Curve:
					// TODO: Curve vis.

					//Handles.color = new Color(1f, 1f, 1f, 1f);
					//v = actualPoint.convertCoordinateSystem(actualPoint.GetValue(ownerEmitter.exampleInputParticleIndex), ownerEmitter.exampleInputParticleIndex);
					//AmpsHelpers.DrawPositionHandle(v, 0.125f, weight.GetValue(ownerEmitter.exampleInputParticleIndex));
					//AmpsHelpers.DrawSamplingIndicator(v, 0.0625f, timeSinceLastSample);
					break;
				case VectorProperty.eDataMode.RandomCurve:
					// TODO: RandomCurve vis.
					break;
			}
		}
#endregion

#endif
	}
}