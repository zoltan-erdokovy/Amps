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
	public class FloatStack : BaseStack
	{
		public float value;

#if UNITY_EDITOR
		// INITIALIZE //
		//
		override public void Initialize(AmpsBlueprint theOwnerBlueprint, AmpsHelpers.eStackFunction sf, string mt)
		{
			base.Initialize(theOwnerBlueprint, sf, mt);

			value = 0;
			isParticleStack = false;
			isVector3Stack = false;
		}
#endif
		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			value = 0;
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// We discard the stack's previous value.
			value = 0;

			foreach (BaseModule m in modules)
			{
				if (m.isEnabled) m.Evaluate(ref value);
			}
		}
	}
}