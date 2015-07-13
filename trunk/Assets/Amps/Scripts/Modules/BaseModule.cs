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
	public class BaseModule : ScriptableObject
	{
		public bool isEnabled = true;
		public string type;

		#if UNITY_EDITOR
		#endif
		public AmpsBlueprint ownerBlueprint;

		#if UNITY_EDITOR
		#endif
		public BaseStack ownerStack;

		public StringProperty moduleName;	// Human description of the intended function of the module.
											// We keep it for runtime compile too for debuging.

//============================================================================//
#if UNITY_EDITOR

		public string subMenuName;				// In which submenu to show this module during AddModule.
		public BaseProperty selectedProperty;
		public Vector4 exampleInput;			// An example input stored here for visualization purposes.
		public bool implementsVisualization;	// Whether the module supports visualization.
		public bool isVisualizationEnabled;		// Whether the module has the visualization button turned on.
		Rect moduleRect;						// Where on the UI the module was drawn.
		bool shouldSelectThis;					// Whether the module should be selected. Selection change
												// involves changes in the UI so it must be delayed until
												// the next layouting pass.
		//[JsonIgnore]
		public List<BaseProperty> properties;	// Contains all properties of a module.
		//[JsonIgnore]
		public List<BaseProperty> sharedProperties;	// Contains the shared properties of a module.
#endif
//============================================================================//

		// INITIALIZE NEW PARTICLES //
		//
		virtual public void InitializeNewParticles()
		{
		}

		// EVALUATE //
		//
		// Evaluate without any particle or emitter data.
		virtual public void Evaluate()
		{
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		virtual public void Evaluate(ref float input)
		{
		}

		// EVALUATE //
		//
		// Evaluate when particle specific float data is available.
		virtual public void Evaluate(ref float[] input)
		{
		}

		// EVALUATE //
		//
		// Evaluate when particle specific vector data is available.
		virtual public void Evaluate(ref Vector4[] input)
		{
		}

		// SOFT RESET //
		//
		virtual public void SoftReset()
		{
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		virtual public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			type = "";
			subMenuName = "";
			ownerBlueprint = theOwnerBlueprint;
			ownerStack = theOwnerStack;
			shouldSelectThis = false;
			exampleInput = Vector4.zero;

			properties = new List<BaseProperty>();
			sharedProperties = new List<BaseProperty>();

			moduleName = ScriptableObject.CreateInstance<StringProperty>();
			moduleName.Initialize("Description", "");
			AddProperty(moduleName, false);
			selectedProperty = moduleName;
		}

		// COPY PROPERTIES //
		//
		virtual public void CopyProperties(BaseModule originalModule)
		{
			CopyProperties(originalModule, null);
		}

		// COPY PROPERTIES //
		//
		virtual public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		{
			name = type;

			isEnabled = originalModule.isEnabled;
			ownerStack = originalModule.ownerStack;
			if (theOwnerBlueprint != null) ownerBlueprint = theOwnerBlueprint;
			else ownerBlueprint = originalModule.ownerBlueprint;

			properties.RemoveAll(item => item == null);

			for (int i = 0; i < properties.Count; i++)
			{
				properties[i].CopyProperty(originalModule.properties[i], theOwnerBlueprint);
			}
		}

		// REFERENCE A PROPERTY //
		//
		virtual public void ReferenceAProperty(BaseProperty property, BaseModule originalModule, BaseProperty originalProperty)
		{
			if (originalModule != null)
			{
				property.dataMode = BaseProperty.eDataMode.Reference;
				property.reference = ScriptableObject.CreateInstance<PropertyReference>();
				property.reference.Initialize(originalModule, originalProperty);
			}
		}

		// REFERENCE PROPERTIES //
		//
		virtual public void ReferenceProperties(BaseModule originalModule)
		{
			isEnabled = originalModule.isEnabled;
			ownerBlueprint = originalModule.ownerBlueprint;

			for (int i = 0; i < properties.Count; i++)
			{
				if (properties[i].allowDataModeReference)
				{
					ReferenceAProperty(properties[i], originalModule, originalModule.properties[i]);
				}
				//else properties[i].CopyProperty(originalModule.properties[i], ownerBlueprint);
			}

			//if (originalModule != null)
			//{
			//    isEnabled = originalModule.isEnabled;
			//    moduleName.value = originalModule.moduleName.value + "(ref)";
			//    ownerBlueprint = originalModule.ownerBlueprint;
			//}
		}

		// DELETE //
		//
		virtual public void Delete()
		{
			for (int i = 0; i < sharedProperties.Count; i++)
			{
				sharedProperties[i].shouldBeRemoved = true;
			}
		}

		// ADD PROPERTY //
		//
		public void AddProperty(BaseProperty bp, bool isShared)
		{
			properties.Add(bp);

			if (isShared && ownerStack.stackFunction == AmpsHelpers.eStackFunction.Shared)
			{
				sharedProperties.Add(bp);
				bp.allowDataModeReference = false;
			}
		}

		// GET SHARED PROPERTIES //
		//
		virtual public List<PropertyReference> GetSharedProperties()
		{
			List<PropertyReference> returnValue = new List<PropertyReference>();

			for (int i = 0; i < sharedProperties.Count; i++)
			{
				PropertyReference pr = ScriptableObject.CreateInstance<PropertyReference>();
				pr.Initialize(this, sharedProperties[i]);
				returnValue.Add(pr);
			}

			return returnValue;
		}

		// SET DEFAULT NAME //
		//
		public void SetDefaultName()
		{
			if (moduleName != null)	// Name can be null only when quickly gathering available module types in AmpsEditor.
			{
				System.Random theRandom = new System.Random();
				moduleName.value = type + " (" + theRandom.Next(0, 999) + ")";
			}
		}

//============================================================================//
#region GUI

		// PROPERTY GROUP //
		//
		public void PropertyGroup(string name)
		{
			GUILayout.BeginHorizontal();
			if (name == "") GUILayout.Label(name, "propertyGroupLabel", GUILayout.Height(3));
			else GUILayout.Label(name, "propertyGroupLabel");
			GUILayout.EndHorizontal();
		}

		// SHOW PROPERTIES //
		//
		virtual public void ShowProperties(ref bool shouldRepaint)
		{
			moduleName.ShowProperty(ref selectedProperty, false);
		}

		// SHOW VISUALIZATION //
		//
		virtual public void ShowVisualization()
		{
			// It's here so not every module needs to implement it.
		}

		// SHOW MODULE //
		//
		virtual public void ShowModule(ref BaseModule selectedModule, out AmpsHelpers.eModuleOperations operation)
		{
			GUIStyle actualStyle;

			operation = AmpsHelpers.eModuleOperations.NoOperation;

			if (selectedModule == this) { actualStyle = GUI.skin.GetStyle("moduleBoxSelected"); }
			else { actualStyle = GUI.skin.GetStyle("moduleBoxNormal"); }

			GUILayout.BeginVertical(actualStyle);							//--------------------------------//

			GUILayout.BeginHorizontal("moduleHeader");						//----------------//
			isEnabled = GUILayout.Toggle(isEnabled, "", "toggle");
			GUILayout.FlexibleSpace();
			GUILayout.Label(type, "moduleName");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("", "optionButton"))
			{
				operation = AmpsHelpers.eModuleOperations.ShowOptions;
				selectedModule = this;
			}
			GUILayout.EndHorizontal();										//----------------\\

			GUILayout.BeginHorizontal();									//----------------//

			GUILayout.BeginVertical();										//--------//
			GUILayout.Space(8);
			GUILayout.BeginHorizontal();									//----//
			if (implementsVisualization) isVisualizationEnabled = GUILayout.Toggle(isVisualizationEnabled, "", "visualizationToggle");
			GUILayout.Label(moduleName.value, "moduleDescription");
			GUILayout.EndHorizontal();										//----\\
			GUILayout.EndVertical();										//--------\\

			GUILayout.BeginVertical("moduleSidebar", GUILayout.Width(17));	//--------//
			if (GUILayout.Button("", "moveUpButton"))
			{
				operation = AmpsHelpers.eModuleOperations.MoveUp;
				selectedModule = this;
			}
			if (GUILayout.Button("", "moveDownButton"))
			{
				operation = AmpsHelpers.eModuleOperations.MoveDown;
				selectedModule = this;
			}
			GUILayout.EndVertical();										//--------\\

			GUILayout.EndHorizontal();										//----------------\\

			GUILayout.EndVertical();										//--------------------------------\\

			// Get the rectangle which was drawn last.
			moduleRect = GUILayoutUtility.GetLastRect();
			if (Event.current.type == EventType.MouseDown && moduleRect.Contains(Event.current.mousePosition))
			{
				shouldSelectThis = true;
			}

			if (Event.current.type == EventType.Layout && shouldSelectThis)
			{
				selectedModule = this;
				shouldSelectThis = false;
			}
		}
#endregion

#endif
	}
}