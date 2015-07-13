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

namespace Amps
{
	[System.Serializable]
	public class BaseSamplerModule : BaseGenericModule
	{
		public DropdownProperty samplerCondition;	// How to decide when to resample.
		public BoolProperty sampleOnSpawn;			// Should always resample on spawn?
		public ScalarProperty interval;				// Resample in what time intervals.
		public ScalarProperty intervalOffset;		// How much the intervals are offset.
		public ScalarProperty directValue;			// Manually control when to resample.
		public float[] lastSampleTimes;				// When the last sample was attempted or indeed taken.
		public int randomSeed;						// Module specific random seed.

		protected bool[] isValidSample;				// True if a valid sample was aquired.

		// E SAMPLER CONDITIONS //
		//
		public enum eSamplerConditions
		{
			OnCreation,
			ByTime,
			ByDirectValue,
			OnCollision
		}

#if UNITY_EDITOR
		// SAMPLER CONDITIONS DISPLAY DATA //
		//
		public static readonly string[] samplerConditionsDisplayData =
		{
			"On creation",
			"By time",
			"By direct value",
			"On collision"
		};
#endif
		// SOFT RESET //
		//
		override public void SoftReset()
		{
			if (ownerStack.isParticleStack)
			{
				lastSampleTimes = new float[ownerBlueprint.ownerEmitter.particleIds.Length];
				isValidSample = new bool[ownerBlueprint.ownerEmitter.particleIds.Length];

				for (int i = 0; i < lastSampleTimes.Length; i++)
				{
					lastSampleTimes[i] = -1;
					isValidSample[i] = false;
				}
			}
			else
			{
				lastSampleTimes = new float[1];
				lastSampleTimes[0] = -1;
				isValidSample = new bool[1];
				isValidSample[0] = false;
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
					lastSampleTimes[ownerBlueprint.ownerEmitter.newParticleIndices[i]] = -1;
					isValidSample[ownerBlueprint.ownerEmitter.newParticleIndices[i]] = false;
				}
			}
		}

		// SHOULD SAMPLE //
		//
		// Check sampling condition when particle data is NOT available.
		public bool ShouldSample()
		{
			return ShouldSample(0);
		}

		// SHOULD SAMPLE //
		//
		// Check sampling condition when particle data IS available.
		public bool ShouldSample(int particleIndex)
		{
			bool returnValue = false;
			float currentTime;

			// Get "currentTime" from the proper source.
			if (ownerStack.isParticleStack)
			{ currentTime = ownerBlueprint.ownerEmitter.particleTimes[particleIndex]; }
			else { currentTime = ownerBlueprint.ownerEmitter.emitterTime; }

			// Apply time offset if appropriate.
			if (samplerCondition.GetValue() == (int)eSamplerConditions.ByTime)
			{
				currentTime = currentTime - intervalOffset.GetValue();
				if (currentTime < 0) currentTime = 0;
			}
			else if (samplerCondition.GetValue() == (int)eSamplerConditions.OnCreation
					&& lastSampleTimes[particleIndex] < 0
					&& currentTime >= intervalOffset.GetValue())
			{
				lastSampleTimes[particleIndex] = currentTime;
				returnValue = true;
			}

			// We do a sample if we should on spawn and if the last sample timestamp is -1 i.e. no sample has been taken yet.
			if (sampleOnSpawn.GetValue() && lastSampleTimes[particleIndex] < 0)
			{
				lastSampleTimes[particleIndex] = currentTime;
				returnValue = true;
			}

			switch ((eSamplerConditions) samplerCondition.GetValue())
			{
				// AmpsHelpers.eSamplerConditions.OnCreation was handled above.

				case eSamplerConditions.ByTime:
					if (currentTime >= lastSampleTimes[particleIndex] + interval.GetValue())
					{
						returnValue = true;
						lastSampleTimes[particleIndex] = Mathf.Round(currentTime / interval.GetValue()) * interval.GetValue();
					}
					break;

				case eSamplerConditions.ByDirectValue:
					float theDirectValue = 0;

					if (ownerStack.isParticleStack) theDirectValue = directValue.GetValue(particleIndex);
					else theDirectValue = directValue.GetValue();

					if (theDirectValue >= 1)
					{
						returnValue = true;
						lastSampleTimes[particleIndex] = currentTime;
					}
					break;

				case eSamplerConditions.OnCollision:
					// True if collision occured on last update.
					// TODO: Implement collision.
					break;
			}

			return returnValue;
		}

		// MANAGE SAMPLING //
		//
		// Does the actual sampling.
		virtual public void ManageSampling(int particleIndex)
		{
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			samplerCondition = ScriptableObject.CreateInstance<DropdownProperty>();
			samplerCondition.Initialize("Sample condition", 0, theOwnerBlueprint);
			AddProperty(samplerCondition, true);
			sampleOnSpawn = ScriptableObject.CreateInstance<BoolProperty>();
			sampleOnSpawn.Initialize("Sample on spawn?", true, theOwnerBlueprint);
			AddProperty(sampleOnSpawn, true);
			interval = ScriptableObject.CreateInstance<ScalarProperty>();
			interval.Initialize("Interval", 1, theOwnerBlueprint);
			interval.SetDataModes(true, false, false, false, true, true);
			AddProperty(interval, true);
			intervalOffset = ScriptableObject.CreateInstance<ScalarProperty>();
			intervalOffset.Initialize("Initial delay", 0, theOwnerBlueprint);
			intervalOffset.SetDataModes(true, false, false, false, true, true);
			AddProperty(intervalOffset, true);
			directValue = ScriptableObject.CreateInstance<ScalarProperty>();
			directValue.Initialize("Direct value", 0, theOwnerBlueprint);
			directValue.SetDataModes(false, false, true, true, true, true);
			AddProperty(directValue, true);
			System.Random theRandom = new System.Random();
			randomSeed = theRandom.Next(1, 65536);
		}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;
			PropertyGroup("Sampler");

			int previousSamplerCondition = samplerCondition.GetValue();
			if (samplerCondition.displayData == null) samplerCondition.displayData = () => samplerConditionsDisplayData; // We have to do this here because delegates are not serialized.
			samplerCondition.ShowProperty(ref selectedProperty, false);
			// If the sampler condition has changed then we select the related dropdown property
			// to void having a property selected which might not be displayed bellow.
			if (samplerCondition.GetValue() != previousSamplerCondition) selectedProperty = samplerCondition;
			if (samplerCondition.GetValue() != (int)eSamplerConditions.OnCreation) sampleOnSpawn.ShowProperty(ref selectedProperty, false);

			// BUG: It is possible that the selected property is not shown which is visually confusing.
			switch (samplerCondition.GetValue())
			{
				case (int)eSamplerConditions.OnCreation:
					intervalOffset.ShowProperty(ref selectedProperty, false);
					break;
				case (int)eSamplerConditions.ByTime:
					interval.ShowProperty(ref selectedProperty, false);
					intervalOffset.ShowProperty(ref selectedProperty, false);
					break;
				case (int)eSamplerConditions.ByDirectValue:
					directValue.ShowProperty(ref selectedProperty, false);
					break;
				default:
					break;
			}

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
#endregion

#endif
	}
}