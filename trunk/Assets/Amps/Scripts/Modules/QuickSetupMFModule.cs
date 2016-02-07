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
	// A quick way to set up basic emitter properties.
	[System.Serializable]
	public class QuickSetupMFModule : BaseMultiFunctionModule
	{
		public ScalarProperty spawnRate;
		public ScalarProperty deathCondition;
		public VectorProperty acceleration;
		public VectorProperty velocity;
		public VectorProperty position;
		public VectorProperty rotationRate;
		public VectorProperty rotation;
		public VectorProperty scale;
		public ColorProperty color;
		public VectorProperty pivotOffset;

		// EVALUATE SPAWN RATE//
		//
		override public void Evaluate_SpawnRate()
		{
			if (modifySpawnRate.GetValue())
			{
				ownerBlueprint.spawnRateStack.value = spawnRate.GetValue();
			}
		}

		// EVALUATE DEATH CONDITION//
		//
		override public void Evaluate_DeathCondition()
		{
			if (modifyDeathCondition.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.deathConditionStack.values[particleIndex] = deathCondition.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE ACCELERATION//
		//
		override public void Evaluate_Acceleration()
		{
			if (modifyAcceleration.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.accelerationStack.values[particleIndex] = acceleration.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE VELOCITY//
		//
		override public void Evaluate_Velocity()
		{
			if (modifyVelocity.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.velocityStack.values[particleIndex] = velocity.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE POSITION//
		//
		override public void Evaluate_Position()
		{
			if (modifyPosition.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.positionStack.values[particleIndex] = position.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE ROTATION RATE//
		//
		override public void Evaluate_RotationRate()
		{
			if (modifyRotationRate.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.rotationRateStack.values[particleIndex] = rotationRate.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE ROTATION //
		//
		override public void Evaluate_Rotation()
		{
			if (modifyRotation.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.rotationStack.values[particleIndex] = rotation.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE SCALE//
		//
		override public void Evaluate_Scale()
		{
			if (modifyScale.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.scaleStack.values[particleIndex] = scale.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE COLOR//
		//
		override public void Evaluate_Color()
		{
			if (modifyColor.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.colorStack.values[particleIndex] = color.GetValue(particleIndex);
				}
			}
		}

		// EVALUATE PIVOT OFFSET//
		//
		override public void Evaluate_PivotOffset()
		{
			if (modifyPivotOffset.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					ownerBlueprint.pivotOffsetStack.values[particleIndex] = pivotOffset.GetValue(particleIndex);
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
			type = "Quick setup";
			SetDefaultName();

			modifySpawnRate.value = true;
			modifyDeathCondition.value = true;
			modifyAcceleration.value = true;
			modifyVelocity.value = true;
			modifyPosition.value = true;
			modifyRotationRate.value = true;
			modifyRotation.value = true;
			modifyScale.value = true;
			modifyColor.value = true;
			modifyPivotOffset.value = true;

			spawnRate = ScriptableObject.CreateInstance<ScalarProperty>();
			spawnRate.Initialize("Spawn rate", 5, theOwnerBlueprint);
			spawnRate.allowDataModeReference = false;
			AddProperty(spawnRate, false);

			deathCondition = ScriptableObject.CreateInstance<ScalarProperty>();
			deathCondition.Initialize("Death condition", 0, theOwnerBlueprint);
			deathCondition.allowDataModeReference = false;
			deathCondition.dataMode = BaseProperty.eDataMode.Curve;
			deathCondition.curve.curveInput = AmpsHelpers.eCurveInputs.ParticleTime;
			deathCondition.curve.inputRangeMin = 0;
			deathCondition.curve.inputRangeMax = 2;	// Default lifetime.
			deathCondition.curve.curve.AddKey(0, 0);
			deathCondition.curve.curve.AddKey(1, 1);
			deathCondition.curve.outputRangeMin = 0;
			deathCondition.curve.outputRangeMax = 1;
			AddProperty(deathCondition, false);

			acceleration = ScriptableObject.CreateInstance<VectorProperty>();
			acceleration.Initialize("Acceleration", new Vector4(0, -0.1f, 0, 0), theOwnerBlueprint);
			acceleration.allowDataModeReference = false;
			acceleration.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsVelocity;
			acceleration.usePerComponentRandom = true;
			acceleration.hideW = true;
			AddProperty(acceleration, false);

			velocity = ScriptableObject.CreateInstance<VectorProperty>();
			velocity.Initialize("Velocity min", new Vector4(0, 0, 0, 0), theOwnerBlueprint);
			velocity.allowDataModeReference = false;
			velocity.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsVelocity;
			velocity.hideW = true;
			velocity.usePerComponentRandom = true;
			velocity.dataMode = BaseProperty.eDataMode.RandomConstant;
			velocity.randomMin = new Vector4(-1, 1, -1, 0);
			velocity.randomMax = new Vector4(1, 2, 1, 0);
			AddProperty(velocity, false);

			position = ScriptableObject.CreateInstance<VectorProperty>();
			position.Initialize("Position", new Vector4(0, 0, 0, 0), theOwnerBlueprint);
			position.allowDataModeReference = false;
			position.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsPosition;
			position.coordSystem = AmpsHelpers.eCoordSystems.Emitter;
			position.hideW = true;
			position.usePerComponentRandom = true;
			AddProperty(position, false);

			rotationRate = ScriptableObject.CreateInstance<VectorProperty>();
			rotationRate.Initialize("Rotation rate", new Vector4(0, 0, 0, 0), theOwnerBlueprint);
			rotationRate.allowDataModeReference = false;
			rotationRate.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsVelocity;
			rotationRate.hideW = true;
			rotationRate.usePerComponentRandom = true;
			AddProperty(rotationRate, false);

			rotation = ScriptableObject.CreateInstance<VectorProperty>();
			rotation.Initialize("Rotation", new Vector4(0, 0, 0, 0), theOwnerBlueprint);
			rotation.allowDataModeReference = false;
			rotation.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsRotation;
			rotation.coordSystem = AmpsHelpers.eCoordSystems.Emitter;
			rotation.hideW = true;
			rotation.usePerComponentRandom = true;
			AddProperty(rotation, false);

			scale = ScriptableObject.CreateInstance<VectorProperty>();
			scale.Initialize("Scale", new Vector4(0.2f, 0.2f, 0.2f, 0), theOwnerBlueprint);
			scale.allowDataModeReference = false;
			scale.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsScale;
			scale.coordSystem = AmpsHelpers.eCoordSystems.Emitter;
			scale.usePerComponentRandom = false;
			scale.hideW = true;
			AddProperty(scale, false);

			color = ScriptableObject.CreateInstance<ColorProperty>();
			color.Initialize("Color", new Vector4(1, 0.5f, 0.25f, 1), theOwnerBlueprint);
			color.allowDataModeReference = false;
			color.usePerComponentRandom = false;
			AddProperty(color, false);

			pivotOffset = ScriptableObject.CreateInstance<VectorProperty>();
			pivotOffset.Initialize("Pivot offset", new Vector4(0, 0, 0, 0), theOwnerBlueprint);
			pivotOffset.allowDataModeReference = false;
			pivotOffset.coordSystemConversionMode = BaseProperty.eCoordSystemConversionMode.AsPosition;
			pivotOffset.hideW = true;
			pivotOffset.usePerComponentRandom = true;
			AddProperty(pivotOffset, false);
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, theOwnerBlueprint);

		//    QuickSetupMFModule om = originalModule as QuickSetupMFModule;
		//    if (om != null)
		//    {
		//        spawnRate.CopyProperty(om.spawnRate, theOwnerBlueprint);
		//        deathCondition.CopyProperty(om.deathCondition, theOwnerBlueprint);
		//        acceleration.CopyProperty(om.acceleration, theOwnerBlueprint);
		//        velocity.CopyProperty(om.velocity, theOwnerBlueprint);
		//        position.CopyProperty(om.position, theOwnerBlueprint);
		//        rotationRate.CopyProperty(om.rotationRate, theOwnerBlueprint);
		//        rotation.CopyProperty(om.rotation, theOwnerBlueprint);
		//        scale.CopyProperty(om.scale, theOwnerBlueprint);
		//        color.CopyProperty(om.color, theOwnerBlueprint);
		//        pivotOffset.CopyProperty(om.pivotOffset, theOwnerBlueprint);
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
			PropertyGroup("Spawn rate");
			modifySpawnRate.ShowProperty(ref selectedProperty, false);
			spawnRate.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Life");
			modifyDeathCondition.ShowProperty(ref selectedProperty, false);
			deathCondition.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Position");
			modifyPosition.ShowProperty(ref selectedProperty, false);
			position.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Acceleration");
			modifyAcceleration.ShowProperty(ref selectedProperty, false);
			acceleration.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Velocity");
			modifyVelocity.ShowProperty(ref selectedProperty, false);
			velocity.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Rotation");
			modifyRotation.ShowProperty(ref selectedProperty, false);
			rotation.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Rotation rate");
			modifyRotationRate.ShowProperty(ref selectedProperty, false);
			rotationRate.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Scale");
			modifyScale.ShowProperty(ref selectedProperty, false);
			scale.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Color");
			modifyColor.ShowProperty(ref selectedProperty, false);
			color.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Pivot offset");
			modifyPivotOffset.ShowProperty(ref selectedProperty, false);
			pivotOffset.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
#endregion

#endif
	}
}