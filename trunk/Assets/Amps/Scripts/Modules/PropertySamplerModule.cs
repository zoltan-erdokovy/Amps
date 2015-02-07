using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	// A particle property is sampled.
	[System.Serializable]
	public class PropertySamplerModule : BaseSamplerModule
	{
		public DropdownProperty property;		// What property to sample.
		public BoolProperty sampleFromParent;	// If true then attempts to get data from the immediate parent GO.
		public BoolProperty modifyX;
		public BoolProperty modifyY;
		public BoolProperty modifyZ;
		public BoolProperty modifyW;

		private Vector4[] samples;				// The actually generated data for each particle.
		private int[] parentParticleIndices;	// The associated particle index in parent emitter.
		private bool wasParentChecked;
		private AmpsEmitter parentEmitter;

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (ownerStack.isParticleStack)
			{
				samples = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
				parentParticleIndices = new int[ownerBlueprint.ownerEmitter.particleIds.Length];
				for (int i = 0; i < samples.Length; i++)
				{
					samples[i] = Vector4.zero;
					parentParticleIndices[i] = -1;
				}
			}
			else
			{
				samples = new Vector4[1];
				samples[0] = Vector4.zero;
				parentParticleIndices = new int[1];
				parentParticleIndices[0] = -1;
			}

			wasParentChecked = false;
		}

		// INITIALIZE NEW PARTICLES //
		//
		override public void InitializeNewParticles()
		{
			base.InitializeNewParticles();

			if (ownerBlueprint.ownerEmitter.newParticleIndices.Count > 0)
			{
				for (int i = 0; i < ownerBlueprint.ownerEmitter.newParticleIndices.Count; i++)
				{
					parentParticleIndices[ownerBlueprint.ownerEmitter.newParticleIndices[i]] = -1;
				}
			}
		}

		// MANAGE SAMPLING //
		//
		// Does the actual sampling.
		override public void ManageSampling(int particleIndex)
		{
			int finalParticleIndex = (particleIndex < 0 ? 0 : particleIndex);
			AmpsHelpers.eCurveInputs wantedProperty = (AmpsHelpers.eCurveInputs)property.GetValue();
			bool isWantedPropertyParticleRelated = AmpsHelpers.isParticleRelatedInput(wantedProperty);
			bool shouldIndeedSample = (particleIndex < 0 && ShouldSample()) || (particleIndex >= 0 && ShouldSample(particleIndex));

			if (wasParentChecked == false)
			{
				if (ownerBlueprint.ownerEmitter.transform.parent != null)
				{
					parentEmitter = ownerBlueprint.ownerEmitter.transform.parent.GetComponent<AmpsEmitter>();
				}
				wasParentChecked = true;
			}

			if (sampleFromParent.GetValue())
			{
				// Leave if it's not time yet to sample.
				if (shouldIndeedSample == false) return;

				// INVALID SAMPLE: Parent doesn't exist or has no emitter component or no blueprint.
				if (parentEmitter == null ||
					parentEmitter.blueprint == null)
				{
					isValidSample[finalParticleIndex] = false;
					return;
				}

				// Try to get valid parent particle index.
				if (parentParticleIndices[finalParticleIndex] < 0)
				{
					parentParticleIndices[finalParticleIndex] = parentEmitter.GetRandomParticleIndex(ownerBlueprint.ownerEmitter.particleIds[finalParticleIndex]);
				}
				
				// TODO: Invalid sample case: particle id at current particle index is different, ie we did lose
				// original particle index as it got replaced by another.

				// INVALID SAMPLE: Invalid parent particle index.
				//
				// TODO: We might have an invalid parent particle index but we might not need it anymore since
				// we already got a valid sample from it before and we don't need a new one for the time being.
				if (parentParticleIndices[finalParticleIndex] < 0)
				{
					isValidSample[finalParticleIndex] = false;
					return;
				}
				
				// INVALID SAMPLE: Parent particle is dead.
				if (parentParticleIndices[finalParticleIndex] >= 0 &&
					parentEmitter.particleMarkers.IsActive(parentParticleIndices[finalParticleIndex]) == false)
				{
					parentParticleIndices[finalParticleIndex] = -1;
					isValidSample[finalParticleIndex] = false;
					return;
				}
				
				// INVALID SAMPLE: Asking for particle specific data but have no valid particle index.
				if (isWantedPropertyParticleRelated && parentParticleIndices[finalParticleIndex] < 0)
				{
					return;
				}

				samples[finalParticleIndex] = AmpsHelpers.GetSystemProperty(parentEmitter.blueprint,
																		parentParticleIndices[finalParticleIndex],
																		wantedProperty);
				isValidSample[finalParticleIndex] = true;
			}
			else
			{
				// Leave if it's not time yet to sample.
				if (shouldIndeedSample == false) return;

				// INVALID SAMPLE: Asking for particle specific data but have no valid particle index.
				if (isWantedPropertyParticleRelated && finalParticleIndex < 0)
				{
					return;
				}

				samples[finalParticleIndex] = AmpsHelpers.GetSystemProperty(ownerBlueprint,
																		finalParticleIndex,
																		wantedProperty);
				isValidSample[finalParticleIndex] = true;
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			float f;

			ManageSampling(-1);
			// We only do anything if there is a valid sample to work with.
			if (isValidSample[0] && lastSampleTimes[0] != -1)
			{
				f = Blend(input, samples[0], weight.GetValue());
				if (modifyX.GetValue()) input = f;
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			float f;

			InitializeNewParticles();

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				ManageSampling(particleIndex);

				if (isValidSample[particleIndex] && lastSampleTimes[particleIndex] != -1)
				{
					f = Blend(input[particleIndex], samples[particleIndex], weight.GetValue(particleIndex));
					if (modifyX.GetValue()) input[particleIndex] = f;
				}
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			Vector4 v;

			InitializeNewParticles();

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				ManageSampling(particleIndex);

				if (isValidSample[particleIndex] && lastSampleTimes[particleIndex] != -1)
				{
					v = Blend(input[particleIndex], samples[particleIndex], weight.GetValue(particleIndex));
					if (modifyX.GetValue()) input[particleIndex].x = v.x;
					if (modifyY.GetValue()) input[particleIndex].y = v.y;
					if (modifyZ.GetValue()) input[particleIndex].z = v.z;
					if (modifyW.GetValue()) input[particleIndex].w = v.w;
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
			type = "Property sampler";
			SetDefaultName();

			property = ScriptableObject.CreateInstance<DropdownProperty>();
			property.Initialize("Property", 0, theOwnerBlueprint);
			AddProperty(property, true);
			sampleFromParent = ScriptableObject.CreateInstance<BoolProperty>();
			sampleFromParent.Initialize("Sample parent?", false, property.ownerBlueprint);
			AddProperty(sampleFromParent, true);
			modifyX = ScriptableObject.CreateInstance<BoolProperty>();
			modifyX.Initialize("Modify X?", true, theOwnerBlueprint);
			AddProperty(modifyX, true);
			modifyY = ScriptableObject.CreateInstance<BoolProperty>();
			modifyY.Initialize("Modify Y?", true, theOwnerBlueprint);
			AddProperty(modifyY, true);
			modifyZ = ScriptableObject.CreateInstance<BoolProperty>();
			modifyZ.Initialize("Modify Z?", true, theOwnerBlueprint);
			AddProperty(modifyZ, true);
			modifyW = ScriptableObject.CreateInstance<BoolProperty>();
			modifyW.Initialize("Modify W?", true, theOwnerBlueprint);
			AddProperty(modifyW, true);
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
			if (property.displayData == null) property.displayData = () => AmpsHelpers.curveInputDisplayData; // We have to do this here because delegates are not serialized.
			property.ShowProperty(ref selectedProperty, false);
			sampleFromParent.ShowProperty(ref selectedProperty, false);
			modifyX.ShowProperty(ref selectedProperty, false);
			modifyY.ShowProperty(ref selectedProperty, false);
			modifyZ.ShowProperty(ref selectedProperty, false);
			modifyW.ShowProperty(ref selectedProperty, false);
			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

#endregion
#endif
	}
}