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
	public class BaseMultiFunctionModule : BaseModule
	{
		public BoolProperty modifySpawnRate;
		public BoolProperty modifyDeathCondition;
		public BoolProperty modifyDeathDuration;
		public BoolProperty modifyCustomScalar;
		public BoolProperty modifyCustomVector;
		public BoolProperty modifyAcceleration;
		public BoolProperty modifyVelocity;
		public BoolProperty modifyPosition;
		public BoolProperty modifyRotationRate;
		public BoolProperty modifyRotation;
		public BoolProperty modifyScale;
		public BoolProperty modifyColor;
		public BoolProperty modifyPivotOffset;

		// EVALUATE SPAWN RATE//
		//
		virtual public void Evaluate_SpawnRate()
		{
		}

		// EVALUATE DEATH CONDITION//
		//
		virtual public void Evaluate_DeathCondition()
		{
		}

		// EVALUATE DEATH DURATION//
		//
		virtual public void Evaluate_DeathDuration()
		{
		}

		// EVALUATE CUSTOM SCALAR//
		//
		virtual public void Evaluate_CustomScalar()
		{
		}

		// EVALUATE CUSTOM VECTOR//
		//
		virtual public void Evaluate_CustomVector()
		{
		}

		// EVALUATE ACCELERATION//
		//
		virtual public void Evaluate_Acceleration()
		{
		}

		// EVALUATE VELOCITY//
		//
		virtual public void Evaluate_Velocity()
		{
		}

		// EVALUATE POSITION//
		//
		virtual public void Evaluate_Position()
		{
		}

		// EVALUATE ROTATION RATE//
		//
		virtual public void Evaluate_RotationRate()
		{
		}

		// EVALUATE ROTATION //
		//
		virtual public void Evaluate_Rotation()
		{
		}

		// EVALUATE SCALE//
		//
		virtual public void Evaluate_Scale()
		{
		}

		// EVALUATE COLOR//
		//
		virtual public void Evaluate_Color()
		{
		}

		// EVALUATE PIVOT OFFSET//
		//
		virtual public void Evaluate_PivotOffset()
		{
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// This one shouldn't be called.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float input)
		{
			// This one shouldn't be called.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float[] input)
		{
			// This one shouldn't be called.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref Vector4[] input)
		{
			// This one shouldn't be called.
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			modifySpawnRate = ScriptableObject.CreateInstance<BoolProperty>();
			modifySpawnRate.Initialize("Modify Spawn rate?", false, theOwnerBlueprint);
			modifySpawnRate.allowDataModeReference = false;
			AddProperty(modifySpawnRate, false);
			modifyDeathCondition = ScriptableObject.CreateInstance<BoolProperty>();
			modifyDeathCondition.Initialize("Modify Death condition?", false, theOwnerBlueprint);
			modifyDeathCondition.allowDataModeReference = false;
			AddProperty(modifyDeathCondition, false);
			modifyDeathDuration = ScriptableObject.CreateInstance<BoolProperty>();
			modifyDeathDuration.Initialize("Modify Death duration?", false, theOwnerBlueprint);
			modifyDeathDuration.allowDataModeReference = false;
			AddProperty(modifyDeathDuration, false);
			modifyCustomScalar = ScriptableObject.CreateInstance<BoolProperty>();
			modifyCustomScalar.Initialize("Modify Custom scalar?", false, theOwnerBlueprint);
			modifyCustomScalar.allowDataModeReference = false;
			AddProperty(modifyCustomScalar, false);
			modifyCustomVector = ScriptableObject.CreateInstance<BoolProperty>();
			modifyCustomVector.Initialize("Modify Custom vector?", false, theOwnerBlueprint);
			modifyCustomVector.allowDataModeReference = false;
			AddProperty(modifyCustomVector, false);
			modifyAcceleration = ScriptableObject.CreateInstance<BoolProperty>();
			modifyAcceleration.Initialize("Modify Acceleration?", false, theOwnerBlueprint);
			modifyAcceleration.allowDataModeReference = false;
			AddProperty(modifyAcceleration, false);
			modifyVelocity = ScriptableObject.CreateInstance<BoolProperty>();
			modifyVelocity.Initialize("Modify Velocity?", false, theOwnerBlueprint);
			modifyVelocity.allowDataModeReference = false;
			AddProperty(modifyVelocity, false);
			modifyPosition = ScriptableObject.CreateInstance<BoolProperty>();
			modifyPosition.Initialize("Modify Position?", false, theOwnerBlueprint);
			modifyPosition.allowDataModeReference = false;
			AddProperty(modifyPosition, false);
			modifyRotationRate = ScriptableObject.CreateInstance<BoolProperty>();
			modifyRotationRate.Initialize("Modify Rotation rate?", false, theOwnerBlueprint);
			modifyRotationRate.allowDataModeReference = false;
			AddProperty(modifyRotationRate, false);
			modifyRotation = ScriptableObject.CreateInstance<BoolProperty>();
			modifyRotation.Initialize("Modify Rotation?", false, theOwnerBlueprint);
			modifyRotation.allowDataModeReference = false;
			AddProperty(modifyRotation, false);
			modifyScale = ScriptableObject.CreateInstance<BoolProperty>();
			modifyScale.Initialize("Modify Scale?", false, theOwnerBlueprint);
			modifyScale.allowDataModeReference = false;
			AddProperty(modifyScale, false);
			modifyColor = ScriptableObject.CreateInstance<BoolProperty>();
			modifyColor.Initialize("Modify Color?", false, theOwnerBlueprint);
			modifyColor.allowDataModeReference = false;
			AddProperty(modifyColor, false);
			modifyPivotOffset = ScriptableObject.CreateInstance<BoolProperty>();
			modifyPivotOffset.Initialize("Modify Pivot offset?", false, theOwnerBlueprint);
			modifyPivotOffset.allowDataModeReference = false;
			AddProperty(modifyPivotOffset, false);
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, theOwnerBlueprint);

		//    BaseMultiFunctionModule om = originalModule as BaseMultiFunctionModule;
		//    if (om != null)
		//    {
		//        modifySpawnRate.CopyProperty(om.modifySpawnRate, theOwnerBlueprint);
		//        modifyDeathCondition.CopyProperty(om.modifyDeathCondition, theOwnerBlueprint);
		//        modifyDeathDuration.CopyProperty(om.modifyDeathDuration, theOwnerBlueprint);
		//        modifyCustomScalar.CopyProperty(om.modifyCustomScalar, theOwnerBlueprint);
		//        modifyCustomVector.CopyProperty(om.modifyCustomVector, theOwnerBlueprint);
		//        modifyAcceleration.CopyProperty(om.modifyAcceleration, theOwnerBlueprint);
		//        modifyVelocity.CopyProperty(om.modifyVelocity, theOwnerBlueprint);
		//        modifyPosition.CopyProperty(om.modifyPosition, theOwnerBlueprint);
		//        modifyRotationRate.CopyProperty(om.modifyRotationRate, theOwnerBlueprint);
		//        modifyRotation.CopyProperty(om.modifyRotation, theOwnerBlueprint);
		//        modifyScale.CopyProperty(om.modifyScale, theOwnerBlueprint);
		//        modifyColor.CopyProperty(om.modifyColor, theOwnerBlueprint);
		//        modifyPivotOffset.CopyProperty(om.modifyPivotOffset, theOwnerBlueprint);
		//    }
		//}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			moduleName.ShowProperty(ref selectedProperty, false);
		}
#endregion
#endif
	}
}