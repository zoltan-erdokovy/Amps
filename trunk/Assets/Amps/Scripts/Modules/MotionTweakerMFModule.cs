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
	public class MotionTweakerMFModule : BaseMultiFunctionModule
	{
		public BoolProperty modifyMomentum;		// Aka acceleration accumulator.
		public BoolProperty modifyDisplacement;	// Aka velocity accumulator.
		public VectorProperty accelerationMultiplier;
		public VectorProperty momentumMultiplier;
		public VectorProperty velocityMultiplier;
		public VectorProperty displacementMultiplier;

		// EVALUATE ACCELERATION//
		//
		override public void Evaluate_Acceleration()
		{
			if (modifyAcceleration.GetValue() || modifyMomentum.GetValue())
			{
				Vector4 v;

				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					if (modifyAcceleration.GetValue())
					{
						v = accelerationMultiplier.GetValue(particleIndex);
						ownerBlueprint.accelerationStack.values[particleIndex].x *= v.x;
						ownerBlueprint.accelerationStack.values[particleIndex].y *= v.y;
						ownerBlueprint.accelerationStack.values[particleIndex].z *= v.z;
					}

					if (modifyMomentum.GetValue())
					{
						v = momentumMultiplier.GetValue(particleIndex);
						ownerBlueprint.ownerEmitter.accelerationAccumulators[particleIndex].x *= v.x;
						ownerBlueprint.ownerEmitter.accelerationAccumulators[particleIndex].y *= v.y;
						ownerBlueprint.ownerEmitter.accelerationAccumulators[particleIndex].z *= v.z;
					}
				}
			}
		}

		// EVALUATE VELOCITY//
		//
		override public void Evaluate_Velocity()
		{
			if (modifyVelocity.GetValue() || modifyDisplacement.GetValue())
			{
				Vector4 v;

				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					if (modifyVelocity.GetValue())
					{
						v = velocityMultiplier.GetValue(particleIndex);
						ownerBlueprint.velocityStack.values[particleIndex].x *= v.x;
						ownerBlueprint.velocityStack.values[particleIndex].y *= v.y;
						ownerBlueprint.velocityStack.values[particleIndex].z *= v.z;
					}

					if (modifyDisplacement.GetValue())
					{
						v = displacementMultiplier.GetValue(particleIndex);
						ownerBlueprint.ownerEmitter.velocityAccumulators[particleIndex].x *= v.x;
						ownerBlueprint.ownerEmitter.velocityAccumulators[particleIndex].y *= v.y;
						ownerBlueprint.ownerEmitter.velocityAccumulators[particleIndex].z *= v.z;
					}
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

			subMenuName = "";
			type = "Motion tweaker";
			SetDefaultName();

			modifyAcceleration.value = true;
			modifyVelocity.value = true;

			modifyMomentum = ScriptableObject.CreateInstance<BoolProperty>();
			modifyMomentum.Initialize("Modify momentum?", true, theOwnerBlueprint);
			modifyMomentum.allowDataModeReference = false;
			AddProperty(modifyMomentum, false);

			modifyDisplacement = ScriptableObject.CreateInstance<BoolProperty>();
			modifyDisplacement.Initialize("Modify displacement?", true, theOwnerBlueprint);
			modifyDisplacement.allowDataModeReference = false;
			AddProperty(modifyDisplacement, false);

			accelerationMultiplier = ScriptableObject.CreateInstance<VectorProperty>();
			accelerationMultiplier.Initialize("Acceleration multiplier", Vector4.one, theOwnerBlueprint);
			accelerationMultiplier.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.NoConversion;
			accelerationMultiplier.allowDataModeReference = false;
			accelerationMultiplier.hideW = true;
			AddProperty(accelerationMultiplier, false);

			momentumMultiplier = ScriptableObject.CreateInstance<VectorProperty>();
			momentumMultiplier.Initialize("Momentum multiplier", Vector4.one, theOwnerBlueprint);
			momentumMultiplier.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.NoConversion;
			momentumMultiplier.allowDataModeReference = false;
			momentumMultiplier.hideW = true;
			AddProperty(momentumMultiplier, false);

			velocityMultiplier = ScriptableObject.CreateInstance<VectorProperty>();
			velocityMultiplier.Initialize("Velocity multiplier", Vector4.one, theOwnerBlueprint);
			velocityMultiplier.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.NoConversion;
			velocityMultiplier.allowDataModeReference = false;
			velocityMultiplier.hideW = true;
			AddProperty(velocityMultiplier, false);

			displacementMultiplier = ScriptableObject.CreateInstance<VectorProperty>();
			displacementMultiplier.Initialize("Displacement multiplier", Vector4.one, theOwnerBlueprint);
			displacementMultiplier.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.NoConversion;
			displacementMultiplier.allowDataModeReference = false;
			displacementMultiplier.hideW = true;
			AddProperty(displacementMultiplier, false);
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, ownerBlueprint);

		//    MotionTweakerMFModule om = originalModule as MotionTweakerMFModule;
		//    if (om != null)
		//    {
		//        modifyMomentum.CopyProperty(om.modifyMomentum, theOwnerBlueprint);
		//        modifyDisplacement.CopyProperty(om.modifyDisplacement, theOwnerBlueprint);
		//        accelerationMultiplier.CopyProperty(om.accelerationMultiplier, theOwnerBlueprint);
		//        momentumMultiplier.CopyProperty(om.momentumMultiplier, theOwnerBlueprint);
		//        velocityMultiplier.CopyProperty(om.velocityMultiplier, theOwnerBlueprint);
		//        displacementMultiplier.CopyProperty(om.displacementMultiplier, theOwnerBlueprint);
		//    }
		//}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;

			PropertyGroup("Acceleration");
			modifyAcceleration.ShowProperty(ref selectedProperty, false);
			accelerationMultiplier.ShowProperty(ref selectedProperty, false);
			modifyMomentum.ShowProperty(ref selectedProperty, false);
			momentumMultiplier.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Velocity");
			modifyVelocity.ShowProperty(ref selectedProperty, false);
			velocityMultiplier.ShowProperty(ref selectedProperty, false);
			modifyDisplacement.ShowProperty(ref selectedProperty, false);
			displacementMultiplier.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
		#endregion

#endif
	}
}