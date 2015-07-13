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
using System.Collections.Generic;

namespace Amps
{
	[System.Serializable]
	public class AmpsBlueprint : ScriptableObject
	{
		public VoidStack emitterStack;
		public VoidStack renderStack;
		public VoidStack sharedStack;
		public FloatStack spawnRateStack;
		public FloatArrayStack deathConditionStack;
		public FloatArrayStack deathDurationStack;
		public FloatArrayStack customScalarStack;
		public VectorArrayStack customVectorStack;
		public VectorArrayStack accelerationStack;
		public VectorArrayStack velocityStack;
		public VectorArrayStack positionStack;
		public VectorArrayStack rotationRateStack;
		public VectorArrayStack rotationStack;
		public VectorArrayStack scaleStack;
		public VectorArrayStack colorStack;
		public VectorArrayStack pivotOffsetStack;
		public MultiFunctionStack multiFunctionStack;

		public List<BaseStack> stacks;	// Contains all stacks;

		[System.NonSerialized]
		public AmpsEmitter ownerEmitter;	// The AmpsEmitter component using this instance of the asset.

#if UNITY_EDITOR
		// INITIALIZE //
		//
		public void Initialize()
		{
			if (stacks == null) stacks = new List<BaseStack>();
			else stacks.Clear();

			// First of all we fully set up the emitter's core parts.
			emitterStack = new VoidStack();
			emitterStack.Initialize(this, AmpsHelpers.eStackFunction.Emitter, typeof(BaseEmitterModule).ToString());
			stacks.Add(emitterStack);
			if (emitterStack.modules.Count == 0)
			{
				BaseEmitterModule em = ScriptableObject.CreateInstance<BaseEmitterModule>();
				em.Initialize(emitterStack, this);
				em.playOnAwake.value = true;
				em.isLooping.value = true;
				em.maxParticles.constant = 10;
				em.moduleName.value = "Basic emitter properties";
				emitterStack.modules.Add(em);
				emitterStack.selectedModule = em;
			}

			renderStack = new VoidStack();
			renderStack.Initialize(this, AmpsHelpers.eStackFunction.Render, typeof(BaseRenderModule).ToString());
			stacks.Add(renderStack);
			sharedStack = new VoidStack();
			sharedStack.Initialize(this, AmpsHelpers.eStackFunction.Shared, typeof(BaseGenericModule).ToString());
			stacks.Add(sharedStack);
			spawnRateStack = new FloatStack();
			spawnRateStack.Initialize(this, AmpsHelpers.eStackFunction.SpawnRate, typeof(BaseGenericModule).ToString());
			stacks.Add(spawnRateStack);
			deathConditionStack = new FloatArrayStack();
			deathConditionStack.Initialize(this, AmpsHelpers.eStackFunction.DeathCondition, typeof(BaseGenericModule).ToString());
			stacks.Add(deathConditionStack);
			deathDurationStack = new FloatArrayStack();
			deathDurationStack.Initialize(this, AmpsHelpers.eStackFunction.DeathDuration, typeof(BaseGenericModule).ToString());
			stacks.Add(deathDurationStack);
			customScalarStack = new FloatArrayStack();
			customScalarStack.Initialize(this, AmpsHelpers.eStackFunction.CustomScalar, typeof(BaseGenericModule).ToString());
			stacks.Add(customScalarStack);
			customVectorStack = new VectorArrayStack();
			customVectorStack.Initialize(this, AmpsHelpers.eStackFunction.CustomVector, typeof(BaseGenericModule).ToString());
			stacks.Add(customVectorStack);
			accelerationStack = new VectorArrayStack();
			accelerationStack.Initialize(this, AmpsHelpers.eStackFunction.Acceleration, typeof(BaseGenericModule).ToString());
			accelerationStack.EnableOldValues();
			stacks.Add(accelerationStack);
			velocityStack = new VectorArrayStack();
			velocityStack.Initialize(this, AmpsHelpers.eStackFunction.Velocity, typeof(BaseGenericModule).ToString());
			velocityStack.EnableOldValues();
			stacks.Add(velocityStack);
			positionStack = new VectorArrayStack();
			positionStack.Initialize(this, AmpsHelpers.eStackFunction.Position, typeof(BaseGenericModule).ToString());
			positionStack.EnableOldValues();
			stacks.Add(positionStack);
			rotationRateStack = new VectorArrayStack();
			rotationRateStack.Initialize(this, AmpsHelpers.eStackFunction.RotationRate, typeof(BaseGenericModule).ToString());
			rotationRateStack.EnableOldValues();
			stacks.Add(rotationRateStack);
			rotationStack = new VectorArrayStack();
			rotationStack.Initialize(this, AmpsHelpers.eStackFunction.Rotation, typeof(BaseGenericModule).ToString());
			rotationStack.EnableOldValues();
			stacks.Add(rotationStack);
			scaleStack = new VectorArrayStack();
			scaleStack.Initialize(this, AmpsHelpers.eStackFunction.Scale, typeof(BaseGenericModule).ToString());
			stacks.Add(scaleStack);
			colorStack = new VectorArrayStack();
			colorStack.Initialize(this, AmpsHelpers.eStackFunction.Color, typeof(BaseGenericModule).ToString());
			stacks.Add(colorStack);
			pivotOffsetStack = new VectorArrayStack();
			pivotOffsetStack.Initialize(this, AmpsHelpers.eStackFunction.PivotOffset, typeof(BaseGenericModule).ToString());
			stacks.Add(pivotOffsetStack);
			multiFunctionStack = new MultiFunctionStack();
			multiFunctionStack.Initialize(this, AmpsHelpers.eStackFunction.MultiFunction, typeof(BaseMultiFunctionModule).ToString());
			stacks.Add(multiFunctionStack);
		}

		// COPY BLUEPRINT //
		//
		public void CopyBlueprint(AmpsBlueprint originalBlueprint)
		{
			emitterStack.CopyStack(originalBlueprint.emitterStack);
			renderStack.CopyStack(originalBlueprint.renderStack);
			sharedStack.CopyStack(originalBlueprint.sharedStack);
			spawnRateStack.CopyStack(originalBlueprint.spawnRateStack);
			deathConditionStack.CopyStack(originalBlueprint.deathConditionStack);
			deathDurationStack.CopyStack(originalBlueprint.deathDurationStack);
			customScalarStack.CopyStack(originalBlueprint.customScalarStack);
			customVectorStack.CopyStack(originalBlueprint.customVectorStack);
			accelerationStack.CopyStack(originalBlueprint.accelerationStack);
			velocityStack.CopyStack(originalBlueprint.velocityStack);
			positionStack.CopyStack(originalBlueprint.positionStack);
			rotationRateStack.CopyStack(originalBlueprint.rotationRateStack);
			rotationStack.CopyStack(originalBlueprint.rotationStack);
			scaleStack.CopyStack(originalBlueprint.scaleStack);
			colorStack.CopyStack(originalBlueprint.colorStack);
			pivotOffsetStack.CopyStack(originalBlueprint.pivotOffsetStack);
			multiFunctionStack.CopyStack(originalBlueprint.multiFunctionStack);
		}

		// GET SUB OBJECTS //
		//
		public List<Object> GetSubObjects()
		{
			ScalarProperty sp;
			VectorProperty vp;
			ColorProperty cp;

			List<Object> returnValue = new List<Object>();

			for (int i = 0; i < stacks.Count; i++)
			{
				for (int j = 0; j < stacks[i].modules.Count; j++)
				{
					//stacks[i].modules[j].name = stacks[i].modules[j].moduleName.GetValue();	// HACK
					returnValue.Add(stacks[i].modules[j]);

					for (int k = 0; k < stacks[i].modules[j].properties.Count; k++)
					{
						returnValue.Add(stacks[i].modules[j].properties[k]);

						if (stacks[i].modules[j].properties[k].GetType() == typeof(ScalarProperty))
						{
							sp = stacks[i].modules[j].properties[k] as ScalarProperty;
							returnValue.Add(sp.curve);
							returnValue.Add(sp.curveMin);
							returnValue.Add(sp.curveMax);
						}

						if (stacks[i].modules[j].properties[k].GetType() == typeof(VectorProperty))
						{
							vp = stacks[i].modules[j].properties[k] as VectorProperty;
							returnValue.Add(vp.curve);
							returnValue.Add(vp.curveMin);
							returnValue.Add(vp.curveMax);
						}

						if (stacks[i].modules[j].properties[k].GetType() == typeof(ColorProperty))
						{
							cp = stacks[i].modules[j].properties[k] as ColorProperty;
							returnValue.Add(cp.curve);
							returnValue.Add(cp.curveMin);
							returnValue.Add(cp.curveMax);
						}

						if (stacks[i].modules[j].properties[k].reference != null) returnValue.Add(stacks[i].modules[j].properties[k].reference);
					}
				}
			}

			return returnValue;
		}
#endif

	}
}