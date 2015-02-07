using UnityEngine;
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class BaseEmitterModule : BaseModule
	{
		public ScalarProperty emitterDuration;	// How long the emitter should last.
		public BoolProperty isLooping;			// If the emitter is looping.
		public ScalarProperty maxParticles;		// The maximum number of computed particles.
												// Particle number actually drawn could be less.
		public ScalarProperty updateRate;		// How often the emitter updates. Could change
												// dynamically for example by the LOD system.
		public ScalarProperty timeScale;			// The overall time dilation of the emitter.
		public ScalarProperty accelerationNoiseSmoothing;	// How much of the noisy acceleration calculation is smoothed.
		public BoolProperty playOnAwake;
		public ScalarProperty pauseWhenUnseenDuration;		// The emitter will pause if unseen for this long.
		//public BoolProperty canParentControlPlayback;		// If true then the playback functions can be called from parent.

		public bool isEmitting = false;

		// EVALUATE //
		//
		override public void Evaluate()
		{
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float input)
		{
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float[] input)
		{
		}

		// EVALUATE //
		//
		override public void Evaluate(ref Vector4[] input)
		{
		}

		// SOFT RESET //
		//
		override public void SoftReset()
		{
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			type = "Basic setup";

			emitterDuration = ScriptableObject.CreateInstance<ScalarProperty>();
			emitterDuration.Initialize("Emitter duration", 5, theOwnerBlueprint);
			emitterDuration.SetDataModes(true, false, false, false, false, true);
			AddProperty(emitterDuration, false);
			isLooping = ScriptableObject.CreateInstance<BoolProperty>();
			isLooping.SetDataModes(true, false, false, false, false, false);
			isLooping.Initialize("Looping?", true, theOwnerBlueprint);
			AddProperty(isLooping, false);
			maxParticles = ScriptableObject.CreateInstance<ScalarProperty>();
			maxParticles.Initialize("Particle limit", 10, theOwnerBlueprint);
			maxParticles.SetDataModes(true, true, false, false, false, false);
			maxParticles.isInteger = true;
			AddProperty(maxParticles, false);
			updateRate = ScriptableObject.CreateInstance<ScalarProperty>();
			updateRate.Initialize("Updates per second", 30, theOwnerBlueprint);
			updateRate.SetDataModes(true, false, false, false, false, false);
			AddProperty(updateRate, false);
			timeScale = ScriptableObject.CreateInstance<ScalarProperty>();
			timeScale.Initialize("Time scale", 1, theOwnerBlueprint);
			timeScale.SetDataModes(true, false, false, false, false, false);
			AddProperty(timeScale, false);
			accelerationNoiseSmoothing = ScriptableObject.CreateInstance<ScalarProperty>();
			accelerationNoiseSmoothing.Initialize("Accel. noise smoothing", 0.5f, theOwnerBlueprint);
			accelerationNoiseSmoothing.SetDataModes(true, false, false, false, false, false);
			AddProperty(accelerationNoiseSmoothing, false);
			playOnAwake = ScriptableObject.CreateInstance<BoolProperty>();
			playOnAwake.SetDataModes(true, false, false, false, false, false);
			playOnAwake.Initialize("Plays on start?", true, theOwnerBlueprint);
			AddProperty(playOnAwake, false);
			pauseWhenUnseenDuration = ScriptableObject.CreateInstance<ScalarProperty>();
			pauseWhenUnseenDuration.Initialize("Pause when unseen duration", 1, theOwnerBlueprint);
			pauseWhenUnseenDuration.SetDataModes(true, false, false, false, false, false);
			AddProperty(pauseWhenUnseenDuration, false);
			//canParentControlPlayback = ScriptableObject.CreateInstance<BoolProperty>();
			//canParentControlPlayback.SetDataModes(true, false, false, false, false, false);
			//canParentControlPlayback.Initialize("Can parent control playback?", true, theOwnerBlueprint);
			//AddProperty(canParentControlPlayback, false);
		}

//============================================================================//
#region GUI

		// SHOW MODULE //
		//
		// The emitter module is special and needs its own, simplified UI renderer.
		override public void ShowModule(ref BaseModule selectedModule, out AmpsHelpers.eModuleOperations operation)
		{
			GUIStyle actualStyle;

			operation = AmpsHelpers.eModuleOperations.NoOperation;
			actualStyle = GUI.skin.GetStyle("moduleBoxSelected");

			GUILayout.BeginVertical(actualStyle);							//--------------------------------//

			GUILayout.BeginHorizontal("moduleHeader");						//----------------//
			GUILayout.FlexibleSpace();
			GUILayout.Label(type, "moduleName");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();										//----------------\\

			GUILayout.BeginHorizontal();									//----------------//

			GUILayout.BeginVertical();										//--------//
			GUILayout.Space(8);
			GUILayout.Label(moduleName.value, "moduleDescription");
			GUILayout.EndVertical();										//--------\\

			GUILayout.EndHorizontal();										//----------------\\

			GUILayout.EndVertical();										//--------------------------------\\
		}

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			BaseProperty previousSelection = selectedProperty;

			//moduleName.ShowProperty(ref selectedProperty, false);
			playOnAwake.ShowProperty(ref selectedProperty, false);
			pauseWhenUnseenDuration.ShowProperty(ref selectedProperty, false);

			//if (canParentControlPlayback == null)	// HACK
			//{
			//    canParentControlPlayback = ScriptableObject.CreateInstance<BoolProperty>();
			//    canParentControlPlayback.SetDataModes(true, false, false, false, false, false);
			//    canParentControlPlayback.Initialize("Can parent control playback?", true, ownerBlueprint);
			//    AddProperty(canParentControlPlayback, false);
			//}
			//canParentControlPlayback.ShowProperty(ref selectedProperty, false);

			emitterDuration.ShowProperty(ref selectedProperty, false);
			isLooping.ShowProperty(ref selectedProperty, false);
			maxParticles.ShowProperty(ref selectedProperty, false);
			timeScale.ShowProperty(ref selectedProperty, false);
			accelerationNoiseSmoothing.ShowProperty(ref selectedProperty, false);
			// TODO: Add update rate.

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}

#endregion

#endif
	}
}