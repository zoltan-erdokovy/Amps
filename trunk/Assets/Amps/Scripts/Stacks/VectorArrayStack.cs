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
	public class VectorArrayStack : BaseStack
	{
		public Vector4[] values;
		public Vector4[] oldValues;
		public bool useOldValues;

#if UNITY_EDITOR
		// INITIALIZE //
		//
		override public void Initialize(AmpsBlueprint theOwnerBlueprint, AmpsHelpers.eStackFunction sf, string mt)
		{
			base.Initialize(theOwnerBlueprint, sf, mt);

			if (ownerBlueprint.ownerEmitter != null) values = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
			isParticleStack = true;

			if (sf == AmpsHelpers.eStackFunction.Acceleration ||
				sf == AmpsHelpers.eStackFunction.Velocity ||
				sf == AmpsHelpers.eStackFunction.Position ||
				sf == AmpsHelpers.eStackFunction.RotationRate ||
				sf == AmpsHelpers.eStackFunction.Rotation ||
				sf == AmpsHelpers.eStackFunction.Scale ||
				sf == AmpsHelpers.eStackFunction.PivotOffset)
			{
				isVector3Stack = true;
			}
			else isVector3Stack = false;
		}

		// ENABLE OLD VALUES //
		//
		public void EnableOldValues()
		{
			useOldValues = true;
			if (ownerBlueprint.ownerEmitter != null) oldValues = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
		}
#endif

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (ownerBlueprint.ownerEmitter != null)
			{
				values = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
				if (useOldValues) oldValues = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
			}
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// We discard the stack's previous values.
			values = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
			foreach (BaseModule m in modules) { if (m.isEnabled) m.Evaluate(ref values); }
		}
	}
}