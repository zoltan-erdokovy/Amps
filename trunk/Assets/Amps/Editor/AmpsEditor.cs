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

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using Amps;

namespace Amps
{
	[InitializeOnLoad]
	public class AmpsEditor : EditorWindow
	{
		static private AmpsEditor ampsEditorWindow;

		// Dictionaries storing grouping different module types so the Add Module button in stacks only offers
		// compatible modules.
		private Dictionary<string, System.Type> genericModulesPresent = new Dictionary<string, System.Type>();
		private Dictionary<string, System.Type> renderModulesPresent = new Dictionary<string, System.Type>();
		private Dictionary<string, System.Type> multiFunctionModulesPresent = new Dictionary<string, System.Type>();
		private List<System.Type> moduleTypes = new List<System.Type>();

		private AmpsEmitter selectedEmitter = null;

		// The different widths of the four editor columns.
		private float layoutStacksColumnWidth = 180f;
		private float layoutModulesColumnWidth = 230f;
		private float layoutPropertiesColumnWidth = 230f;
		private float layoutPickerColumnWidth = 270f;
		private float parameterPanelHeightFull = 128f;
		private float parameterPanelHeightCollapsed = 32f;

		// Helpers for scrollbars.
		private Vector2 scrollPositionStacks = Vector2.zero;
		private Vector2 scrollPositionModules = Vector2.zero;
		private Vector2 scrollPositionProperties = Vector2.zero;
		private Vector2 scrollPositionPicker = Vector2.zero;
		private Vector2 scrollPositionParameters = Vector2.zero;

		// The default AmpsEditor skin.
		// TODO: Make and use another skin for dark unity editor skin.
		private GUISkin ampsSkin;

		private UnityEngine.Object blueprintGUIObject = null;
		private bool isEditorLocked = false;
		private bool isParametersPanelOpen = false;
		private AmpsEmitter[] selfAndChildEmitters;

		private bool wasGamePlaying = false;

		// CREATE MODULE DUMP //
		void CreateModuleDump()
		{
			Amps.AmpsBlueprint theDump = ScriptableObject.CreateInstance<Amps.AmpsBlueprint>();
			theDump.Initialize();

			theDump.emitterStack.modules.Clear();	// Remove the auto-initialized emitter module.
			moduleTypes.Sort((x, y) => string.Compare(x.ToString(), y.ToString()));
			foreach (Type moduleType in moduleTypes)
			{
				Debug.Log(moduleType.ToString());
				BaseModule newModule = ScriptableObject.CreateInstance(moduleType) as BaseModule;
				newModule.Initialize(theDump.customVectorStack, theDump);
				theDump.customVectorStack.modules.Add(newModule);
			}
			SaveBlueprintToAsset(theDump, "Assets/Dump");
		}

		// PORT ASSET //
		//
		void PortAsset()
		{
			string dump1FileName = "Assets/Dump_from";
			string dump2FileName = "Assets/Dump";

			// Building remapping dictionary.
			Dictionary<string, string> remappingDict = new Dictionary<string, string>();

			StreamReader dump1Reader = new StreamReader(dump1FileName);
			string[] dump1Lines = dump1Reader.ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.None);
			StreamReader dump2Reader = new StreamReader(dump2FileName);
			string[] dump2Lines = dump2Reader.ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.None);

			for (int i = 0; i < dump1Lines.Length; i++)
			{
				// Example line:
				// m_Script: {fileID: 313073108, guid: b4c82f59e61bef74e92f25aa89b92e99, type: 3}
				if (dump1Lines[i].Contains("m_Script: {fileID:") && dump2Lines[i].Contains("m_Script: {fileID:"))
				{
					remappingDict[dump1Lines[i]] = dump2Lines[i];
				}
			}

			//foreach (var pair in remappingDict)
			//{
			//    Debug.Log(pair.Key + " -> " + pair.Value);
			//}

			string originalAssetFileName = EditorUtility.OpenFilePanel("Asset to fix", "Assets/", "asset");
			if (string.IsNullOrEmpty(originalAssetFileName) == false)
			{
				StreamReader originalAssetReader = new StreamReader(originalAssetFileName);
				string fixedAssetFileName = System.IO.Path.GetDirectoryName(originalAssetFileName) + "/" +
											System.IO.Path.GetFileNameWithoutExtension(originalAssetFileName) + "_fixed" +
											System.IO.Path.GetExtension(originalAssetFileName);
				StreamWriter fixedAssetWriter = new StreamWriter(fixedAssetFileName);
				string lineToWrite;
				string[] originalAssetLines = originalAssetReader.ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.None);

				for (int i = 0; i < originalAssetLines.Length; i++)
				{
					lineToWrite = originalAssetLines[i];
					if (remappingDict.ContainsKey(originalAssetLines[i]))
					{
						lineToWrite = remappingDict[originalAssetLines[i]];
					}

					fixedAssetWriter.WriteLine(lineToWrite);
				}

				fixedAssetWriter.Close();
			}
		}

		// SAVE BLUEPRINT TO ASSET //
		//
		// Saves an AmpsBlueprint with all the related objects to a custom asset file.
		// Previous contents will be replaced entirely.
		public void SaveBlueprintToAsset(Amps.AmpsBlueprint theBlueprint, string theAssetFilePath)
		{
			AssetDatabase.CreateAsset(theBlueprint, theAssetFilePath);
			theBlueprint.name = "Blueprint";
			List<UnityEngine.Object> subObjects = theBlueprint.GetSubObjects();
			foreach (var item in subObjects) AssetDatabase.AddObjectToAsset(item, theAssetFilePath);
			AssetDatabase.SaveAssets();
		}

		// LOAD BLUEPRINT FROM ASSET //
		//
		public void LoadBlueprintFromAsset(string theAssetFilePath)
		{
			AmpsBlueprint theBlueprint = null;

			UnityEngine.Object[] subObjects = AssetDatabase.LoadAllAssetsAtPath(theAssetFilePath);

			foreach (var o in subObjects)
			{
				if (o != null && o.GetType() == typeof(Amps.AmpsBlueprint)) theBlueprint = o as Amps.AmpsBlueprint;
			}

			if (theBlueprint != null)
			{
				selectedEmitter.blueprint = theBlueprint;
				selectedEmitter.HandleBlueprintChange();
			}
			else AmpsHelpers.AmpsWarning("Selected asset is not a blueprint.");
		}

		// DUPLICATE ASSET //
		//
		// Called from a GenericMenu in PlaceEmitterControls().
		public void DuplicateAsset()
		{
			string editedAssetFilePath = AssetDatabase.GetAssetPath(selectedEmitter.blueprint);
			if (string.IsNullOrEmpty(editedAssetFilePath) == false)
			{
				string editedAssetFileName = System.IO.Path.GetFileNameWithoutExtension(editedAssetFilePath);
				string duplicateAssetFilePath = EditorUtility.SaveFilePanel("Duplicate asset", "Assets/", editedAssetFileName + " (copy)", "asset");
				if (string.IsNullOrEmpty(duplicateAssetFilePath) == false)
				{
					AssetDatabase.CopyAsset(editedAssetFilePath, duplicateAssetFilePath);
					//LoadBlueprintFromAsset(duplicateAssetFilePath);	// BUG: Fails. The new asset file is not available right away.
				}
			}
		}

		// COMMIT TO ASSET //
		//
		public void CommitToAsset(string thePath)	// TODO: Merge this with SaveBlueprintToAsset().
		{
			AmpsBlueprint newBlueprint = ScriptableObject.CreateInstance<Amps.AmpsBlueprint>();
			newBlueprint.Initialize();
			newBlueprint.CopyBlueprint(selectedEmitter.blueprint);
			EditorUtility.SetDirty(selectedEmitter.blueprint);
			SaveBlueprintToAsset(newBlueprint, thePath);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			LoadBlueprintFromAsset(thePath);
			OnSelectionChange();	// HACK: Without this line default values are shown on commit while actual values are used/saved.
		}

		// SOFT RESET //
		//
		public void SoftReset()
		{
			AmpsEmitter[] selfAndChildEmitters = selectedEmitter.transform.root.GetComponentsInChildren<AmpsEmitter>();

			if (selfAndChildEmitters.Length > 0)
			{
				foreach (AmpsEmitter currentEmitter in selfAndChildEmitters)
				{
					if (currentEmitter.blueprint != null && currentEmitter.enabled)
					{
						currentEmitter.SoftReset();
						if (wasGamePlaying) currentEmitter.HandleBlueprintChange();	// Fixes errors when coming back from game mode.

						// Making sure the during editing the blueprint is executed
						// regardless of the emitter module's playOnAwake property.
						currentEmitter.isPlaying = true;
					}
				}
			}
		}

		// PAUSE HIERARCHY //
		//
		public void PauseHierarchy()
		{
			AmpsEmitter[] selfAndChildEmitters = selectedEmitter.transform.root.GetComponentsInChildren<AmpsEmitter>();

			if (selfAndChildEmitters.Length > 0)
			{
				foreach (AmpsEmitter currentEmitter in selfAndChildEmitters)
				{
					currentEmitter.Pause();
				}
			}
		}

		// UNPAUSE HIERARCHY //
		//
		public void UnpauseHierarchy()
		{
			AmpsEmitter[] selfAndChildEmitters = selectedEmitter.transform.root.GetComponentsInChildren<AmpsEmitter>();

			if (selfAndChildEmitters.Length > 0)
			{
				foreach (AmpsEmitter currentEmitter in selfAndChildEmitters)
				{
					currentEmitter.Unpause();
				}
			}
		}

		// SAVE SETTINGS //
		//
		void SaveSettings()
		{
			EditorPrefs.SetBool("AmpsEditor_isParametersPanelOpen", isParametersPanelOpen);
		}

		// LOAD SETTINGS //
		//
		void LoadSettings()
		{
			isParametersPanelOpen = EditorPrefs.GetBool("AmpsEditor_isParametersPanelOpen", false);
		}

//============================================================================//
#region GUI functions

		// SHOW STATS //
		//
		void ShowStats(Camera camera)
		{
			if (selectedEmitter == null || selectedEmitter.particleMarkers == null) return;

			Vector3 labelPosition = Vector3.zero;
			Vector3 cameraPosition = camera.transform.position;
			Quaternion cameraRotationQuat = camera.transform.rotation;
			float lineHeight = 16;
			float f = 0;

			float unitsPerPixel = (Mathf.Abs(Mathf.Tan(Mathf.Deg2Rad * (camera.fieldOfView / 2))) * 2) / camera.pixelHeight;
			lineHeight *= unitsPerPixel;

			labelPosition.x = 0;
			labelPosition.y = (((camera.pixelHeight / 2) * unitsPerPixel) - lineHeight) - 0.04f;
			labelPosition.z = 1;

			f = 1 / Mathf.Clamp(selectedEmitter.smoothDeltaTime, 0.001f, 2f);
			Handles.Label((cameraRotationQuat * labelPosition) + cameraPosition, f.ToString("F0") + " ups");

			labelPosition.y -= lineHeight;
			f = selectedEmitter.particleMarkers.ActiveCount;
			Handles.Label((cameraRotationQuat * labelPosition) + cameraPosition, f.ToString("F0") + " particles");
		}

		// LOAD SKIN //
		//
		void LoadSkin()
		{
			ampsSkin = AssetDatabase.LoadAssetAtPath("Assets/Amps/Gui/AmpsSkin.guiskin", typeof(GUISkin)) as GUISkin;
		}

		// GET COLUMN RECT //
		//
		Rect GetColumnRect(int columnIndex)
		{
			Rect returnValue = new Rect();
			float winH;// = ampsEditorWindow.position.height - parameterPanelHeight;
			
			if (isParametersPanelOpen) winH = ampsEditorWindow.position.height - parameterPanelHeightFull;
			else winH = ampsEditorWindow.position.height - parameterPanelHeightCollapsed;

			switch (columnIndex)
			{
				case 0:
					returnValue = new Rect(0, 24, layoutStacksColumnWidth, winH - 24);
					break;
				case 1:
					returnValue = new Rect(layoutStacksColumnWidth, 24, layoutModulesColumnWidth, winH - 24);
					break;
				case 2:
					returnValue = new Rect(layoutStacksColumnWidth + layoutModulesColumnWidth, 24, layoutPropertiesColumnWidth, winH - 24);
					break;
				case 3:
					returnValue = new Rect(layoutStacksColumnWidth + layoutModulesColumnWidth + layoutPropertiesColumnWidth, 24, layoutPickerColumnWidth, winH - 24);
					break;
			}
			return returnValue;
		}

		// PLACE EMITTER CONTROLS //
		//
		void PlaceEmitterControls()
		{
			bool validEditedGO = false;
			bool validEditedComponent = false;
			bool validEditedBlueprint = false;
			bool validBlueprintGUIObject = false;
			string editedBlueprintPath = "";
			string blueprintGUIObjectPath = "";
			bool wasMenuShown = false;	// Used to skip certain operations if the context menu was shown.

			if (isEditorLocked)
			{
				validEditedComponent = selectedEmitter != null;
				if (validEditedComponent) validEditedGO = selectedEmitter.gameObject != null;
				if (validEditedComponent) validEditedBlueprint = selectedEmitter.blueprint != null;
				if (validEditedGO && validEditedComponent) editedBlueprintPath = AssetDatabase.GetAssetPath(selectedEmitter.blueprint);
				validBlueprintGUIObject = blueprintGUIObject != null;
				if (validBlueprintGUIObject) blueprintGUIObjectPath = AssetDatabase.GetAssetPath(blueprintGUIObject);
			}
			else
			{
				validEditedGO = Selection.activeGameObject != null;
				if (validEditedGO)
				{
					selectedEmitter = Selection.activeGameObject.GetComponent<AmpsEmitter>();
					validEditedComponent = selectedEmitter != null;
				}
				if (validEditedComponent) validEditedBlueprint = selectedEmitter.blueprint != null;
				else selectedEmitter = null;
				if (validEditedGO && validEditedComponent)
				{
					editedBlueprintPath = AssetDatabase.GetAssetPath(selectedEmitter.blueprint);
				}
				validBlueprintGUIObject = blueprintGUIObject != null;
				if (validBlueprintGUIObject) blueprintGUIObjectPath = AssetDatabase.GetAssetPath(blueprintGUIObject);
			}
			if (validEditedGO == false ||
					validEditedComponent == false ||
					validEditedBlueprint == false)
			{
				blueprintGUIObject = null;
			}

			GUILayout.BeginHorizontal();

			if (validEditedGO == false)
			{
				GUILayout.Label("No game object.", GUILayout.Width(layoutStacksColumnWidth));

				if (GUILayout.Button("Add Amps emitter object", GUILayout.Width(layoutModulesColumnWidth)))
				{
					GameObject newAmpsEmitterObject = new GameObject();
					newAmpsEmitterObject.name = "New Amps Emitter Object";

					// Setting GameObject icon by ArkaneX //
					Texture2D icon = (Texture2D)Resources.Load("AmpsIcon");
					if (icon != null)
					{
						var editorGUIUtilityType = typeof(EditorGUIUtility);
						var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
						var args = new object[] { newAmpsEmitterObject, icon };
						editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
					}
					///////////////////////////////////////

					Selection.activeGameObject = newAmpsEmitterObject;
					newAmpsEmitterObject.AddComponent<AmpsEmitter>();
					selectedEmitter = newAmpsEmitterObject.GetComponent<AmpsEmitter>();
					SoftReset();
					selectedEmitter.needsRepaint = true;
				}
			}

			if (validEditedGO &&
				validEditedComponent == false)
			{
				GUILayout.Label("No emitter component.", GUILayout.Width(layoutStacksColumnWidth));

				if (GUILayout.Button("Add Amps emitter component", GUILayout.Width(layoutModulesColumnWidth)))
				{
					Selection.activeGameObject.AddComponent<AmpsEmitter>();
					selectedEmitter = Selection.activeGameObject.GetComponent<AmpsEmitter>();
					SoftReset();
					selectedEmitter.needsRepaint = true;
				}
			}

			if (validEditedGO &&
				validEditedComponent)
			{
				if (validEditedBlueprint && (editedBlueprintPath != blueprintGUIObjectPath))
				{
					// BUG? The following line returns null even for a valid path. Worked in Unity 4.5.
					blueprintGUIObject = AssetDatabase.LoadMainAssetAtPath(editedBlueprintPath);
				}

				GUILayout.BeginHorizontal(GUILayout.MinWidth(layoutStacksColumnWidth - 2));
				GUILayout.Label(selectedEmitter.gameObject.name);
				GUILayout.BeginVertical();
				GUILayout.Space(4);

				if (validEditedBlueprint == false) GUI.enabled = false;
				bool wasEditorLocked = isEditorLocked;
				isEditorLocked = GUILayout.Toggle(isEditorLocked, "", "lock");
				if (wasEditorLocked && isEditorLocked == false) OnSelectionChange();
				if (validEditedBlueprint == false) GUI.enabled = true;

				GUILayout.EndVertical();
				GUILayout.Label(":");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(layoutModulesColumnWidth-2));
				//blueprintGUIObject = EditorGUILayout.ObjectField(blueprintGUIObject, typeof(UnityEngine.Object), false);
				blueprintGUIObject = EditorGUILayout.ObjectField(blueprintGUIObject, typeof(Amps.AmpsBlueprint), false);
				GUILayout.BeginVertical();
				GUILayout.Space(4);
				if (validEditedBlueprint == false) GUI.enabled = false;
				if (GUILayout.Button("", "optionButton"))
				{
					GenericMenu moduleOptionsMenu = new GenericMenu();

					moduleOptionsMenu.AddItem(new GUIContent("Duplicate blueprint"),
												false,
												DuplicateAsset);
					moduleOptionsMenu.AddItem(new GUIContent("Create module dump"),
												false,
												CreateModuleDump);
					moduleOptionsMenu.AddItem(new GUIContent("Port an asset"),
												false,
												PortAsset);
					moduleOptionsMenu.ShowAsContext();
					wasMenuShown = true;
				}
				if (validEditedBlueprint == false) GUI.enabled = true;

				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(layoutPropertiesColumnWidth-2));
				if (GUILayout.Button("Restart", GUILayout.ExpandWidth(true)))
				{
					SoftReset();
					selectedEmitter.needsRepaint = true;
				}
				if (GUILayout.Button("Pause", GUILayout.ExpandWidth(true)))
				{
					if (selectedEmitter.isPlaying) selectedEmitter.Pause();
					else selectedEmitter.Unpause();
				}
				if (GUILayout.Button("Pause all", GUILayout.ExpandWidth(true)))
				{
					if (selectedEmitter.isPlaying) PauseHierarchy();
					else UnpauseHierarchy();
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Commit", GUILayout.ExpandWidth(false)))
				{
					CommitToAsset(editedBlueprintPath);
				}
				GUILayout.Space(8);
				GUILayout.Label(AmpsHelpers.version, "propertyGroupLabel");	// Showing the version number.
				GUILayout.Space(8);
				GUILayout.EndHorizontal();

				if (wasMenuShown == false)
				{
					// Re-evaluate related variables.
					validBlueprintGUIObject = blueprintGUIObject != null;
					if (validBlueprintGUIObject) blueprintGUIObjectPath = AssetDatabase.GetAssetPath(blueprintGUIObject);
					else blueprintGUIObjectPath = "";

					// Now we push back changes to the edited blueprint.
					if (validBlueprintGUIObject == false && validEditedBlueprint)
					{
						// HACK: We don't push back null refs as a workaround for the suspected LoadMainAssetAtPath bug in line ~437.
						//selectedEmitter.blueprint = null;
						//selectedEmitter.HandleBlueprintChange();
					}
					else if (validBlueprintGUIObject && editedBlueprintPath != blueprintGUIObjectPath)
					{
						LoadBlueprintFromAsset(blueprintGUIObjectPath);
					}
				}
			}

			GUILayout.EndHorizontal();
		}

		// PLACE STACKS COLUMN //
		//
		void PlaceStacksColumn()
		{
			GUILayout.BeginArea(GetColumnRect(0));
			GUILayout.BeginVertical("column");

			GUILayout.BeginHorizontal("column");
			GUILayout.FlexibleSpace();
			GUILayout.Label("Stacks", "columnLabel");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			scrollPositionStacks = GUILayout.BeginScrollView(scrollPositionStacks);

			selectedEmitter.ShowStacks();
			if (selectedEmitter.needsRepaint)
			{
				Repaint();
				selectedEmitter.needsRepaint = false;
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		// PLACE MODULES COLUMN //
		//
		void PlaceModulesColumn()
		{
			bool shouldRepaint = false;

			GUILayout.BeginArea(GetColumnRect(1));
			GUILayout.BeginVertical("column");

			GUILayout.BeginHorizontal("column");
			GUILayout.FlexibleSpace();
			GUILayout.Label("Modules", "columnLabel");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			scrollPositionModules = GUILayout.BeginScrollView(scrollPositionModules);

			selectedEmitter.selectedStack.ShowModules(out shouldRepaint);
			//selectedEmitter.blueprint.emitterStack.ShowModules(out shouldRepaint);

			// TODO: Could/should the AddModuleMenu be moved to BaseStack.ShowModules()?
			if (selectedEmitter.selectedStack.moduleType != typeof(BaseEmitterModule).ToString())
			{
				AddModuleMenu();
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();

			if (shouldRepaint) Repaint();
		}

		// PLACE PROPERTIES COLUMN //
		//
		void PlacePropertiesColumn()
		{
			bool shouldRepaint = false;

			GUILayout.BeginArea(GetColumnRect(2));
			GUILayout.BeginVertical("column");

			GUILayout.BeginHorizontal("column");
			GUILayout.FlexibleSpace();
			GUILayout.Label("Properties", "columnLabel");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			scrollPositionProperties = GUILayout.BeginScrollView(scrollPositionProperties);

			if (selectedEmitter.selectedStack.selectedModule != null)
			{
				selectedEmitter.selectedStack.selectedModule.ShowProperties(ref shouldRepaint);
			}

			if (shouldRepaint) Repaint();

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		// PLACE PICKER COLUMN //
		//
		void PlacePickerColumn()
		{
			GUILayout.BeginArea(GetColumnRect(3));
			GUILayout.BeginVertical("column");

			GUILayout.BeginHorizontal("column");
			GUILayout.FlexibleSpace();
			GUILayout.Label("Value picker", "columnLabel");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			scrollPositionPicker = GUILayout.BeginScrollView(scrollPositionPicker);

			if (selectedEmitter.selectedStack.selectedModule != null)
			{
				if (selectedEmitter.selectedStack.selectedModule.selectedProperty == null) selectedEmitter.selectedStack.selectedModule.selectedProperty = selectedEmitter.selectedStack.selectedModule.moduleName;
				selectedEmitter.selectedStack.selectedModule.selectedProperty.ShowPicker();
				if (selectedEmitter.selectedStack.selectedModule.selectedProperty.shouldRepaintPicker)
				{
					Repaint();
				}
			}

			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		// PLACE PARAMETER EDITOR //
		//
		void PlaceParameterEditor()
		{
			float winH = ampsEditorWindow.position.height;
			float winW = ampsEditorWindow.position.width;

			if (isParametersPanelOpen)
			{
				GUILayout.BeginArea(new Rect(0, winH - parameterPanelHeightFull, winW, parameterPanelHeightFull));
			}
			else 
			{
				GUILayout.BeginArea(new Rect(0, winH - parameterPanelHeightCollapsed, winW, parameterPanelHeightCollapsed));
			}

			GUILayout.BeginVertical("column");
			GUILayout.BeginHorizontal("column");
			GUILayout.FlexibleSpace();
			isParametersPanelOpen = GUILayout.Toggle(isParametersPanelOpen, "", "toggle");
			GUILayout.Label("Parameters", "columnLabel");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if (isParametersPanelOpen)
			{
				scrollPositionParameters = GUILayout.BeginScrollView(scrollPositionParameters, false, false);
				selectedEmitter.parameters.RemoveAll(param => param.shouldBeRemoved);
				foreach (BaseParameter currentParameter in selectedEmitter.parameters)
				{
					currentParameter.ShowParameter();
				}
				AddParameterMenu();
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		// ADD GENERIC MODULE MENU CALLBACK //
		//
		void AddGenericModuleMenuCallback(object item)
		{
			System.Type moduleType = genericModulesPresent[item as string];

			BaseGenericModule newModule = ScriptableObject.CreateInstance(moduleType) as BaseGenericModule;
			newModule.Initialize(selectedEmitter.selectedStack, selectedEmitter.blueprint);
			selectedEmitter.selectedStack.modules.Add(newModule);
			selectedEmitter.selectedStack.selectedModule = newModule;
			if (selectedEmitter.selectedStack.stackFunction == AmpsHelpers.eStackFunction.Shared)
			{
				selectedEmitter.UpdateSharedPropertyList();
			}
		}

		// ADD SHARED MODULE MENU CALLBACK //
		//
		void AddSharedModuleMenuCallback(object item)
		{
			BaseGenericModule theSharedModule = item as BaseGenericModule;

			if (theSharedModule != null)
			{
				BaseGenericModule newModule = ScriptableObject.CreateInstance(theSharedModule.GetType()) as BaseGenericModule;
				newModule.Initialize(selectedEmitter.selectedStack, selectedEmitter.blueprint);
				newModule.ReferenceProperties(theSharedModule);
				selectedEmitter.selectedStack.modules.Add(newModule);
				selectedEmitter.selectedStack.selectedModule = newModule;
			}
		}

		// ADD RENDER MODULE MENU CALLBACK //
		//
		void AddRenderModuleMenuCallback(object item)
		{
			System.Type moduleType = renderModulesPresent[item as string];

			BaseRenderModule newModule = ScriptableObject.CreateInstance(moduleType) as BaseRenderModule;
			newModule.Initialize(selectedEmitter.selectedStack, selectedEmitter.blueprint);
			selectedEmitter.selectedStack.modules.Add(newModule);
			selectedEmitter.selectedStack.selectedModule = newModule;
		}

		// ADD MULTI FUNCTION MODULE MENU CALLBACK //
		//
		void AddMultiFunctionModuleMenuCallback(object item)
		{
			System.Type moduleType = multiFunctionModulesPresent[item as string];

			BaseMultiFunctionModule newModule = ScriptableObject.CreateInstance(moduleType) as BaseMultiFunctionModule;
			newModule.Initialize(selectedEmitter.selectedStack, selectedEmitter.blueprint);
			selectedEmitter.selectedStack.modules.Add(newModule);
			selectedEmitter.selectedStack.selectedModule = newModule;
		}

		// ADD MODULE MENU //
		//
		void AddModuleMenu()
		{
			GenericMenu addModuleMenu = new GenericMenu();
			string buttonLabel = "(+) Add a module";

			if (selectedEmitter.selectedStack.stackFunction == AmpsHelpers.eStackFunction.Shared)
			{
				buttonLabel = "(+) Add a shared module";
			}

			GUILayout.BeginVertical("moduleAddBox");
			if (GUILayout.Button(buttonLabel, "moduleAddButton"))
			{
				switch (selectedEmitter.selectedStack.moduleType)
				{
					case "Amps.BaseGenericModule":
						foreach (var m in genericModulesPresent.OrderBy(i => i.Key))
						{
							addModuleMenu.AddItem(new GUIContent(m.Key),
													false,
													AddGenericModuleMenuCallback,
													m.Key);
						}
						if (selectedEmitter.sharedProperties.Count > 0)
						{
							addModuleMenu.AddSeparator("");
							if (selectedEmitter.selectedStack.stackFunction != AmpsHelpers.eStackFunction.Shared)
							{
								foreach (PropertyReference pr in selectedEmitter.sharedProperties)
								{
									// HACK: Exploiting the fact that the same item can be added multiple times but it will
									// only show up once.
									addModuleMenu.AddItem(new GUIContent("Shared/" + pr.module.moduleName.GetValue()),
														false,
														AddSharedModuleMenuCallback,
														pr.module);
								}
							}
						}
						break;

					case "Amps.BaseRenderModule":
						foreach (var m in renderModulesPresent.OrderBy(i => i.Key))
						{
							addModuleMenu.AddItem(new GUIContent(m.Key),
													false,
													AddRenderModuleMenuCallback,
													m.Key);
						}
						break;

					case "Amps.BaseMultiFunctionModule":
						foreach (var m in multiFunctionModulesPresent.OrderBy(i => i.Key))
						{
							addModuleMenu.AddItem(new GUIContent(m.Key),
													false,
													AddMultiFunctionModuleMenuCallback,
													m.Key);
						}
						break;
				}

				addModuleMenu.ShowAsContext();
			}
			GUILayout.EndVertical();
		}

		// ADD PARAMETER MENU CALLBACK //
		//
		void AddParameterMenuCallback(object item)
		{
			BaseParameter theNewParameter = item as BaseParameter;

			selectedEmitter.parameters.Add(theNewParameter);
		}

		// ADD PARAMETER MENU //
		//
		void AddParameterMenu()
		{
			GenericMenu addParameterMenu = new GenericMenu();
			string buttonLabel = "(+) Add a parameter";

			GUILayout.BeginVertical("moduleAddBox");
			if (GUILayout.Button(buttonLabel, "moduleAddButton"))
			{
				addParameterMenu.AddItem(new GUIContent("Bool parameter"),
										false,
										AddParameterMenuCallback,
										new BaseParameter(AmpsHelpers.eParameterTypes.Bool));
				addParameterMenu.AddItem(new GUIContent("GameObject parameter"),
										false,
										AddParameterMenuCallback,
										new BaseParameter(AmpsHelpers.eParameterTypes.GameObject));
				addParameterMenu.AddItem(new GUIContent("Material parameter"),
										false,
										AddParameterMenuCallback,
										new BaseParameter(AmpsHelpers.eParameterTypes.Material));
				addParameterMenu.AddItem(new GUIContent("Mesh parameter"),
										false,
										AddParameterMenuCallback,
										new BaseParameter(AmpsHelpers.eParameterTypes.Mesh));
				addParameterMenu.AddItem(new GUIContent("Scalar parameter"),
										false,
										AddParameterMenuCallback,
										new BaseParameter(AmpsHelpers.eParameterTypes.Scalar));
				addParameterMenu.AddItem(new GUIContent("Vector parameter"),
										false,
										AddParameterMenuCallback,
										new BaseParameter(AmpsHelpers.eParameterTypes.Vector));

				addParameterMenu.ShowAsContext();
			}
			GUILayout.EndVertical();
		}

#endregion

//============================================================================//
#region System events

		// INIT //
		//
		[MenuItem("Window/Amps Editor")]
		public static void Init()
		{
			ampsEditorWindow = (AmpsEditor)EditorWindow.GetWindow(typeof(AmpsEditor), false, "Amps Editor") as AmpsEditor;
			ampsEditorWindow.minSize = new Vector2(910f, 300f);
			ampsEditorWindow.maxSize = new Vector2(910f, 1200f);
			ampsEditorWindow.LoadSkin();

			EditorApplication.update += ampsEditorWindow.Update;

			SceneView.onSceneGUIDelegate -= ampsEditorWindow.OnSceneGUI;	// HACK: Unsubscribe blindly since there is no way of telling if we've subscribed already.
			SceneView.onSceneGUIDelegate += ampsEditorWindow.OnSceneGUI;

			// Getting assembly containing module classes.
			var targetAssembly = Assembly.GetExecutingAssembly();
			System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			foreach (var a in assemblies)
			{
				//#if AMPS_DLL
				//if (a.FullName.Contains("AmpsDLL")) targetAssembly = a;
				//#else
				if (a.FullName.Contains("Assembly-CSharp,")) targetAssembly = a;
				//#endif
			}

			VectorArrayStack tempStack = new VectorArrayStack();
			tempStack.stackFunction = AmpsHelpers.eStackFunction.CustomVector;
			System.Collections.Generic.IEnumerable<System.Type> subtypes;

			// Registering Emitter modules.
			ampsEditorWindow.moduleTypes.Add(typeof(BaseEmitterModule));

			// Registering Generic modules.
			subtypes = targetAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseGenericModule)));
			foreach (var mt in subtypes)
			{
				BaseGenericModule gm = ScriptableObject.CreateInstance(mt) as BaseGenericModule;
				gm.Initialize(tempStack, null);

				if (String.IsNullOrEmpty(gm.type) == false)
				{
					string smn = gm.subMenuName;
					if (smn != "") { smn += "/"; }
					ampsEditorWindow.genericModulesPresent.Add(smn + gm.type, mt);
					ampsEditorWindow.moduleTypes.Add(mt);
				}
			}
			//Debug.Log(ampsEditorWindow.genericModulesPresent.Count + " Generic modules.");

			// Registering Render modules.
			subtypes = targetAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseRenderModule)));
			foreach (var mt in subtypes)
			{
				BaseRenderModule rm = ScriptableObject.CreateInstance(mt) as BaseRenderModule;
				rm.Initialize(tempStack, null);
				if (String.IsNullOrEmpty(rm.type) == false)
				{
					string smn = rm.subMenuName;
					if (smn != "") { smn += "/"; }
					ampsEditorWindow.renderModulesPresent.Add(smn + rm.type, mt);
					ampsEditorWindow.moduleTypes.Add(mt);
				}
			}
			//Debug.Log(ampsEditorWindow.renderModulesPresent.Count + " Render modules.");

			// Registering Multi function modules.
			subtypes = targetAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseMultiFunctionModule)));
			foreach (var mt in subtypes)
			{
				BaseMultiFunctionModule mfm = ScriptableObject.CreateInstance(mt) as BaseMultiFunctionModule;
				mfm.Initialize(tempStack, null);
				if (String.IsNullOrEmpty(mfm.type) == false)
				{
					string smn = mfm.subMenuName;
					if (smn != "") { smn += "/"; }
					ampsEditorWindow.multiFunctionModulesPresent.Add(smn + mfm.type, mt);
					ampsEditorWindow.moduleTypes.Add(mt);
				}
			}
			//Debug.Log(ampsEditorWindow.multiFunctionModulesPresent.Count + " Multi-function modules.");

			if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent("AmpsEmitter") != null)
			{
				ampsEditorWindow.selectedEmitter = Selection.activeGameObject.GetComponent("AmpsEmitter") as AmpsEmitter;
				if (ampsEditorWindow.selectedEmitter.blueprint != null)
				{
					ampsEditorWindow.selectedEmitter.selectedStack = ampsEditorWindow.selectedEmitter.blueprint.emitterStack;
					ampsEditorWindow.selectedEmitter.selectedStack.selectedModule = ampsEditorWindow.selectedEmitter.selectedStack.modules[0];
					ampsEditorWindow.selectedEmitter.emitterModule = ampsEditorWindow.selectedEmitter.blueprint.emitterStack.modules[0] as Amps.BaseEmitterModule;
					ampsEditorWindow.selectedEmitter.HandleBlueprintChange();
				}
			}

			ampsEditorWindow.LoadSettings();
		}

		// ON SELECTION CHANGE //
		//
		void OnSelectionChange()
		{
			if (selectedEmitter != null && selectedEmitter.blueprint != null)
			{
				selectedEmitter.selectedStack = selectedEmitter.blueprint.emitterStack;
				selectedEmitter.selectedStack.selectedModule = selectedEmitter.selectedStack.modules[0];
				selectedEmitter.emitterModule = selectedEmitter.blueprint.emitterStack.modules[0] as Amps.BaseEmitterModule;
			}

			if (Selection.gameObjects.Length > 0 && isEditorLocked == false && Application.isPlaying == false)
			{
				selectedEmitter = Selection.gameObjects[0].GetComponent<AmpsEmitter>();
				if (selectedEmitter != null && selectedEmitter.blueprint != null)
				{
					selfAndChildEmitters = null;	// To be updated in Update().
					selectedEmitter.HandleBlueprintChange();
				}
			}
			Repaint();
		}

		// ON GUI //
		//
		void OnGUI()
		{
			if (ampsEditorWindow == null)
			{
				Init();
			}

			if (ampsSkin != null)
			{
				GUI.skin = ampsSkin;

				GUILayout.BeginVertical();

				if (Application.isPlaying)
				{
					GUILayout.FlexibleSpace();
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					GUILayout.Label("(The game is running.)", GUILayout.Width(layoutStacksColumnWidth));
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					GUILayout.FlexibleSpace();
				}
				else
				{
					PlaceEmitterControls();
					if (selectedEmitter != null && selectedEmitter.blueprint != null)
					{
						// Deleting or replacing the used blueprint asset makes the emitter module invalid so we need to check and fix.
						if (selectedEmitter.emitterModule == null) selectedEmitter.SoftReset();
						GUILayout.BeginHorizontal();
						PlaceStacksColumn();
						PlaceModulesColumn();
						PlacePropertiesColumn();
						PlacePickerColumn();
						GUILayout.EndHorizontal();
					}

					if (selectedEmitter != null) PlaceParameterEditor();
				}

				GUILayout.EndVertical();
			}
			else AmpsHelpers.AmpsWarning("ampsSkin is missing!");
		}

		// ON SCENE GUI //
		//
		public void OnSceneGUI(SceneView view)
		{
			if (Application.isPlaying && wasGamePlaying == false) wasGamePlaying = true;
			if (Application.isPlaying == false && wasGamePlaying)
			{
				SoftReset();
				wasGamePlaying = false;
			}

			if (selectedEmitter != null && Application.isPlaying == false)
			{
				if (selectedEmitter.blueprint != null && selectedEmitter.blueprint.ownerEmitter != selectedEmitter) selectedEmitter.blueprint.ownerEmitter = selectedEmitter;

				foreach (BaseModule m in selectedEmitter.visualizedModules)
				{
					if (m.isVisualizationEnabled) m.ShowVisualization();
				}

				if (view.m_RenderMode != DrawCameraMode.Wireframe)
				{
					AmpsEmitter[] selfAndChildEmitters = selectedEmitter.transform.root.GetComponentsInChildren<AmpsEmitter>();

					for (int i = 0; i < selfAndChildEmitters.Length; i++)
					{
						if (selfAndChildEmitters[i].blueprint != null)
						{
							// We have to do this manually since the emitter's OnWillRenderObject() is only called in-game.
							selfAndChildEmitters[i].lastRenderTime = selfAndChildEmitters[i].currentTime;
							selfAndChildEmitters[i].currentCameraTransform = view.camera.transform;
							selfAndChildEmitters[i].blueprint.renderStack.Evaluate();
						}
					}

					if (selectedEmitter.blueprint != null) ShowStats(view.camera);	// Show stats of the selected emitter only.
				}
			}
		}

		// GET ACTIVE SCENE VIEW //
		//
		public static SceneView GetActiveSceneView()
		{
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof(SceneView))
			{
				return (SceneView)EditorWindow.focusedWindow;
			}
			ArrayList temp = SceneView.sceneViews;

			return (SceneView)temp[0];
		}

		// UPDATE //
		//
		void Update()
		{
			if (Application.isPlaying) return;	// Not to interfere with a running game.

			if (selectedEmitter == null)
			{
				if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent("AmpsEmitter") != null)
				{
					selectedEmitter = Selection.activeGameObject.GetComponent("AmpsEmitter") as AmpsEmitter;
				}
			}
			
			if (selectedEmitter != null)
			{
				if (selfAndChildEmitters == null) selfAndChildEmitters = selectedEmitter.transform.root.GetComponentsInChildren<AmpsEmitter>();
				foreach (AmpsEmitter currentEmitter in selfAndChildEmitters)
				{
					if (currentEmitter.blueprint != null && currentEmitter.enabled)
					{
						// isEmitterDataReset is NOT serialized so it will be false on scene load.
						// Deleting or replacing the used blueprint asset makes the emitter module invalid so we need to check and fix.
						if (currentEmitter.isEmitterDataReset == false || currentEmitter.emitterModule == null)
						{
							currentEmitter.HandleBlueprintChange();
						}

						if (currentEmitter.isEditorDriven == false) currentEmitter.isEditorDriven = true;
						// We have to do this manually since the emitter's Update() is only called in-game.
						currentEmitter.DoUpdate();
					}
				}

				// TODO: Only repaint at the selectedEmitter.updateRate rate.
				SceneView.RepaintAll();
			}
		}

		// ON DESTROY //
		//
		void OnDestroy()
		{
			EditorApplication.update -= ampsEditorWindow.Update;
			SceneView.onSceneGUIDelegate -= ampsEditorWindow.OnSceneGUI;
			SaveSettings();
		}

#endregion

	}
}
#endif