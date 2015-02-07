using UnityEngine;
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public abstract class BaseCurve : ScriptableObject
	{
		public AmpsHelpers.eCurveInputs curveInput = AmpsHelpers.eCurveInputs.EmitterTime;
		public AmpsHelpers.eVectorComponents curveInputVectorComponent = AmpsHelpers.eVectorComponents.X;
		public AmpsHelpers.eColorComponents curveInputColorComponent = AmpsHelpers.eColorComponents.R;
		public AmpsHelpers.eCoordSystems curveInputCoordSystem = AmpsHelpers.eCoordSystems.World;
		public float inputRangeMin = 0;
		public float inputRangeMax = 1;
		public bool isInputRangeRandom = false;
		public float inputRangeRandomMin = 0;
		public float inputRangeRandomMax = 1;
		public int randomSeed = 0;				// A curve specific random seed;

		// GET CURVE INPUT //
		//
		public float GetCurveInput(AmpsBlueprint ownerBlueprint, int particleIndex)
		{
			float returnValue = 0;
			Vector3 v3 = Vector3.zero;
			Vector4 v4 = Vector4.zero;
			//Matrix4x4 toMatrix = AmpsHelpers.identityMatrix; // Slightly faster than Matrix4x4.identity;

			v4 = AmpsHelpers.GetSystemProperty(ownerBlueprint, particleIndex, curveInput);

			// The only need to work with conversion to emitter space as everything
			// is world relative by default.
			if (curveInputCoordSystem == AmpsHelpers.eCoordSystems.Emitter)
			{
				//toMatrix = ownerBlueprint.ownerEmitter.emitterMatrixFull;
				if (AmpsHelpers.isPositionInput(curveInput))
				{
					v3 = AmpsHelpers.ConvertVector4Vector3(v4);
					v3 -= ownerBlueprint.ownerEmitter.transform.position;
					v3 = AmpsHelpers.RotateAroundPoint(v3, ownerBlueprint.ownerEmitter.emitterPosition, ownerBlueprint.ownerEmitter.transform.rotation);
					v4 = AmpsHelpers.ConvertVector3Vector4(v3, 0);
				}
				else if (AmpsHelpers.isRotationInput(curveInput))
				{
					v3 = AmpsHelpers.ConvertVector4Vector3(v4);
					v3 += ownerBlueprint.ownerEmitter.transform.rotation.eulerAngles;
					v4 = AmpsHelpers.ConvertVector3Vector4(v3, 0);
				}
				else if (AmpsHelpers.isVelocityInput(curveInput))
				{
					v3 = AmpsHelpers.ConvertVector4Vector3(v4);
					v3 += ownerBlueprint.ownerEmitter.emitterMatrixPositionZero.MultiplyPoint3x4(v3);
					v4 = AmpsHelpers.ConvertVector3Vector4(v3, 0);
				}
				else if (AmpsHelpers.isScaleInput(curveInput))
				{
					v3 = AmpsHelpers.ConvertVector4Vector3(v4);
					v3.x *= ownerBlueprint.ownerEmitter.transform.lossyScale.x;
					v3.y *= ownerBlueprint.ownerEmitter.transform.lossyScale.y;
					v3.z *= ownerBlueprint.ownerEmitter.transform.lossyScale.z;
					v4 = AmpsHelpers.ConvertVector3Vector4(v3, 0);
				}
			}

			if (AmpsHelpers.isFloatInput(curveInput))
			{
				returnValue = v4.x;
			}
			else
			{
				//if (AmpsHelpers.isPositionInput(curveInput)) v = toMatrix.MultiplyPoint3x4(v);
				//else v = toMatrix.MultiplyVector(v);

				switch (curveInputVectorComponent)
				{
					case AmpsHelpers.eVectorComponents.X:
						returnValue = v4.x;
						break;
					case AmpsHelpers.eVectorComponents.Y:
						returnValue = v4.y;
						break;
					case AmpsHelpers.eVectorComponents.Z:
						returnValue = v4.z;
						break;
					case AmpsHelpers.eVectorComponents.W:
						returnValue = v4.w;
						break;
					case AmpsHelpers.eVectorComponents.Mag:
						returnValue = new Vector3(v4.x, v4.y, v4.z).magnitude;
						break;
				}
				// TODO: Handle Color.
			}

			// Normalize input.
			float finalInputRangeMin = inputRangeMin;
			float finalInputRangeMax = inputRangeMax;
			if (isInputRangeRandom)
			{
				System.Random theRandom = new System.Random(ownerBlueprint.ownerEmitter.randomSeed + randomSeed + ownerBlueprint.ownerEmitter.particleIds[particleIndex]);
				finalInputRangeMin = Mathf.Lerp(inputRangeMin, inputRangeRandomMin, (float)theRandom.NextDouble());
				finalInputRangeMax = Mathf.Lerp(inputRangeMax, inputRangeRandomMax, (float)theRandom.NextDouble());
			}

			if (finalInputRangeMax - finalInputRangeMin == 0) returnValue = 0;
			else returnValue = (returnValue - finalInputRangeMin) / (finalInputRangeMax - finalInputRangeMin);

			return returnValue;
		}

		// RANDOMIZE //
		//
		public void Randomize()
		{
			System.Random theRandom = new System.Random();
			randomSeed = theRandom.Next();
		}

	}
}