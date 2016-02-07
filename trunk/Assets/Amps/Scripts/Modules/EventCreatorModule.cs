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
	public class EventCreatorModule : BaseGenericModule
	{
		// E CONDITIONS //
		//
		public enum eConditions
		{
			Greater,
			Less,
			Equal,
			NotEqual
		}
		// CONDITIONS DISPLAY DATA //
		//
		public static readonly string[] conditionsDisplayData =
		{
			"is greater than",
			"is less than",
			"is equal to",
			"is not equal to"
		};

		public StringProperty eventName;
		public ScalarProperty value;
		public DropdownProperty condition;
		public BoolProperty useCurrentStack;
		public DropdownProperty property;
		public DropdownProperty propertyVectorComponent;
		public ScalarProperty maxEventCount;
		private int currentEventCount = 0;
		public ScalarProperty minEventDelay;
		private float timeOfLastEvent = 0;		// Time of last event in "Emitter time".
		private float previousLoopTime = 1;
		public DropdownProperty sentProperty1;
		public DropdownProperty sentProperty2;
		public DropdownProperty sentProperty3;
		public DropdownProperty sentProperty4;

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			currentEventCount = 0;
			timeOfLastEvent = -1;
			previousLoopTime = 1;
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			if (ownerBlueprint.ownerEmitter.emitterLoopTime < previousLoopTime) currentEventCount = 0;
			previousLoopTime = ownerBlueprint.ownerEmitter.emitterLoopTime;

			if (currentEventCount < maxEventCount.GetValue() &&
				ownerBlueprint.ownerEmitter.emitterTime > timeOfLastEvent + minEventDelay.GetValue())
			{
				ManageEvent(new Vector4(input, 0, 0, 0), true, -1);
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			if (ownerBlueprint.ownerEmitter.emitterLoopTime < previousLoopTime) currentEventCount = 0;
			previousLoopTime = ownerBlueprint.ownerEmitter.emitterLoopTime;

			if (ownerBlueprint.ownerEmitter.emitterTime > timeOfLastEvent + minEventDelay.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					if (currentEventCount < maxEventCount.GetValue())
					{
						ManageEvent(new Vector4(input[particleIndex], 0, 0, 0), true, particleIndex);
					}
					else break;
				}
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			if (ownerBlueprint.ownerEmitter.emitterLoopTime < previousLoopTime) currentEventCount = 0;
			previousLoopTime = ownerBlueprint.ownerEmitter.emitterLoopTime;

			if (ownerBlueprint.ownerEmitter.emitterTime > timeOfLastEvent + minEventDelay.GetValue())
			{
				int particleIndex;
				for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
				{
					particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
					if (currentEventCount < maxEventCount.GetValue())
					{
						ManageEvent(input[particleIndex], false, particleIndex);
					}
					else break;
				}
			}
		}

		// MANAGE EVENT //
		//
		void ManageEvent(Vector4 input, bool isInputFloat, int particleIndex)
		{
			bool shouldTriggerEvent = false;
			Vector4 rawProperty;
			bool isRawPropertyFloat = isInputFloat;
			float thePropertyValue = 0;

			if (useCurrentStack.GetValue() == false)
			{
				rawProperty = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, (AmpsHelpers.eCurveInputs)property.GetValue());
				isRawPropertyFloat = AmpsHelpers.isFloatInput((AmpsHelpers.eCurveInputs)property.GetValue());
			}
			else rawProperty = input;

			if (isRawPropertyFloat) thePropertyValue = rawProperty.x;
			else
			{
				switch (propertyVectorComponent.GetValue())
				{
					case (int)AmpsHelpers.eVectorComponents.X:
						thePropertyValue = rawProperty.x;
						break;
					case (int)AmpsHelpers.eVectorComponents.Y:
						thePropertyValue = rawProperty.y;
						break;
					case (int)AmpsHelpers.eVectorComponents.Z:
						thePropertyValue = rawProperty.z;
						break;
					case (int)AmpsHelpers.eVectorComponents.W:
						thePropertyValue = rawProperty.w;
						break;
					case (int)AmpsHelpers.eVectorComponents.Mag:
						thePropertyValue = new Vector3(rawProperty.x, rawProperty.y, rawProperty.z).magnitude;
						break;
				}
			}

			switch (condition.GetValue())
			{
				case (int)eConditions.Greater:
					shouldTriggerEvent = thePropertyValue > value.GetValue();
					break;
				case (int)eConditions.Less:
					shouldTriggerEvent = thePropertyValue < value.GetValue();
					break;
				case (int)eConditions.Equal:
					shouldTriggerEvent = thePropertyValue == value.GetValue();
					break;
				case (int)eConditions.NotEqual:
					shouldTriggerEvent = thePropertyValue != value.GetValue();
					break;
			}

			if (shouldTriggerEvent)
			{
				EventData theEventData = new EventData();
				theEventData.eventName = eventName.GetValue();
				theEventData.particleIndex = particleIndex;
				theEventData.DataSlot1 = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, (AmpsHelpers.eCurveInputs)sentProperty1.GetValue());
				theEventData.DataSlot2 = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, (AmpsHelpers.eCurveInputs)sentProperty2.GetValue());
				theEventData.DataSlot3 = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, (AmpsHelpers.eCurveInputs)sentProperty3.GetValue());
				theEventData.DataSlot4 = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, (AmpsHelpers.eCurveInputs)sentProperty4.GetValue());

				#if UNITY_EDITOR
				AmpsEmitter[] selfAndChildEmitters = ownerBlueprint.ownerEmitter.transform.root.GetComponentsInChildren<AmpsEmitter>();
				for (int i = 0; i < selfAndChildEmitters.Length; i++) selfAndChildEmitters[i].AmpsHandleEvent(theEventData);
				#else
				ownerBlueprint.ownerEmitter.BroadcastMessage("AmpsHandleEvent", theEventData, SendMessageOptions.DontRequireReceiver);
				#endif

				currentEventCount++;
				timeOfLastEvent = ownerBlueprint.ownerEmitter.emitterTime;
			}
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Misc.ToString());
			type = "Event creator";
			SetDefaultName();

			eventName = ScriptableObject.CreateInstance<StringProperty>();
			eventName.Initialize("Event name", theOwnerBlueprint);
			eventName.value = "event";
			eventName.SetDataModes(true, false, false, false, false, false);
			AddProperty(eventName, false);

			value = ScriptableObject.CreateInstance<ScalarProperty>();
			value.Initialize("Value", 0f, theOwnerBlueprint);
			value.SetDataModes(true, false, false, false, false, false);
			AddProperty(value, false);

			condition = ScriptableObject.CreateInstance<DropdownProperty>();
			condition.Initialize("Condition", 0, theOwnerBlueprint);
			condition.SetDataModes(true, false, false, false, false, false);
			AddProperty(condition, false);

			useCurrentStack = ScriptableObject.CreateInstance<BoolProperty>();
			useCurrentStack.Initialize("Current stack?", theOwnerBlueprint);
			useCurrentStack.value = true;
			useCurrentStack.SetDataModes(true, false, false, false, false, false);
			AddProperty(useCurrentStack, false);

			property = ScriptableObject.CreateInstance<DropdownProperty>();
			property.Initialize("Property", 0, theOwnerBlueprint);
			property.SetDataModes(true, false, false, false, false, false);
			AddProperty(property, false);

			propertyVectorComponent = ScriptableObject.CreateInstance<DropdownProperty>();
			propertyVectorComponent.Initialize("Component", 0, theOwnerBlueprint);
			propertyVectorComponent.SetDataModes(true, false, false, false, false, false);
			AddProperty(propertyVectorComponent, false);

			maxEventCount = ScriptableObject.CreateInstance<ScalarProperty>();
			maxEventCount.Initialize("Max event count per loop", 1f, theOwnerBlueprint);
			maxEventCount.SetDataModes(true, false, false, false, false, false);
			maxEventCount.isInteger = true;
			AddProperty(maxEventCount, false);

			minEventDelay = ScriptableObject.CreateInstance<ScalarProperty>();
			minEventDelay.Initialize("Min event delay", 0.1f, theOwnerBlueprint);
			minEventDelay.SetDataModes(true, false, false, false, false, false);
			AddProperty(minEventDelay, false);

			sentProperty1 = ScriptableObject.CreateInstance<DropdownProperty>();
			sentProperty1.Initialize("Sent property 1", 0, theOwnerBlueprint);
			sentProperty1.SetDataModes(true, false, false, false, false, false);
			AddProperty(sentProperty1, false);

			sentProperty2 = ScriptableObject.CreateInstance<DropdownProperty>();
			sentProperty2.Initialize("Sent property 2", 0, theOwnerBlueprint);
			sentProperty2.SetDataModes(true, false, false, false, false, false);
			AddProperty(sentProperty2, false);

			sentProperty3 = ScriptableObject.CreateInstance<DropdownProperty>();
			sentProperty3.Initialize("Sent property 3", 0, theOwnerBlueprint);
			sentProperty3.SetDataModes(true, false, false, false, false, false);
			AddProperty(sentProperty3, false);

			sentProperty4 = ScriptableObject.CreateInstance<DropdownProperty>();
			sentProperty4.Initialize("Sent property 4", 0, theOwnerBlueprint);
			sentProperty4.SetDataModes(true, false, false, false, false, false);
			AddProperty(sentProperty4, false);
		}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			moduleName.ShowProperty(ref selectedProperty, false);
			eventName.ShowProperty(ref selectedProperty, false);
			
			useCurrentStack.ShowProperty(ref selectedProperty, false);
			if (useCurrentStack.GetValue() == false)
			{
				if (property.displayData == null) property.displayData = () => AmpsHelpers.curveInputDisplayData; // We have to do this here because delegates are not serialized.
				property.ShowProperty(ref selectedProperty, false);
			}

			if ((useCurrentStack.GetValue() == false &&
				 AmpsHelpers.isFloatInput((AmpsHelpers.eCurveInputs)property.GetValue()) == false)
				||
				(useCurrentStack.GetValue() &&
				 AmpsHelpers.isFloatStack(ownerStack.stackFunction) == false))
			{
				if (propertyVectorComponent.displayData == null) propertyVectorComponent.displayData = () => AmpsHelpers.vectorComponentsDisplayData; // We have to do this here because delegates are not serialized.
				propertyVectorComponent.ShowProperty(ref selectedProperty, false);
			}

			if (condition.displayData == null) condition.displayData = () => conditionsDisplayData; // We have to do this here because delegates are not serialized.
			condition.ShowProperty(ref selectedProperty, false);
			value.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Constraints");

			maxEventCount.ShowProperty(ref selectedProperty, false);
			minEventDelay.ShowProperty(ref selectedProperty, false);

			PropertyGroup("Extra data");

			if (sentProperty1.displayData == null) sentProperty1.displayData = () => AmpsHelpers.curveInputDisplayData; // We have to do this here because delegates are not serialized.
			sentProperty1.ShowProperty(ref selectedProperty, false);
			if (sentProperty2.displayData == null) sentProperty2.displayData = () => AmpsHelpers.curveInputDisplayData; // We have to do this here because delegates are not serialized.
			sentProperty2.ShowProperty(ref selectedProperty, false);
			if (sentProperty3.displayData == null) sentProperty3.displayData = () => AmpsHelpers.curveInputDisplayData; // We have to do this here because delegates are not serialized.
			sentProperty3.ShowProperty(ref selectedProperty, false);
			if (sentProperty4.displayData == null) sentProperty4.displayData = () => AmpsHelpers.curveInputDisplayData; // We have to do this here because delegates are not serialized.
			sentProperty4.ShowProperty(ref selectedProperty, false);

			shouldRepaint = true;
		}
#endregion
#endif
	}
}