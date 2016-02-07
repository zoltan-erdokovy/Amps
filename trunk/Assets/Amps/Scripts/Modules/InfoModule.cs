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
	// This module shows the current value of the stack.
	[System.Serializable]
	public class InfoModule : BaseGenericModule
	{

		#if UNITY_EDITOR
		public StringProperty label;
		public BoolProperty showCurrentStack;
		public DropdownProperty property;
		#endif

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// This one shouldn't be called.
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			#if UNITY_EDITOR
			if (showCurrentStack.GetValue()) label.value = input.ToString("F3");
			else
			{
				if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex < 0) return;
				Vector4 v = AmpsHelpers.GetSystemProperty(ownerBlueprint, ownerBlueprint.ownerEmitter.exampleInputParticleIndex, (AmpsHelpers.eCurveInputs)property.GetValue());
				label.value = v.x.ToString("F3") + ", " +
								v.y.ToString("F3") + ", " +
								v.z.ToString("F3") + ", " +
								v.w.ToString("F3");
			}
			moduleName.value = label.value;
			#endif
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			#if UNITY_EDITOR
			if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex < 0) return;

			if (showCurrentStack.GetValue())
			{
				label.value = input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex].ToString("F3");
			}
			else
			{
				Vector4 v = AmpsHelpers.GetSystemProperty(ownerBlueprint, ownerBlueprint.ownerEmitter.exampleInputParticleIndex, (AmpsHelpers.eCurveInputs)property.GetValue());
				label.value = v.x.ToString("F3") + ", " +
								v.y.ToString("F3") + ", " +
								v.z.ToString("F3") + ", " +
								v.w.ToString("F3");
			}
			moduleName.value = label.value;
			#endif
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			#if UNITY_EDITOR
			Vector4 v;
			if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex < 0) return;

			if (showCurrentStack.GetValue()) v = input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex];
			else v = AmpsHelpers.GetSystemProperty(ownerBlueprint, ownerBlueprint.ownerEmitter.exampleInputParticleIndex, (AmpsHelpers.eCurveInputs)property.GetValue());

			label.value = v.x.ToString("F3") + ", " +
								v.y.ToString("F3") + ", " +
								v.z.ToString("F3") + ", " +
								v.w.ToString("F3");
			moduleName.value = label.value;
			#endif
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Misc.ToString());
			type = "Info";
			SetDefaultName();

			label = ScriptableObject.CreateInstance<StringProperty>();
			label.Initialize("=", theOwnerBlueprint);
			label.value = "n.a.";
			AddProperty(label, false);

			showCurrentStack = ScriptableObject.CreateInstance<BoolProperty>();
			showCurrentStack.Initialize("Show current stack value?", theOwnerBlueprint);
			showCurrentStack.value = true;
			showCurrentStack.SetDataModes(true, false, false, false, false, false);
			AddProperty(showCurrentStack, false);

			property = ScriptableObject.CreateInstance<DropdownProperty>();
			property.Initialize("Property", 0, theOwnerBlueprint);
			AddProperty(property, true);
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, theOwnerBlueprint);
		//}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			showCurrentStack.ShowProperty(ref selectedProperty, false);
			if (showCurrentStack.GetValue() == false)
			{
				if (property.displayData == null) property.displayData = () => AmpsHelpers.curveInputDisplayData; // We have to do this here because delegates are not serialized.
				property.ShowProperty(ref selectedProperty, false);
			}
			label.ShowProperty(ref selectedProperty, true);

			shouldRepaint = true;
		}
#endregion
#endif
	}
}