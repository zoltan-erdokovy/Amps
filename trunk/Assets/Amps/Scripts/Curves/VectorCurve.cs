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
	public class VectorCurve : BaseCurve
	{
		public float outputRangeXMin = 0;
		public float outputRangeXMax = 1;
		public float outputRangeYMin = 0;
		public float outputRangeYMax = 1;
		public float outputRangeZMin = 0;
		public float outputRangeZMax = 1;
		public float outputRangeWMin = 0;
		public float outputRangeWMax = 1;

		public AnimationCurve curveX;
		public AnimationCurve curveY;
		public AnimationCurve curveZ;
		public AnimationCurve curveW;

		// INITIALIZE //
		//
		public void Initialize(Vector4 v, string theName)
		{
			curveX = new AnimationCurve(new Keyframe(0, v.x));
			curveY = new AnimationCurve(new Keyframe(0, v.y));
			curveZ = new AnimationCurve(new Keyframe(0, v.z));
			curveW = new AnimationCurve(new Keyframe(0, v.w));
			name = theName;

			Randomize();
		}

		// INITIALIZE //
		//
		public void Initialize(VectorCurve originalCurve)
		{
			curveInput = originalCurve.curveInput;
			curveInputVectorComponent = originalCurve.curveInputVectorComponent;
			curveInputColorComponent = originalCurve.curveInputColorComponent;
			curveInputCoordSystem = originalCurve.curveInputCoordSystem;
			inputRangeMin = originalCurve.inputRangeMin;
			inputRangeMax = originalCurve.inputRangeMax;
			isInputRangeRandom = originalCurve.isInputRangeRandom;
			inputRangeRandomMin = originalCurve.inputRangeRandomMin;
			inputRangeRandomMax = originalCurve.inputRangeRandomMax;
			outputRangeXMin = originalCurve.outputRangeXMin;
			outputRangeXMax = originalCurve.outputRangeXMax;
			outputRangeYMin = originalCurve.outputRangeYMin;
			outputRangeYMax = originalCurve.outputRangeYMax;
			outputRangeZMin = originalCurve.outputRangeZMin;
			outputRangeZMax = originalCurve.outputRangeZMax;
			outputRangeWMin = originalCurve.outputRangeWMin;
			outputRangeWMax = originalCurve.outputRangeWMax;

			curveX = new AnimationCurve(originalCurve.curveX.keys);
			curveX.preWrapMode = originalCurve.curveX.preWrapMode;
			curveX.postWrapMode = originalCurve.curveX.postWrapMode;

			curveY = new AnimationCurve(originalCurve.curveY.keys);
			curveY.preWrapMode = originalCurve.curveY.preWrapMode;
			curveY.postWrapMode = originalCurve.curveY.postWrapMode;

			curveZ = new AnimationCurve(originalCurve.curveZ.keys);
			curveZ.preWrapMode = originalCurve.curveZ.preWrapMode;
			curveZ.postWrapMode = originalCurve.curveZ.postWrapMode;

			curveW = new AnimationCurve(originalCurve.curveW.keys);
			curveW.preWrapMode = originalCurve.curveW.preWrapMode;
			curveW.postWrapMode = originalCurve.curveW.postWrapMode;

			name = originalCurve.name;

			Randomize();
		}

		// EVALUATE //
		//
		public Vector4 Evaluate(AmpsEmitter ownerEmitter)
		{
			return Evaluate(ownerEmitter, -1);
		}

		// EVALUATE //
		//
		public Vector4 Evaluate(AmpsEmitter ownerEmitter, int particleIndex)
		{
			Vector4 returnValue = Vector4.zero;

			// If a particle related curve input was chosen but particle data is not available
			// then we just return 0. (index == -1, typically when the owner property is in an
			// emitter property stack where particle data and index is meaningless.)
			//
			// TODO: We might need a mechanic to show warnings.
			if (AmpsHelpers.isParticleRelatedInput(curveInput) && particleIndex < 0)
			{
				return Vector4.zero;
			}

			float finalCurveInput = GetCurveInput(ownerEmitter.blueprint, particleIndex);
			// Remap output.
			returnValue.x = Mathf.Lerp(outputRangeXMin, outputRangeXMax, curveX.Evaluate(finalCurveInput));
			returnValue.y = Mathf.Lerp(outputRangeYMin, outputRangeYMax, curveY.Evaluate(finalCurveInput));
			returnValue.z = Mathf.Lerp(outputRangeZMin, outputRangeZMax, curveZ.Evaluate(finalCurveInput));
			returnValue.w = Mathf.Lerp(outputRangeWMin, outputRangeWMax, curveW.Evaluate(finalCurveInput));

			return returnValue;
		}
	}
}