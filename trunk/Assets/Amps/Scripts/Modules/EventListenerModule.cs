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
	public class EventListenerModule : BaseGenericModule
	{
		// E TRIGGER DATA MODES //
		//
		public enum eTriggerDataModes
		{
			CustomValue,
			EventData1,
			EventData2,
			EventData3,
			EventData4
		}
		// EVENT DATA MODES DISPLAY DATA //
		//
		public static readonly string[] triggerDataModesDisplayData =
		{
			"Custom value",
			"Sent property 1",
			"Sent property 2",
			"Sent property 3",
			"Sent property 4"
		};

		public StringProperty eventName;
		public VectorProperty untriggeredValue;
		public VectorProperty triggeredCustomValue;
		public DropdownProperty triggerDataMode;
		public BoolProperty infiniteTriggerCount;
		public ScalarProperty maxTriggerCount;
		private int currentTriggerCount = 0;
		private float previousLoopTime = 1;
		public BoolProperty triggerToggle;
		public ScalarProperty triggerDuration;
		private float lastTriggerTime = 0;

		private bool isTriggered;
		private EventData lastEventData;

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			currentTriggerCount = 0;
			previousLoopTime = 1;
			lastTriggerTime = 0;

			isTriggered = false;
			lastEventData = null;
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			Vector4 finalValue = untriggeredValue.GetValue();

			if (ownerBlueprint.ownerEmitter.emitterLoopTime < previousLoopTime) currentTriggerCount = 0;
			previousLoopTime = ownerBlueprint.ownerEmitter.emitterLoopTime;
			if (triggerToggle.GetValue() == false &&
				ownerBlueprint.ownerEmitter.emitterTime > lastTriggerTime + triggerDuration.GetValue()) isTriggered = false;

			if (isTriggered)
			{
				switch ((eTriggerDataModes) triggerDataMode.GetValue())
				{
					case eTriggerDataModes.CustomValue:
						finalValue = triggeredCustomValue.GetValue();
						break;
					case eTriggerDataModes.EventData1:
						finalValue = lastEventData.DataSlot1;
						break;
					case eTriggerDataModes.EventData2:
						finalValue = lastEventData.DataSlot2;
						break;
					case eTriggerDataModes.EventData3:
						finalValue = lastEventData.DataSlot3;
						break;
					case eTriggerDataModes.EventData4:
						finalValue = lastEventData.DataSlot4;
						break;
				}
			}

			input = Blend(input, finalValue, weight.GetValue());
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			Vector4 finalValue = untriggeredValue.GetValue();

			if (ownerBlueprint.ownerEmitter.emitterLoopTime < previousLoopTime) currentTriggerCount = 0;
			previousLoopTime = ownerBlueprint.ownerEmitter.emitterLoopTime;
			if (triggerToggle.GetValue() == false &&
				ownerBlueprint.ownerEmitter.emitterTime > lastTriggerTime + triggerDuration.GetValue()) isTriggered = false;

			if (isTriggered)
			{
				switch ((eTriggerDataModes)triggerDataMode.GetValue())
				{
					case eTriggerDataModes.CustomValue:
						finalValue = triggeredCustomValue.GetValue();
						break;
					case eTriggerDataModes.EventData1:
						finalValue = lastEventData.DataSlot1;
						break;
					case eTriggerDataModes.EventData2:
						finalValue = lastEventData.DataSlot2;
						break;
					case eTriggerDataModes.EventData3:
						finalValue = lastEventData.DataSlot3;
						break;
					case eTriggerDataModes.EventData4:
						finalValue = lastEventData.DataSlot4;
						break;
				}
			}

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				input[particleIndex] = Blend(input[particleIndex], finalValue, weight.GetValue(particleIndex));
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			Vector4 finalValue = untriggeredValue.GetValue();

			if (ownerBlueprint.ownerEmitter.emitterLoopTime < previousLoopTime) currentTriggerCount = 0;
			previousLoopTime = ownerBlueprint.ownerEmitter.emitterLoopTime;
			if (triggerToggle.GetValue() == false &&
				ownerBlueprint.ownerEmitter.emitterTime > lastTriggerTime + triggerDuration.GetValue()) isTriggered = false;

			if (isTriggered)
			{
				switch ((eTriggerDataModes)triggerDataMode.GetValue())
				{
					case eTriggerDataModes.CustomValue:
						finalValue = triggeredCustomValue.GetValue();
						break;
					case eTriggerDataModes.EventData1:
						finalValue = lastEventData.DataSlot1;
						break;
					case eTriggerDataModes.EventData2:
						finalValue = lastEventData.DataSlot2;
						break;
					case eTriggerDataModes.EventData3:
						finalValue = lastEventData.DataSlot3;
						break;
					case eTriggerDataModes.EventData4:
						finalValue = lastEventData.DataSlot4;
						break;
				}
			}

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				input[particleIndex] = Blend(input[particleIndex], finalValue, weight.GetValue(particleIndex));
			}
		}

		// HANDLE EVENT //
		//
		public void HandleEvent(EventData theEventData)
		{
			bool isTriggerCountFine = infiniteTriggerCount.GetValue() ||
									  (infiniteTriggerCount.GetValue() == false &&
									   currentTriggerCount < maxTriggerCount.GetValue());

			if (theEventData.eventName == eventName.GetValue() &&
				isTriggerCountFine)
			{
				if (triggerToggle.GetValue()) isTriggered = isTriggered == false;
				else isTriggered = true;

				lastEventData = theEventData;
				lastTriggerTime = ownerBlueprint.ownerEmitter.emitterTime;
				currentTriggerCount++;
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
			type = "Event listener";
			SetDefaultName();

			eventName = ScriptableObject.CreateInstance<StringProperty>();
			eventName.Initialize("Event name", theOwnerBlueprint);
			eventName.value = "event";
			eventName.SetDataModes(true, false, false, false, false, false);
			AddProperty(eventName, false);

			untriggeredValue = ScriptableObject.CreateInstance<VectorProperty>();
			untriggeredValue.Initialize("Untriggered value", theOwnerBlueprint);
			AddProperty(untriggeredValue, false);

			triggeredCustomValue = ScriptableObject.CreateInstance<VectorProperty>();
			triggeredCustomValue.Initialize("Triggered value", theOwnerBlueprint);
			triggeredCustomValue.constant = new Vector4(1, 0, 0, 0);
			AddProperty(triggeredCustomValue, false);

			triggerDataMode = ScriptableObject.CreateInstance<DropdownProperty>();
			triggerDataMode.Initialize("Trigger data mode", 0, theOwnerBlueprint);
			triggerDataMode.SetDataModes(true, false, false, false, false, false);
			AddProperty(triggerDataMode, false);

			infiniteTriggerCount = ScriptableObject.CreateInstance<BoolProperty>();
			infiniteTriggerCount.Initialize("Infinite trigger count?", theOwnerBlueprint);
			infiniteTriggerCount.value = true;
			infiniteTriggerCount.SetDataModes(true, false, false, false, false, false);
			AddProperty(infiniteTriggerCount, false);

			maxTriggerCount = ScriptableObject.CreateInstance<ScalarProperty>();
			maxTriggerCount.Initialize("Max trigger count per loop", 1f, theOwnerBlueprint);
			maxTriggerCount.SetDataModes(true, false, false, false, false, false);
			maxTriggerCount.isInteger = true;
			AddProperty(maxTriggerCount, false);

			triggerToggle = ScriptableObject.CreateInstance<BoolProperty>();
			triggerToggle.Initialize("Does an event toggle?", theOwnerBlueprint);
			triggerToggle.value = true;
			triggerToggle.SetDataModes(true, false, false, false, false, false);
			AddProperty(triggerToggle, false);

			triggerDuration = ScriptableObject.CreateInstance<ScalarProperty>();
			triggerDuration.Initialize("Trigger duration", 1f, theOwnerBlueprint);
			triggerDuration.SetDataModes(true, true, false, false, false, false);
			AddProperty(triggerDuration, false);
		}
//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			eventName.ShowProperty(ref selectedProperty, false);
			untriggeredValue.ShowProperty(ref selectedProperty, false);

			if (triggerDataMode.displayData == null) triggerDataMode.displayData = () => triggerDataModesDisplayData; // We have to do this here because delegates are not serialized.
			triggerDataMode.ShowProperty(ref selectedProperty, false);

			if ((eTriggerDataModes)triggerDataMode.GetValue() == eTriggerDataModes.CustomValue)
			{
				triggeredCustomValue.ShowProperty(ref selectedProperty, false);
			}

			infiniteTriggerCount.ShowProperty(ref selectedProperty, false);
			if (infiniteTriggerCount.GetValue() == false)
			{
				maxTriggerCount.ShowProperty(ref selectedProperty, false);
			}

			triggerToggle.ShowProperty(ref selectedProperty, false);
			if (triggerToggle.GetValue() == false)
			{
				triggerDuration.ShowProperty(ref selectedProperty, false);
			}
		}
#endregion
#endif
	}
}