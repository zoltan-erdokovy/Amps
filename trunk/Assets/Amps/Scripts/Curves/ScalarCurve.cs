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
	public class ScalarCurve : BaseCurve
	{
		public float outputRangeMin = 0;
		public float outputRangeMax = 1;

		public AnimationCurve curve;

		// INITIALIZE //
		//
		public void Initialize(float f, string theName)
		{
			curve = new AnimationCurve(new Keyframe(0, f));
			name = theName;

			Randomize();
		}

		// INITIALIZE //
		//
		public void Initialize(ScalarCurve originalCurve)
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
			outputRangeMin = originalCurve.outputRangeMin;
			outputRangeMax = originalCurve.outputRangeMax;

			curve = new AnimationCurve(originalCurve.curve.keys);
			curve.preWrapMode = originalCurve.curve.preWrapMode;
			curve.postWrapMode = originalCurve.curve.postWrapMode;

			name = originalCurve.name;

			Randomize();
		}

		// EVALUATE //
		//
		public float Evaluate(AmpsEmitter ownerEmitter)
		{
			return Evaluate(ownerEmitter, -1);
		}

		// EVALUATE //
		//
		public float Evaluate(AmpsEmitter ownerEmitter, int particleIndex)
		{
			float returnValue = 0;

			// If a particle related curve input was chosen but particle data is not available
			// then we just return 0 like a champ. (index == -1, typically when the owner
			// property is in an emitter property stack where particle data and index is meaningless.)
			//
			// TODO: We might need a mechanic to show warnings.
			if (AmpsHelpers.isParticleRelatedInput(curveInput) && particleIndex < 0)
			{
				return 0;
			}

			float finalCurveInput = GetCurveInput(ownerEmitter.blueprint, particleIndex);

			// Remap output.
			returnValue = Mathf.Lerp(outputRangeMin, outputRangeMax, curve.Evaluate(finalCurveInput));

			return returnValue;
		}
	}
}