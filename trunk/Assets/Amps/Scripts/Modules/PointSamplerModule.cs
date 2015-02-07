using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	// A point is sampled in different coordinate systems.
	[System.Serializable]
	public class PointSamplerModule : BaseSamplerModule
	{
		public VectorProperty point;	// The basis for the sampling.
		private Vector4[] points;		// The actually generated data for each particle.

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (ownerStack.isParticleStack)
			{
				points = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
				for (int i = 0; i < points.Length; i++)
				{
					points[i] = Vector4.zero;
				}
			}
			else
			{
				points = new Vector4[1];
				points[0] = Vector4.zero;
			}
		}

		// MANAGE SAMPLING //
		//
		// Does the actual sampling.
		override public void ManageSampling(int particleIndex)
		{
			if (particleIndex < 0 && ShouldSample())
			{
				point.Randomize();
				points[0] = point.GetValue();
			}
			else if (particleIndex >= 0 && ShouldSample(particleIndex))
			{
				point.Randomize();
				points[particleIndex] = point.GetValue(particleIndex);
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

				input = Blend(input, points[0], weight.GetValue());
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

					input[particleIndex] = Blend(input[particleIndex], points[particleIndex], weight.GetValue(particleIndex));
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
					input[particleIndex] = Blend(input[particleIndex], points[particleIndex], weight.GetValue(particleIndex));
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

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Shapes.ToString());
			type = "Point sampler";
			SetDefaultName();

			point = ScriptableObject.CreateInstance<VectorProperty>();
			point.Initialize("Point", Vector4.zero, theOwnerBlueprint);
			point.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsPosition;
			implementsVisualization = true;
			AddProperty(point, true);
		}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;
			PropertyGroup("Shape: Point");
			point.ShowProperty(ref selectedProperty, false);

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

			if (point.dataMode == BaseProperty.eDataMode.Reference) actualPoint = (VectorProperty)point.reference.property;
			else actualPoint = point;

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