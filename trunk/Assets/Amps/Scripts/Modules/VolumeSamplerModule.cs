using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class VolumeSamplerModule : BaseSamplerModule
	{
		public GameObjectProperty sampledObject;			// The object containing the collider to be sampled.
		public VectorProperty insideValue;
		public VectorProperty outsideValue;
		private bool[] isInsideFlags;

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (ownerStack.isParticleStack) isInsideFlags = new bool[ownerBlueprint.ownerEmitter.particleIds.Length];
			else isInsideFlags = new bool[1];
		}

		// MANAGE SAMPLING //
		//
		// Does the actual sampling.
		override public void ManageSampling(int particleIndex)
		{
			Vector3 testedPosition = Vector3.zero;
			Collider theCollider;

			// If this is the first frame of a new particle then we skip sampling and wait for the next round
			// when the position stack will have been evaluated and updated from its default (0,0,0) value.
			if (particleIndex >= 0 && ownerBlueprint.ownerEmitter.particleTimes[particleIndex] < 0.1) return;	// HACK: There are misses when it's less than 0.1 for some reason.

			if ((particleIndex < 0 && ShouldSample()) || (particleIndex >= 0 && ShouldSample(particleIndex)))
			{
				if (particleIndex < 0)
				{
					testedPosition = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, AmpsHelpers.eCurveInputs.EmitterPosition);
				}
				else
				{
					testedPosition = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, AmpsHelpers.eCurveInputs.Position);
				}

				theCollider = sampledObject.GetValue().collider;
				if (theCollider != null)
				{
					Vector3 theRayVector = theCollider.transform.position - testedPosition;
					Ray theRay = new Ray(testedPosition, Vector3.Normalize(theRayVector));
					RaycastHit theRaycastHit = new RaycastHit();
					bool gotIntersection = theCollider.Raycast(theRay, out theRaycastHit, theRayVector.magnitude);
					//Debug.DrawLine(theCollider.transform.position, testedPosition);

					if (gotIntersection) isInsideFlags[particleIndex] = false;
					else isInsideFlags[particleIndex] = true;
				}

			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			if (sampledObject.GetValue() != null) ManageSampling(0);

			// We only do anything if there is a valid sample to work with.
			if (lastSampleTimes[0] != -1)
			{
#if UNITY_EDITOR
				exampleInput = new Vector4(input, input, input, input);
				ownerBlueprint.ownerEmitter.exampleInputParticleIndex = -1;
#endif

				if (isInsideFlags[0]) input = Blend(input, insideValue.GetValue(), weight.GetValue());
				else input = Blend(input, outsideValue.GetValue(), weight.GetValue());
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

					if (isInsideFlags[particleIndex])
					{
						input[particleIndex] = Blend(input[particleIndex], insideValue.GetValue(particleIndex), weight.GetValue(particleIndex));
					}
					else
					{
						input[particleIndex] = Blend(input[particleIndex], outsideValue.GetValue(particleIndex), weight.GetValue(particleIndex));
					}
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

				if (lastSampleTimes[particleIndex] != -1)
				{
#if UNITY_EDITOR
					if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex != -1)
					{
						exampleInput = input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex];
					}
#endif

					if (isInsideFlags[particleIndex]) input[particleIndex] = Blend(input[particleIndex], insideValue.GetValue(particleIndex), weight.GetValue(particleIndex));
					else input[particleIndex] = Blend(input[particleIndex], outsideValue.GetValue(particleIndex), weight.GetValue(particleIndex));
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
			type = "Volume sampler";
			SetDefaultName();

			sampledObject = ScriptableObject.CreateInstance<GameObjectProperty>();
			sampledObject.Initialize("Object", null, theOwnerBlueprint);
			sampledObject.SetDataModes(false, false, false, false, false, true);
			AddProperty(sampledObject, false);
			insideValue = ScriptableObject.CreateInstance<VectorProperty>();
			insideValue.Initialize("Value if inside", Vector4.one, theOwnerBlueprint);
			insideValue.SetConversionMode(theOwnerStack.stackFunction);
			insideValue.SetDefaultCoordSystem(theOwnerStack.stackFunction);
			AddProperty(insideValue, true);
			outsideValue = ScriptableObject.CreateInstance<VectorProperty>();
			outsideValue.Initialize("Value if outside", Vector4.one, theOwnerBlueprint);
			outsideValue.SetConversionMode(theOwnerStack.stackFunction);
			outsideValue.SetDefaultCoordSystem(theOwnerStack.stackFunction);
			AddProperty(outsideValue, true);
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
			insideValue.ShowProperty(ref selectedProperty, false);
			outsideValue.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
#endregion

#endif
	}
}