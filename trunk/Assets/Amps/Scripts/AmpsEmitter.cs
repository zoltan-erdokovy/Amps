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
using System.Collections.Generic;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amps
{
	[AddComponentMenu("Amps/Emitter")]
	[System.Serializable]
//	[ExecuteInEditMode]
	public class AmpsEmitter : MonoBehaviour
	{
		public AmpsBlueprint blueprint;			// Reference to the currently used AmpsBlueprint asset.

		#if UNITY_EDITOR
		private AmpsBlueprint previousBlueprint;	// The asset which was used the last time we checked. Helps handling asset ref changes.
		#endif

		public float emitterTime;
		public float nonZeroParticleCountTime;
		public float deltaTime;
		public float smoothDeltaTime;
		public float previousTimeScale;
		public Mesh emitterMesh;
		public bool isPlaying = true;
		public bool isPaused = false;
		public bool isStopped = false;
		public bool isLooping = false;
		private bool wasPaused = false;
		private bool wasPlaying = false;
		public Transform currentCameraTransform;

		// Not serialized flag so we can tell if this emitter (and the scene) was freshly loaded.
		[System.NonSerialized]
		public bool isEmitterDataReset;

		public float emitterLoopTime;		// 0..1, repeats on each loop.
		public Vector3 emitterAcceleration;
		public Vector3 emitterVelocity;
		public Vector3 emitterPosition;
		public Vector3 emitterRotation;
		public Vector3 emitterDirection;
		public Vector3 emitterScale;
		public Matrix4x4 emitterMatrixFull;			// Used for coordinate system conversions.
		public Matrix4x4 emitterMatrixPositionZero;

		public int[] particleIds;			// Mostly used as random seeds.
		public float[] particleTimes;		// The ages of the particles.
		public float[] particleDyingTimes;	// At which time the particle supposed to be removed, after DeathCondition reaching 1.
		public float[] travelDistances;
		public float[] collisionTimes;				// The timestamps of last collisions.
		public Vector3[] accelerationAccumulators;	// The accumulating particle accelerations.
		public Vector3[] velocityAccumulators;		// The accumulating particle velocities.
		public Vector3[] rotationRateAccumulators;	// The accumulating particle rotation rates.

		// This is the pool for managing particle availability. ParticleMarker is an empty class.
		// TODO: It would be better to use bools for the pool instead of instances of this class.
		public Pool<ParticleMarker> particleMarkers;

		public float spawnRateAccumulator = 0;					// For computing the actual number of particles to be created.
		public List<int> newParticleIndices = new List<int>();	// The indices of the newly created particles
																// so modules can manage their own particle
																// specific data structures.
		public int[] activeParticleIndices;						// The array of indices for the active particles.
		public BaseStack selectedStack = null;
		public List<BaseParameter> parameters = new List<BaseParameter>();		// All emitter parameters.
		public List<BaseModule> visualizedModules = new List<BaseModule>();		// The list of modules which need to be drawn.
		public List<EventListenerModule> eventListenerModules = new List<EventListenerModule>();	// Listener modules which need to be notified about events.
		public int exampleInputParticleIndex;									// The particle used for visualization.
		public int randomSeed;													// An emitter specific random seed.
		//private int originalRandomSeed;											// The original random seed in Unity, to be restored.

		public double currentTime = 0;
		public double prevTime = 0;
		public double lastRenderTime = 0;
		[System.NonSerialized]
		public bool isEditorDriven = false;						// Used to adapt features for editing.
		
		public BaseEmitterModule emitterModule;

		#if UNITY_EDITOR		
		public List<PropertyReference> sharedProperties = new List<PropertyReference>();
		public bool needsRepaint;	// If the editor window needs a repaint due to changes in the emitter.
		#endif

		private MeshFilter meshFilter;

		// GET PARAMETER //
		//
		public BaseParameter GetParameter(string desiredParameterName, AmpsHelpers.eParameterTypes desiredType)
		{
			BaseParameter returnValue = null;

			if (parameters.Count > 0)
			{
				for (int i = 0; i < parameters.Count; i++)
				{
					if (parameters[i].name == desiredParameterName && parameters[i].type == desiredType)
					{
						returnValue = parameters[i];
						break;
					}
				}
			}

			return returnValue;
		}

		// GET RANDOM PARTICLE INDEX //
		//
		public int GetRandomParticleIndex(int theParticleId)
		{
			int returnValue = -1;

			if (particleMarkers.ActiveCount > 0)
			{
				System.Random theRandom = new System.Random(theParticleId);
				int randomIndex = theRandom.Next(0, particleMarkers.ActiveCount);

				for (int i = 0; i < activeParticleIndices.Length; i++)
				{
					if (i == randomIndex)
					{
						returnValue = activeParticleIndices[i];
						break;
					}
				}
			}

			return returnValue;
		}

		// UPDATE CURRENT TIME //
		//
		void UpdateCurrentTime()
		{
			#if UNITY_EDITOR
			currentTime = EditorApplication.timeSinceStartup * emitterModule.timeScale.constant;
			#else
			currentTime = Time.realtimeSinceStartup  * emitterModule.timeScale.constant;
			#endif
		}

		// AWAKE //
		//
		void Awake()
		{
			AmpsHelpers.identityMatrix = Matrix4x4.identity;	// Set that variable when an existing emitter is loaded in the scene.
			HandleBlueprintChange();
			SoftReset();
		}

		// PLAY //
		//
		public void Play()
		{
			SoftReset();

			isPlaying = true;
			isStopped = false;
		}

		// PAUSE //
		//
		public void Pause()
		{
			isPlaying = false;
			isPaused = true;
			wasPaused = false;
		}
		
		// UNPAUSE //
		//
		public void Unpause()
		{
			isPlaying = true;
			isPaused = false;
			wasPaused = true;
		}

		// STOP //
		//
		public void Stop()
		{
			if (emitterModule.isLooping.GetValue())
			{
				emitterModule.isLooping.value = false;
				isLooping = false;
			}
			else
			{
				isPlaying = false;
				isStopped = true;
			}
		}

		// ON WILL RENDER OBJECT //
		//
		void OnWillRenderObject()
		{
			lastRenderTime = currentTime;

			// TODO: Switch from low detail mode to full detail.
			if (wasPlaying)
			{
				wasPlaying = false;
				Unpause();
			}

			if (blueprint != null &&
				enabled &&
				particleMarkers.ActiveCount > 0 &&
				isPlaying)
			{
				if (Camera.current != null) currentCameraTransform = Camera.current.transform;

				emitterMesh.MarkDynamic();
				blueprint.renderStack.Evaluate();
				emitterMesh.RecalculateBounds();
			}
		}

		// ON BECAME VISIBLE //
		//
		void OnBecameVisible()
		{
			//lastRenderTime = currentTime;
			//if (gameObject.name == "CardDeckGlowEmitter") Amps.AmpsHelpers.AmpsWarning("OnBecameVisible()");
			//// TODO: Switch from low detail mode to full detail.
			//if (wasPlaying)
			//{
			//	if (gameObject.name == "CardDeckGlowEmitter") Amps.AmpsHelpers.AmpsWarning("OnBecameVisible() - Unpausing");
			//	wasPlaying = false;
			//	Unpause();
			//}
		}

		// ON BECAME INVISIBLE //
		//
		//void OnBecameInvisible()
		//{
		//    // TODO: Switch from full detail mode to low detail.
		//    if (isPlaying)
		//    {
		//        wasPlaying = true;
		//        Pause();
		//    }
		//}

		// ON ENABLE //
		//
		public void OnEnable()
		{
			//SoftReset();
		}

		// START //
		//
		void Start()
		{
		}

		// RESET //
		//
		void Reset()
		{
			#if UNITY_EDITOR
			Initialize();
			#endif
		}

		// AMPS HANDLE EVENT //
		//
		public void AmpsHandleEvent(EventData theEventData)
		{
			if (theEventData != null && theEventData.eventName != null)
			{
				for (int i = 0; i < eventListenerModules.Count; i++)
				{
					eventListenerModules[i].HandleEvent(theEventData);
				}
			}
		}

		// HANDLE BLUEPRINT CHANGE //
		//
		public void HandleBlueprintChange()
		{
			if (blueprint != null)
			{
				blueprint.ownerEmitter = this;
				selectedStack = blueprint.emitterStack;
				emitterModule = blueprint.emitterStack.modules[0] as BaseEmitterModule;
				#if UNITY_EDITOR
				UpdateSharedPropertyList();
				#endif
			}
			else
			{
				selectedStack = null;
				emitterModule = null;
			}

			#if UNITY_EDITOR
			previousBlueprint = blueprint;
			#endif

			SoftReset();
		}

		// UPDATE MATERIALS //
		//
		public void UpdateMaterials()
		{
			MeshRenderer mr = GetComponent<MeshRenderer>();
			Material[] materials = new Material[blueprint.renderStack.modules.Count];

			for (int i = 0; i < blueprint.renderStack.modules.Count; i++)
			{
				BaseRenderModule rm = blueprint.renderStack.modules[i] as BaseRenderModule;
				materials[i] = rm.particleMaterial.GetValue();
			}

			mr.materials = materials;
		}

		// RESET EMITTER DATA //
		//
		public void ResetEmitterData()
		{
			emitterTime = 0;
			nonZeroParticleCountTime = 0;
			emitterLoopTime = 0;
			smoothDeltaTime = 0;
			emitterAcceleration = Vector3.zero;
			emitterVelocity = Vector3.zero;
			emitterPosition = transform.position;
			emitterRotation = transform.rotation.eulerAngles;
			emitterScale = transform.lossyScale;
			exampleInputParticleIndex = -1;
			System.Random theRandom = new System.Random();
			randomSeed = theRandom.Next(65535);
			spawnRateAccumulator = 0;
			newParticleIndices = new List<int>();
			emitterModule = blueprint.emitterStack.modules[0] as BaseEmitterModule;

			isPlaying = emitterModule.playOnAwake.GetValue();
			wasPlaying = false;
			isStopped = isPlaying == false;
			isPaused = false;
			wasPaused = false;
			isLooping = emitterModule.isLooping.GetValue();

			UpdateCurrentTime();
			prevTime = currentTime;
			lastRenderTime = currentTime;

			int maxParticleNumber = (int)emitterModule.maxParticles.GetValue();
			particleMarkers = new Pool<ParticleMarker>(maxParticleNumber);
			particleIds = new int[maxParticleNumber];
			particleTimes = new float[maxParticleNumber];
			particleDyingTimes = new float[maxParticleNumber];
			accelerationAccumulators = new Vector3[maxParticleNumber];
			velocityAccumulators = new Vector3[maxParticleNumber];
			rotationRateAccumulators = new Vector3[maxParticleNumber];
			travelDistances = new float[maxParticleNumber];
			collisionTimes = new float[maxParticleNumber];

			isEmitterDataReset = true;
		}

		// SOFT RESET //
		//
		public void SoftReset()
		{
			if (blueprint == null) return;

			ResetMesh();
			ResetEmitterData();

			//if (blueprint.ownerEmitter != this) blueprint.ownerEmitter = this;

			blueprint.emitterStack.SoftReset();
			blueprint.renderStack.SoftReset();
			// sharedStack does not need a SoftReset() as none of the modules there are executed.
			blueprint.spawnRateStack.SoftReset();
			blueprint.deathConditionStack.SoftReset();
			blueprint.deathDurationStack.SoftReset();
			blueprint.customScalarStack.SoftReset();
			blueprint.customVectorStack.SoftReset();
			blueprint.accelerationStack.SoftReset();
			blueprint.velocityStack.SoftReset();
			blueprint.positionStack.SoftReset();
			blueprint.rotationRateStack.SoftReset();
			blueprint.rotationStack.SoftReset();
			blueprint.scaleStack.SoftReset();
			blueprint.colorStack.SoftReset();
			blueprint.pivotOffsetStack.SoftReset();

			blueprint.multiFunctionStack.SoftReset();

			#if UNITY_EDITOR
			visualizedModules.Clear();
			visualizedModules.AddRange(blueprint.accelerationStack.GetVisualizedModules());
			visualizedModules.AddRange(blueprint.velocityStack.GetVisualizedModules());
			visualizedModules.AddRange(blueprint.positionStack.GetVisualizedModules());
			visualizedModules.AddRange(blueprint.rotationRateStack.GetVisualizedModules());
			visualizedModules.AddRange(blueprint.rotationStack.GetVisualizedModules());
			visualizedModules.AddRange(blueprint.scaleStack.GetVisualizedModules());
			visualizedModules.AddRange(blueprint.colorStack.GetVisualizedModules());
			visualizedModules.AddRange(blueprint.pivotOffsetStack.GetVisualizedModules());
			

			eventListenerModules.Clear();
			eventListenerModules.AddRange(blueprint.spawnRateStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.deathConditionStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.deathDurationStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.customScalarStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.customVectorStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.accelerationStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.velocityStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.positionStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.rotationRateStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.rotationStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.scaleStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.colorStack.GetEventListenerModules());
			eventListenerModules.AddRange(blueprint.pivotOffsetStack.GetEventListenerModules());
			#endif

			previousTimeScale = emitterModule.timeScale.constant;
		}

		// RESET MESH //
		//
		public void ResetMesh()
		{
			if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
#if UNITY_EDITOR
			meshFilter.sharedMesh = new Mesh();
			emitterMesh = meshFilter.sharedMesh;
#else
			meshFilter.mesh = new Mesh();
			emitterMesh = meshFilter.mesh;
#endif
			emitterMesh.name = "AmpsParticleMesh";

			if (blueprint != null && blueprint.renderStack.modules.Count > 0)
			{
				emitterMesh.subMeshCount = blueprint.renderStack.modules.Count;
				UpdateMaterials();
			}
		}

		// UPDATE //
		//
		void Update()
		{
			if (blueprint != null)
			{
				DoUpdate();
			}
		}

		// DO UPDATE //
		//
		public void DoUpdate()
		{
			Vector3 currentEmitterVelocity;
			Vector3 currentEmitterAcceleration;

			#if UNITY_EDITOR
			if (blueprint != previousBlueprint) HandleBlueprintChange();
			#endif

			deltaTime = 0;
			UpdateCurrentTime();

			// Pause when not edited, playing and not seen for a while.
			if (isEditorDriven == false &&
				isPlaying &&
				lastRenderTime + emitterModule.pauseWhenUnseenDuration.GetValue() < currentTime)
			{
				wasPlaying = true;
				Pause();
				return;
			}

			if ((emitterModule.isLooping.GetValue() == false &&
				emitterTime > emitterModule.emitterDuration.GetValue()) &&
				particleMarkers.ActiveCount == 0)
			{
				isPlaying = false;
				isStopped = true;
			}
			
			if (enabled == false ||		// Prevents updates driven by the amps editor window while playing.
				isPlaying == false)
			{
				return;
			}
			
			//originalRandomSeed = Random.seed;

#if UNITY_EDITOR
			if (previousTimeScale != emitterModule.timeScale.constant) SoftReset();

			// At extremely low update rates (often caused by opening a menu or switching tasks)
			// we softreset to prevent spawnRateAccumulator reaching huge values.
			//if ((float)(currentTime - prevTime) > 1 && wasPaused == false) SoftReset();
			if ((float)(currentTime - prevTime) > 1) deltaTime = 0.08333f;	// 1/120 seconds, to avoid division by zero.
#endif

			if (wasPaused == true)
			{
				deltaTime = 0.08333f;	// 1/120 seconds, to avoid division by zero.
				wasPaused = false;
			}
			else deltaTime = (float)(currentTime - prevTime);
			smoothDeltaTime = Mathf.Lerp(smoothDeltaTime, deltaTime, 0.02f);

			emitterTime += deltaTime;
			emitterLoopTime = emitterTime / emitterModule.emitterDuration.GetValue();
			emitterLoopTime = emitterLoopTime - Mathf.Floor(emitterLoopTime); // Frac();

			if (particleMarkers.ActiveCount == 0 && nonZeroParticleCountTime != 0) nonZeroParticleCountTime = 0f;
			if (particleMarkers.ActiveCount > 0) nonZeroParticleCountTime += deltaTime;

			currentEmitterVelocity = (transform.position - emitterPosition) / deltaTime;
			currentEmitterAcceleration = (currentEmitterVelocity - currentEmitterVelocity) / deltaTime;
			emitterPosition = transform.position;
			emitterRotation = transform.rotation.eulerAngles;
			emitterScale = transform.lossyScale;
			emitterDirection = transform.forward;
			emitterVelocity = currentEmitterVelocity;
			emitterAcceleration = currentEmitterAcceleration;
			emitterMatrixFull = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
			emitterMatrixPositionZero = Matrix4x4.TRS(Vector3.zero, transform.rotation, transform.lossyScale);

			activeParticleIndices = particleMarkers.GetActiveIndices();

			for (int i = 0; i < activeParticleIndices.Length; i++)
			{
				particleTimes[activeParticleIndices[i]] += deltaTime;
			}
			
			EvaluateStacks();

			prevTime = currentTime;

			//Random.seed = originalRandomSeed;
		}


		// EVALUATE STACKS //
		//
		void EvaluateStacks()
		{
			// Killing off particles if necessary.
			blueprint.deathConditionStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_DeathCondition();

			int particleIndex;
			for (int i = 0; i < activeParticleIndices.Length; i++)
			{
				particleIndex = activeParticleIndices[i];
				if (blueprint.deathConditionStack.values[particleIndex] >= 1 && particleDyingTimes[particleIndex] == -1)
				{
					particleDyingTimes[particleIndex] = particleTimes[particleIndex] + blueprint.deathDurationStack.values[particleIndex];
				}

				if (particleDyingTimes[particleIndex] != -1 && particleTimes[particleIndex] >= particleDyingTimes[particleIndex])
				{
					particleMarkers.Return(activeParticleIndices[i]);
					if (particleIndex == exampleInputParticleIndex) exampleInputParticleIndex = -1;
				}
			}

			newParticleIndices.Clear();
			visualizedModules.Clear();	// It will be repopulated in stack.Evaluate() functions.

			// We only spawn new particles if the emitter is looping or if it isn't but still has duration left.
			if (emitterModule.isLooping.GetValue() ||
				(emitterModule.isLooping.GetValue() == false &&
				 emitterTime <= emitterModule.emitterDuration.GetValue()))
			{
				blueprint.spawnRateStack.Evaluate();
				blueprint.multiFunctionStack.Evaluate_SpawnRate();

				// This condition prevents accumulation while the emitter is maxed out.
				if (particleMarkers.AvailableCount > 0) spawnRateAccumulator += blueprint.spawnRateStack.value * deltaTime;

				// Spawning new particles.
				if (Mathf.Floor(spawnRateAccumulator) > 0 && particleMarkers.AvailableCount > 0)
				{
					int toSpawn = Mathf.Min(Mathf.FloorToInt(spawnRateAccumulator), particleMarkers.AvailableCount);
					for (int i = 0; i < toSpawn; i++)
					{
						int newIndex = -1;	// For storing the index of the newly added item.

						try { particleMarkers.Get(out newIndex); }
						catch (System.InvalidOperationException) { break; }	// No more room in the pool...

						// Initializing data for the new particle.
						particleIds[newIndex] = Random.Range(0, int.MaxValue);//Mathf.FloorToInt((float)currentTime) * (newIndex + 1);
						particleTimes[newIndex] = 0;
						particleDyingTimes[newIndex] = -1;
						blueprint.deathConditionStack.values[newIndex] = 0;
						blueprint.deathDurationStack.values[newIndex] = 0;
						blueprint.customScalarStack.values[newIndex] = 0;
						blueprint.customVectorStack.values[newIndex] = Vector4.zero;
						blueprint.accelerationStack.values[newIndex] = Vector4.zero;
						blueprint.velocityStack.values[newIndex] = Vector4.zero;
						blueprint.positionStack.values[newIndex] = Vector4.zero;
						blueprint.rotationRateStack.values[newIndex] = Vector4.zero;
						blueprint.rotationStack.values[newIndex] = Vector4.zero;
						blueprint.scaleStack.values[newIndex] = Vector4.one;//Vector4.zero;
						blueprint.colorStack.values[newIndex] = Vector4.zero;
						blueprint.pivotOffsetStack.values[newIndex] = Vector4.zero;
						accelerationAccumulators[newIndex] = Vector3.zero;
						velocityAccumulators[newIndex] = Vector3.zero;
						rotationRateAccumulators[newIndex] = Vector3.zero;
						travelDistances[newIndex] = 0;
						collisionTimes[newIndex] = 0;

						newParticleIndices.Add(newIndex);
					}

					spawnRateAccumulator -= Mathf.Floor(spawnRateAccumulator);	// Leave fractions only.
				}
			}
			else // If we are not looping or beyond the set duration...
			{
				if (spawnRateAccumulator != 0) spawnRateAccumulator = 0;
			}

#if UNITY_EDITOR
			if (exampleInputParticleIndex == -1) UpdateExampleParticleIndex();
#endif
			activeParticleIndices = particleMarkers.GetActiveIndices();	// Updating the array with freshly spawned particles.

			// If there are no particles at this time then we don't bother evaluating the rest of the stacks.
			if (particleMarkers.ActiveCount == 0) return;

			blueprint.deathDurationStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_DeathDuration();

			blueprint.customScalarStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_CustomScalar();

			blueprint.customVectorStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_CustomVector();

			blueprint.accelerationStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_Acceleration();
			AccumulateAcceleration();

			blueprint.velocityStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_Velocity();
			AccumulateVelocity();

			blueprint.positionStack.Evaluate();					// Evaluating the dedicated stack.
			blueprint.multiFunctionStack.Evaluate_Position();	// Evaluating the multi function stack (post process).
			CalculateFinalPosition();								// Handling position specific calculations.

			blueprint.pivotOffsetStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_PivotOffset();

			blueprint.rotationRateStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_RotationRate();
			AccumulateRotationRate();

			blueprint.rotationStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_Rotation();
			CalculateFinalRotation();

			blueprint.scaleStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_Scale();

			blueprint.colorStack.Evaluate();
			blueprint.multiFunctionStack.Evaluate_Color();
		}

		// ACCUMULATE ACCELERATION //
		//
		public void AccumulateAcceleration()
		{
			foreach (Pool<ParticleMarker>.Node node in particleMarkers.ActiveNodes)
			{
				accelerationAccumulators[node.NodeIndex] += AmpsHelpers.ConvertVector4Vector3(blueprint.accelerationStack.values[node.NodeIndex]) *
																								deltaTime;
			}
		}

		// ACCUMULATE VELOCITY //
		//
		public void AccumulateVelocity()
		{
			foreach (Pool<ParticleMarker>.Node node in particleMarkers.ActiveNodes)
			{
				blueprint.velocityStack.values[node.NodeIndex] += AmpsHelpers.ConvertVector3Vector4(accelerationAccumulators[node.NodeIndex], 0);
				velocityAccumulators[node.NodeIndex] += AmpsHelpers.ConvertVector4Vector3(blueprint.velocityStack.values[node.NodeIndex]) * deltaTime;
			}
		}

		// CALCULATE FINAL POSITION //
		//
		public void CalculateFinalPosition()
		{
			foreach (Pool<ParticleMarker>.Node node in particleMarkers.ActiveNodes)
			{
				// We calculate velocity coming from direct positioning.
				Vector4 velocityFromPosition = (blueprint.positionStack.values[node.NodeIndex] - blueprint.positionStack.oldValues[node.NodeIndex]) / deltaTime;
				// Since we have no need for the old value anymore, we update it.
				blueprint.positionStack.oldValues[node.NodeIndex] = blueprint.positionStack.values[node.NodeIndex];
				// We factor in velocity coming from the velocity stack.
				blueprint.positionStack.values[node.NodeIndex] += AmpsHelpers.ConvertVector3Vector4(velocityAccumulators[node.NodeIndex], 0);
				
				// Update velocity data with the velocity change coming from direct positioning.
				blueprint.velocityStack.values[node.NodeIndex] += velocityFromPosition;
				// We calculate acceleration coming from direct velocity setting plus direct positioning.
				Vector4 accelerationFromVelocity = (blueprint.velocityStack.values[node.NodeIndex] - blueprint.velocityStack.oldValues[node.NodeIndex]) / deltaTime;
				// Since we have no need for the old value anymore, we update it.
				blueprint.velocityStack.oldValues[node.NodeIndex] = blueprint.velocityStack.values[node.NodeIndex];
				// Update acceleration data with some smoothing.
				blueprint.accelerationStack.values[node.NodeIndex] = Vector4.Lerp(blueprint.accelerationStack.oldValues[node.NodeIndex], blueprint.accelerationStack.values[node.NodeIndex]
																		+ accelerationFromVelocity,
																		Mathf.Lerp(100, 0, emitterModule.accelerationNoiseSmoothing.constant) * deltaTime);
				// We store the current values for later use.
				blueprint.accelerationStack.oldValues[node.NodeIndex] = blueprint.accelerationStack.values[node.NodeIndex];
			}
		}

		// ACCUMULATE ROTATION RATE //
		//
		public void AccumulateRotationRate()
		{
			foreach (Pool<ParticleMarker>.Node node in particleMarkers.ActiveNodes)
			{
				rotationRateAccumulators[node.NodeIndex] += AmpsHelpers.ConvertVector4Vector3(blueprint.rotationRateStack.values[node.NodeIndex])
																								* deltaTime;
			}
		}

		// CALCULATE FINAL ROTATION //
		//
		public void CalculateFinalRotation()
		{
			Vector3 v;

			foreach (Pool<ParticleMarker>.Node node in particleMarkers.ActiveNodes)
			{
				// We calculate velocity coming from direct rotation.
				Vector4 rotationRateFromRotation = (blueprint.rotationStack.values[node.NodeIndex] - blueprint.rotationStack.oldValues[node.NodeIndex]) / deltaTime;
				// Since we have no need for the old value anymore, we update it.
				blueprint.rotationStack.oldValues[node.NodeIndex] = blueprint.rotationStack.values[node.NodeIndex];
				// We factor in rotation rate coming from the rotation rate stack.
				v = (Quaternion.Euler(blueprint.rotationStack.values[node.NodeIndex]) * Quaternion.Euler(rotationRateAccumulators[node.NodeIndex])).eulerAngles;
				blueprint.rotationStack.values[node.NodeIndex] = new Vector4(v.x, v.y, v.z, 0);

				// Update rotation rate data with the rotation change coming from direct rotation.
				blueprint.rotationRateStack.values[node.NodeIndex] += rotationRateFromRotation;
			}
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		public void Initialize()
		{
			if (GetComponent<MeshFilter>() == null) gameObject.AddComponent<MeshFilter>();
			meshFilter = GetComponent<MeshFilter>();
			if (GetComponent<MeshRenderer>() == null) gameObject.AddComponent<MeshRenderer>();
			ResetMesh();

			AmpsHelpers.identityMatrix = Matrix4x4.identity;	// Set that variable when the emitter is created in the editor.
			blueprint = null;
		}

		// UPDATE EXAMPLE PARTICLE INDEX //
		//
		public void UpdateExampleParticleIndex()
		{
			if (exampleInputParticleIndex == -1)
			{
				float lowestParticleTime = float.MaxValue;

				foreach (Pool<ParticleMarker>.Node node in particleMarkers.ActiveNodes)
				{
					if (particleTimes[node.NodeIndex] < lowestParticleTime)
					{
						exampleInputParticleIndex = node.NodeIndex;
						lowestParticleTime = particleTimes[node.NodeIndex];
					}
				}
			}
		}

		// UPDATE SHARED PROPERTY LIST //
		//
		public void UpdateSharedPropertyList()
		{
			sharedProperties.Clear();

			foreach (BaseModule m in blueprint.sharedStack.modules)
			{
				sharedProperties.AddRange(m.GetSharedProperties());
			}
		}

//============================================================================//
#region GUI

		// SHOW STACKS //
		//
		public void ShowStacks()
		{
			GUILayout.BeginVertical("stackGroup1");
			blueprint.emitterStack.ShowStack(ref selectedStack);
			GUILayout.EndVertical();

			GUILayout.BeginVertical("stackGroup1");
			blueprint.renderStack.ShowStack(ref selectedStack);
			blueprint.sharedStack.ShowStack(ref selectedStack);
			GUILayout.EndVertical();

			GUILayout.BeginVertical("stackGroup4");
			blueprint.spawnRateStack.ShowStack(ref selectedStack);
			blueprint.deathConditionStack.ShowStack(ref selectedStack);
			blueprint.deathDurationStack.ShowStack(ref selectedStack);
			GUILayout.EndVertical();

			GUILayout.BeginVertical("stackGroup4");
			blueprint.customScalarStack.ShowStack(ref selectedStack);
			blueprint.customVectorStack.ShowStack(ref selectedStack);
			blueprint.accelerationStack.ShowStack(ref selectedStack);
			blueprint.velocityStack.ShowStack(ref selectedStack);
			blueprint.positionStack.ShowStack(ref selectedStack);
			blueprint.pivotOffsetStack.ShowStack(ref selectedStack);
			blueprint.rotationRateStack.ShowStack(ref selectedStack);
			blueprint.rotationStack.ShowStack(ref selectedStack);
			blueprint.scaleStack.ShowStack(ref selectedStack);
			blueprint.colorStack.ShowStack(ref selectedStack);
			GUILayout.EndVertical();

			GUILayout.BeginVertical("stackGroup1");
			blueprint.multiFunctionStack.ShowStack(ref selectedStack);
			GUILayout.EndVertical();
		}

		// ON DRAW GIZMOS SELECTED //
		//
		// Makeshift way of rendering particles.
		void OnDrawGizmosSelected()
		{
			//Vector3 particlePosition;
			//Vector3 particleRotation;
			//Vector3 arrowPoint1;
			//Vector3 arrowPoint2;
			//Vector3 arrowPoint3;

			if (particleMarkers != null)
			{
				foreach (Pool<ParticleMarker>.Node node in particleMarkers.ActiveNodes)
				{
					if (visualizedModules.Count > 0 && node.NodeIndex == exampleInputParticleIndex)
					{
						Gizmos.color = new Color(1, 1, 1, 0.5F);
						Gizmos.DrawSphere(blueprint.positionStack.values[node.NodeIndex], 0.1f);
					}

					//arrowPoint1 = new Vector3(0, 0, -1f);
					//arrowPoint2 = new Vector3(0.5f, 0, -0.5f);
					//arrowPoint3 = new Vector3(-0.5f, 0, -0.5f);
					//Gizmos.color = HSVtoRGB(new Vector3((1 - deathConditionStack.values[node.NodeIndex]) * 0.3f, 1, 1));
					//Gizmos.color = colorStack.values[node.NodeIndex];
					//Gizmos.DrawCube(positionStack.values[node.NodeIndex], new Vector3(0.02f, 0.02f, 0.02f));
					//particlePosition = new Vector3(positionStack.values[node.NodeIndex].x, positionStack.values[node.NodeIndex].y, positionStack.values[node.NodeIndex].z);
					//particleRotation = new Vector3(rotationStack.values[node.NodeIndex].x, rotationStack.values[node.NodeIndex].y, rotationStack.values[node.NodeIndex].z);
					//arrowPoint1.z *= scaleStack.values[node.NodeIndex].z;
					//arrowPoint2.x *= scaleStack.values[node.NodeIndex].x;
					//arrowPoint2.z *= scaleStack.values[node.NodeIndex].z;
					//arrowPoint3.x *= scaleStack.values[node.NodeIndex].x;
					//arrowPoint3.z *= scaleStack.values[node.NodeIndex].z;
					//Gizmos.DrawLine(particlePosition, particlePosition + AmpsHelpers.RotateAroundPoint(arrowPoint1, Vector3.zero, Quaternion.Euler(particleRotation)));
					//Gizmos.DrawLine(particlePosition, particlePosition + AmpsHelpers.RotateAroundPoint(arrowPoint2, Vector3.zero, Quaternion.Euler(particleRotation)));
					//Gizmos.DrawLine(particlePosition, particlePosition + AmpsHelpers.RotateAroundPoint(arrowPoint3, Vector3.zero, Quaternion.Euler(particleRotation)));
				}
			}
		}

#endregion

#endif
	}
}