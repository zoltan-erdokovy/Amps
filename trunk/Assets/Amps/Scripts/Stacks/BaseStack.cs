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
using System;
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	[System.Serializable]
	public class BaseStack
	{
		public string name;
		public AmpsHelpers.eStackFunction stackFunction;			// Providing a clue about what the stack instance is used for.
		public List<BaseModule> modules = new List<BaseModule>();
		public string moduleType = typeof(BaseModule).ToString();	// The compatible module type.

		#if UNITY_EDITOR
		public BaseModule selectedModule = null;					// The currently selected module, can be null if the stack is empty.
		#endif

		#if UNITY_EDITOR
		#endif
		public AmpsBlueprint ownerBlueprint;					// The blueprint the stack resides in, used main for aquiring data.

		public bool isVector3Stack;
		public bool isParticleStack;

		// SOFT RESET //
		//
		virtual public void SoftReset()
		{
			List<int> nullModuleIndices = new List<int>();

			if (modules.Count > 0)
			{
				for (int i = 0; i < modules.Count; i++)
				{
					if (modules[i] == null) nullModuleIndices.Add(i);	// HACK: Which is a module sometimes null?
					else modules[i].SoftReset();
				}
			}

			if (nullModuleIndices.Count > 0)
			{
				for (int i = 0; i < nullModuleIndices.Count; i++)
				{
					modules.Remove(modules[nullModuleIndices[i]]);
				}
			}
		}

		// EVALUATE //
		//
		virtual public void Evaluate()
		{
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		virtual public void Initialize(AmpsBlueprint theOwnerBlueprint, AmpsHelpers.eStackFunction theStackFunction, string theModuleType)
		{
			stackFunction = theStackFunction;
			name = AmpsHelpers.formatEnumString(theStackFunction.ToString());
			moduleType = theModuleType;
			ownerBlueprint = theOwnerBlueprint;
		}

		// COPY STACK //
		//
		public void CopyStack(BaseStack originalStack)
		{
			modules.Clear();
			foreach (var item in originalStack.modules)
			{
				BaseModule m = ScriptableObject.CreateInstance(item.GetType()) as BaseModule;
				m.Initialize(this, ownerBlueprint);
				m.CopyProperties(item, ownerBlueprint);
				modules.Add(m);
			}
		}

		// GET VISUALIZED MODULES //
		//
		public List<BaseModule> GetVisualizedModules()
		{
			List<BaseModule> returnValue = new List<BaseModule>();

			for (int i = 0; i < modules.Count; i++)
			{
				if (modules[i].implementsVisualization) returnValue.Add(modules[i]);
			}

			return returnValue;
		}

		// GET EVENT LISTENER MODULES //
		//
		public List<EventListenerModule> GetEventListenerModules()
		{
			List<EventListenerModule> returnValue = new List<EventListenerModule>();

			for (int i = 0; i < modules.Count; i++)
			{
				if (modules[i].GetType() == typeof(Amps.EventListenerModule)) returnValue.Add(modules[i] as EventListenerModule);
			}

			return returnValue;
		}

//============================================================================//
#region GUI

		// SHOW STACK //
		//
		// The stack renders its own UI.
		// The passed parameter helps to determine whether itself is selected in the
		// owner emitter. Also if either of the two (label looking) buttons are pressed
		// then selectedStack is set to self.
		public void ShowStack(ref BaseStack selectedStack)
		{
			// Shows the box for the stack.
			GUIStyle actualStyle;
			GUIStyle actualButtonStyle;

			if (selectedStack == this)
			{
				actualStyle = GUI.skin.GetStyle("stackBoxSelected");
				actualButtonStyle = GUI.skin.GetStyle("stackButton");
			}
			else
			{
				actualStyle = GUI.skin.GetStyle("stackBoxNormal");
				actualButtonStyle = GUI.skin.GetStyle("stackButton");
			}

			GUILayout.BeginHorizontal(actualStyle);
			if (GUILayout.Button(name, actualButtonStyle, GUILayout.Height(20)))
			{
				// Directly focus the ever present description property to avoid the weird
				// "focused inputfield carries over value to new inputfield" issue.
				GUI.FocusControl(AmpsHelpers.stringControlName);
				selectedStack = this;
			}
			if (modules.Count > 0)
			{
				if (GUILayout.Button("(" + modules.Count.ToString() + ")", "stackModuleNumber", GUILayout.Height(20)))
				{
					selectedStack = this;
					// Directly focus the ever present description property to avoid the weird
					// "focused inputfield carries over value to new inputfield" issue.
					GUI.FocusControl(AmpsHelpers.stringControlName);
				}
			}

			GUILayout.EndHorizontal();
		}

		// SHOW MODULES //
		//
		public void ShowModules(out bool shouldRepaint)
		{
			shouldRepaint = false;
			AmpsHelpers.eModuleOperations moduleOperation;
			bool isInfoModulePresent = false;	// A constantly updating info module makes debugging easier.

			if (modules.Count > 0)
			{
				if (selectedModule == null) selectedModule = modules[0];	// Deleting/replacing Blueprints can cause it to be null.

				// We store current selection so we can tell if a module selected itself,
				// in which case we need to repaint the UI.
				BaseModule previousSelection = selectedModule;

				foreach (BaseModule m in modules)
				{
					// We pass selectedModule so the module can switch draw styles if it recognizes
					// itself, but can also change the selection to itself from inside.
					// ModuleOperation is defined by which button the user clicked.
					m.ShowModule(ref selectedModule, out moduleOperation);
					switch (moduleOperation)
					{
						case AmpsHelpers.eModuleOperations.NoOperation:
							break;
						case AmpsHelpers.eModuleOperations.ShowOptions:
							GenericMenu moduleOptionsMenu = new GenericMenu();
							moduleOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.eModuleOperations.Duplicate.ToString())),
														false,
														DuplicateSelectedModule);
							moduleOptionsMenu.AddSeparator("");
							moduleOptionsMenu.AddItem(new GUIContent(AmpsHelpers.formatEnumString(AmpsHelpers.eModuleOperations.Remove.ToString())),
														false,
														RemoveSelectedModule);
							moduleOptionsMenu.ShowAsContext();
							break;
						case AmpsHelpers.eModuleOperations.MoveUp:
							MoveUpSelectedModule();
							break;
						case AmpsHelpers.eModuleOperations.MoveDown:
							MoveDownSelectedModule();
							break;
					}

					if (m.GetType() == typeof(InfoModule) && m.isEnabled) isInfoModulePresent = true;
				}

				if (isInfoModulePresent) shouldRepaint = true;

				if (selectedModule != previousSelection)
				{
					shouldRepaint = true;
					GUI.FocusControl(AmpsHelpers.stringControlName);
				}
			}
		}

#endregion

//============================================================================//
#region Module management

		// DELETE MODULE FROM ASSET //
		//
		public void DeleteModuleFromAsset(BaseModule theModule)
		{
			for (int i = 0; i < theModule.properties.Count; i++)
			{
				if (theModule.properties[i].GetType() == typeof(ScalarProperty))
				{
					ScalarProperty sp = theModule.properties[i] as ScalarProperty;
					UnityEngine.Object.DestroyImmediate(sp.curve, true);
					UnityEngine.Object.DestroyImmediate(sp.curveMin, true);
					UnityEngine.Object.DestroyImmediate(sp.curveMax, true);
				}

				if (theModule.properties[i].GetType() == typeof(VectorProperty))
				{
					VectorProperty vp = theModule.properties[i] as VectorProperty;
					UnityEngine.Object.DestroyImmediate(vp.curve, true);
					UnityEngine.Object.DestroyImmediate(vp.curveMin, true);
					UnityEngine.Object.DestroyImmediate(vp.curveMax, true);
				}

				if (theModule.properties[i].GetType() == typeof(ColorProperty))
				{
					ColorProperty cp = theModule.properties[i] as ColorProperty;
					UnityEngine.Object.DestroyImmediate(cp.curve, true);
					UnityEngine.Object.DestroyImmediate(cp.curveMin, true);
					UnityEngine.Object.DestroyImmediate(cp.curveMax, true);
				}

				if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(theModule.properties[i].reference)) == false)
				{
					UnityEngine.Object.DestroyImmediate(theModule.properties[i].reference, true);
				}
				UnityEngine.Object.DestroyImmediate(theModule.properties[i], true);
			}
			UnityEngine.Object.DestroyImmediate(theModule, true);
			AssetDatabase.SaveAssets();
			ownerBlueprint.ownerEmitter.SoftReset();
		}

		// MOVE UP SELECTED MODULE //
		//
		void MoveUpSelectedModule()
		{
			int selectedIndex = modules.FindIndex(delegate(BaseModule bm) { return bm == selectedModule; });
			if (selectedIndex != -1 && selectedIndex > 0)
			{
				BaseModule tempModule = modules[selectedIndex - 1];
				modules[selectedIndex - 1] = selectedModule;
				modules[selectedIndex] = tempModule;
			}
			ownerBlueprint.ownerEmitter.SoftReset();
		}

		// MOVE DOWN SELECTED MODULE //
		//
		void MoveDownSelectedModule()
		{
			int selectedIndex = modules.FindIndex(delegate(BaseModule bm) { return bm == selectedModule; });
			if (selectedIndex != -1 && selectedIndex < modules.Count - 1)
			{
				BaseModule tempModule = modules[selectedIndex + 1];
				modules[selectedIndex + 1] = selectedModule;
				modules[selectedIndex] = tempModule;
			}
			ownerBlueprint.ownerEmitter.SoftReset();
		}

		// REMOVE SELECTED MODULE //
		//
		// Called from a GenericMenu in ShowModules().
		public void RemoveSelectedModule()
		{
			int selectedIndex = modules.FindIndex(delegate(BaseModule bm) { return bm == selectedModule; });
			DeleteModuleFromAsset(selectedModule);
			selectedModule.Delete();
			modules.Remove(selectedModule);
			if (modules.Count > 0)
			{
				if (selectedIndex >= modules.Count) selectedModule = modules[selectedIndex - 1];
				else selectedModule = modules[selectedIndex];
			}
			else selectedModule = null;
			ownerBlueprint.ownerEmitter.SoftReset();
		}

		// DUPLICATE SELECTED MODULE //
		//
		// Called from a GenericMenu in ShowModules().
		public void DuplicateSelectedModule()
		{
			int selectedIndex = modules.FindIndex(delegate(BaseModule bm) { return bm == selectedModule; });

			if (ownerBlueprint.ownerEmitter.selectedStack.stackFunction == AmpsHelpers.eStackFunction.MultiFunction)
			{
				BaseMultiFunctionModule duplicateModule = ScriptableObject.CreateInstance(selectedModule.GetType()) as BaseMultiFunctionModule;
				duplicateModule.Initialize(this, ownerBlueprint);
				duplicateModule.CopyProperties(selectedModule);
				modules.Insert(selectedIndex + 1, duplicateModule);
			}
			else
			{
				BaseModule duplicateModule = ScriptableObject.CreateInstance(selectedModule.GetType()) as BaseGenericModule;
				duplicateModule.Initialize(this, ownerBlueprint);
				duplicateModule.CopyProperties(selectedModule);
				modules.Insert(selectedIndex + 1, duplicateModule);
			}
			selectedModule = modules[selectedIndex + 1];
			ownerBlueprint.ownerEmitter.SoftReset();
		}

#endregion

#endif

		// GET EXAMPLE VALUE //
		//
		public string GetExampleValue(int exampleParticleIndex)
		{
			string returnValue = "";

			switch (stackFunction)
			{
				case AmpsHelpers.eStackFunction.Emitter:
					break;
				case AmpsHelpers.eStackFunction.Render:
					break;
				case AmpsHelpers.eStackFunction.SpawnRate:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.spawnRateStack.value.ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.DeathCondition:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.deathConditionStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.DeathDuration:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.deathDurationStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.CustomScalar:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.customScalarStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.CustomVector:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.customVectorStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.Acceleration:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.accelerationStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.Velocity:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.velocityStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.Position:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.positionStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.RotationRate:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.rotationRateStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.Rotation:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.rotationStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.Scale:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.scaleStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.Color:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.colorStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.PivotOffset:
					returnValue = ownerBlueprint.ownerEmitter.blueprint.pivotOffsetStack.values[exampleParticleIndex].ToString("F3");
					break;
				case AmpsHelpers.eStackFunction.MultiFunction:
					break;
			}

			return returnValue;
		}
	}
}