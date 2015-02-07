using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Text;

namespace Amps
{
	public static class AmpsHelpers
	{
		public const string version = "v0.35";

		// String constants for naming place GUI controls.
		public const string stackControlName = "stackControl";
		public const string moduleControlName = "moduleControl";
		public const string propertyControlName = "propertyControl";
		public const string pickerControlName = "pickerControl";
		public const string stringControlName = "stringControl";

		public static Matrix4x4 identityMatrix;

		// E STACK FUNCTION //
		//
		public enum eStackFunction
		{
			Emitter,
			Render,
			Shared,
			SpawnRate,
			DeathCondition,
			DeathDuration,
			CustomScalar,
			CustomVector,
			Acceleration,
			Velocity,
			Position,
			RotationRate,
			Rotation,
			Scale,
			Color,
			PivotOffset,
			MultiFunction
		}

		// E MODULE OPERATIONS //
		//
		public enum eModuleOperations
		{
			NoOperation,
			ShowOptions,
			MoveUp,
			MoveDown,
			Duplicate,
			Remove
		}

		// E PROPERTY OPERATIONS //
		//
		public enum ePropertyOperations
		{
			NoOperation,
			ShowOptions,
			Constant,
			Random,
			Curve,
			RandomCurve,
			Reference,
			Parameter
		}

		// E CURVE INPUTS //
		//
		public enum eCurveInputs
		{
			EmitterTime,
			LoopTime,
			ParticleTime,
			DeathCondition,
			DyingDuration,
			DyingTime,
			CollisionTime,
			SpawnRate,
			CustomScalar,
			FrameRate,
			EmitterPosition,
			EmitterRotation,
			EmitterForward,
			EmitterScale,
			EmitterAcceleration,
			EmitterVelocity,
			CustomVector,
			Acceleration,
			Velocity,
			Position,
			RotationRate,
			Rotation,
			Forward,
			Scale,
			Color,
			ParticleCount,
			EmitterPositionLocal,
			EmitterRotationLocal,
			NonZeroParticleCountTime
		}

		// CURVE INPUT DISPLAY DATA //
		//
		// To be used with the related enum in DropdownProperty.
		public static readonly string[] curveInputDisplayData =
		{
			"Time: Emitter",
			"Time: Loop (0..1)",
			"Time: Particle",
			"Death condition (0..1)",
			"Dying duration",
			"Time: Dying (0..1)",
			"Time: Collision (Doesn't work)",
			"Spawn rate",
			"Custom scalar",
			"Frame rate (0..1)",
			"Position: Emitter",
			"Rotation: Emitter",
			"Forward: Emitter",
			"Scale: Emitter",
			"Acceleration: Emitter",
			"Velocity: Emitter",
			"Custom vector",
			"Acceleration: Particle",
			"Velocity: Particle",
			"Position: Particle",
			"Rotation rate: Particle",
			"Rotation: Particle",
			"Forward: Particle",
			"Scale: Particle",
			"Color",
			"Particle count",
			"Position: Emitter (Local)",
			"Rotation: Emitter (Local)",
			"Time: Particles exist"
		};

		// VECTOR COMPONENTS //
		//
		public enum eVectorComponents
		{
			X,
			Y,
			Z,
			W,
			Mag
		}
		// VECTOR COMPONENTS DISPLAY DATA //
		//
		public static readonly string[] vectorComponentsDisplayData = 
		{
			"X",
			"Y",
			"Z",
			"W",
			"Mag"
		};

		// E COLOR COMPONENTS //
		//
		public enum eColorComponents
		{
			R,
			G,
			B,
			A,
			Hue,
			Sat,
			Value
		}
		// COLOR COMPONENT DISPLAY DATA //
		//
		public static readonly string[] colorComponentsDisplayData =
		{
			"R",
			"G",
			"B",
			"A",
			"Hue",
			"Sat",
			"Value"
		};

		// E COORDINATE SYSTEMS //
		//
		public enum eCoordSystems
		{
			World,
			Emitter
		}
		// COORDINATE SYSTEMS DISPLAY DATA //
		//
		public static readonly string[] coordSystemsDisplayData =
		{
			"World",
			"Emitter"
		};

		// E PARAMETER TYPES //
		//
		public enum eParameterTypes
		{
			Bool,
			Scalar,
			Vector,
			GameObject,
			Mesh,
			Material
		}
		// PARAMETER TYPES DISPLAY DATA //
		//
		public static readonly string[] parameterTypesDisplayData =
		{
			"Bool",
			"Scalar",
			"Vector",
			"GameObject",
			"Mesh",
			"Material"
		};

		// IS PARTICLE RELATED INPUT //
		//
		public static bool isParticleRelatedInput(eCurveInputs ci)
		{
			bool returnValue = true;

			if (ci == eCurveInputs.EmitterTime ||
				ci == eCurveInputs.LoopTime ||
				ci == eCurveInputs.SpawnRate ||
				ci == eCurveInputs.EmitterPosition ||
				ci == eCurveInputs.EmitterRotation ||
				ci == eCurveInputs.EmitterForward ||
				ci == eCurveInputs.EmitterScale ||
				ci == eCurveInputs.EmitterAcceleration ||
				ci == eCurveInputs.EmitterVelocity ||
				ci == eCurveInputs.FrameRate ||
				ci == eCurveInputs.ParticleCount)
			{
				returnValue = false;
			}

			return returnValue;
		}

		// IS FLOAT STACK //
		//
		public static bool isFloatStack(eStackFunction sf)
		{
			bool returnValue = false;

			if (sf == eStackFunction.CustomScalar ||
				sf == eStackFunction.DeathCondition ||
				sf == eStackFunction.DeathDuration ||
				sf == eStackFunction.SpawnRate)
			{
				returnValue = true;
			}

			return returnValue;
		}

		// IS FLOAT INPUT //
		//
		public static bool isFloatInput(eCurveInputs ci)
		{
			bool returnValue = false;

			if (ci == eCurveInputs.EmitterTime ||
				ci == eCurveInputs.LoopTime ||
				ci == eCurveInputs.ParticleTime ||
				ci == eCurveInputs.DeathCondition ||
				ci == eCurveInputs.DyingDuration ||
				ci == eCurveInputs.DyingTime ||
				ci == eCurveInputs.CollisionTime ||
				ci == eCurveInputs.SpawnRate ||
				ci == eCurveInputs.CustomScalar ||
				ci == eCurveInputs.FrameRate ||
				ci == eCurveInputs.ParticleCount ||
				ci == eCurveInputs.NonZeroParticleCountTime)
			{
				returnValue = true;
			}

			return returnValue;
		}

		// IS POSITION INPUT //
		//
		public static bool isPositionInput(eCurveInputs ci)
		{
			bool returnValue = false;

			if (ci == eCurveInputs.EmitterPosition ||
				ci == eCurveInputs.Position)
			{
				returnValue = true;
			}

			return returnValue;
		}

		// IS ROTATION INPUT //
		//
		public static bool isRotationInput(eCurveInputs ci)
		{
			bool returnValue = false;

			if (ci == eCurveInputs.EmitterRotation ||
				ci == eCurveInputs.Rotation)
			{
				returnValue = true;
			}

			return returnValue;
		}

		// IS VELOCITY INPUT //
		//
		public static bool isVelocityInput(eCurveInputs ci)
		{
			bool returnValue = false;

			if (ci == eCurveInputs.EmitterVelocity ||
				ci == eCurveInputs.EmitterAcceleration ||
				ci == eCurveInputs.Velocity ||
				ci == eCurveInputs.Acceleration)
			{
				returnValue = true;
			}

			return returnValue;
		}

		// IS SCALE INPUT //
		//
		public static bool isScaleInput(eCurveInputs ci)
		{
			bool returnValue = false;

			if (ci == eCurveInputs.EmitterScale ||
				ci == eCurveInputs.Scale)
			{
				returnValue = true;
			}

			return returnValue;
		}

		// FORMAT ENUM STRING //
		//
		public static string formatEnumString(string enumString)
		{
			StringBuilder returnValue = new StringBuilder(enumString.Length * 2);
			returnValue.Append(enumString[0]);
			for (int i = 1; i < enumString.Length; i++)
			{
				if (char.IsUpper(enumString[i]))
				{
					returnValue.Append(' ');
				}
				returnValue.Append(enumString[i]);
			}

			returnValue.Replace("Nrm", "(0-1)");
			if (returnValue[0] == '_')
			{
				returnValue.Remove(0, 1);
				returnValue.Insert(0, "(");
				returnValue.Append(" )");
			}
			returnValue.Replace("_", "-");

			return returnValue.ToString();
		}

		// CONVERT BOOL FLOAT //
		//
		public static float ConvertBoolFloat(bool input)
		{
			return input ? 1f : 0f;
		}

		// CONVERT BOOL VECTOR //
		//
		public static Vector4 ConvertBoolVector(bool input)
		{
			return input ? Vector4.one : Vector4.zero;
		}

		// CONVERT FLOAT BOOL //
		//
		public static bool ConvertFloatBool(float input)
		{
			if (input > 0.5f) return true;
			else return false;
		}

		// CONVERT FLOAT VECTOR4 //
		//
		public static Vector4 ConvertFloatVector4(float input)
		{
			return new Vector4(input, input, input, input);
		}

		// CONVERT FLOAT VECTOR3 //
		//
		public static Vector3 ConvertFloatVector3(float input)
		{
			return new Vector3(input, input, input);
		}

		// CONVERT VECTOR BOOL //
		//
		public static bool ConvertVectorBool(Vector4 input)
		{
			if (input.magnitude > 0.5f) return true;
			else return false;
		}

		// CONVERT VECTOR FLOAT //
		//
		public static float ConvertVectorFloat(Vector4 input)
		{
			return input.magnitude;
		}

		// CONVERT VECTOR4 VECTOR3 //
		//
		public static Vector3 ConvertVector4Vector3(Vector4 input)
		{
			return new Vector3(input.x, input.y, input.z);
		}

		// CONVERT VECTOR3 VECTOR4 //
		//
		public static Vector4 ConvertVector3Vector4(Vector3 input, float w)
		{
			return new Vector4(input.x, input.y, input.z, w);
		}

		// RGB TO HSV //
		//
		public static Vector3 RGBtoHSV(Color rgb)
		{
			Vector3 hsv = Vector3.zero;

			hsv.z = Mathf.Max(rgb.r, rgb.g, rgb.b);
			float m = Mathf.Min(rgb.r, rgb.g, rgb.b);
			float c = hsv.z - m;

			if (c != 0)
			{
				hsv.y = c / hsv.z;
				Vector3 Delta = (new Vector3(hsv.z, hsv.z, hsv.z) - new Vector3(rgb.r, rgb.g, rgb.b)) / c;
				Delta = Delta - new Vector3(Delta.z, Delta.x, Delta.y);
				Delta.x += 2;
				Delta.y += 4;
				if (rgb.r >= hsv.z) { hsv.x = Delta.z; }
				else if (rgb.g >= hsv.z) { hsv.x = Delta.x; }
				else hsv.x = Delta.y;
				hsv.x = hsv.x / 6;
				hsv.x = hsv.x - Mathf.Floor(hsv.x);	//Frac()
			}

			return hsv;
		}

		// RGBA TO HSVA //
		//
		public static Vector4 RGBAtoHSVA(Color rgba)
		{
			Vector3 hsv = RGBtoHSV(new Color(rgba.r, rgba.g, rgba.b));
			return new Vector4(hsv.x, hsv.y, hsv.z, rgba.a);
		}

		// HSV TO RGB //
		//
		public static Color HSVtoRGB(Vector3 hsv)
		{
			Vector3 hue = Vector3.zero;
			Vector3 v = Vector3.zero;

			hue.x = Mathf.Clamp01(Mathf.Abs(hsv.x * 6 - 3) - 1);
			hue.y = Mathf.Clamp01(2 - Mathf.Abs(hsv.x * 6 - 2));
			hue.z = Mathf.Clamp01(2 - Mathf.Abs(hsv.x * 6 - 4));

			v = ((hue - Vector3.one) * hsv.y + Vector3.one) * hsv.z;
			return new Color(v.x, v.y, v.z, 1f);
		}

		// HSVA TO RGBA //
		//
		public static Color HSVAtoRGBA(Vector4 hsva)
		{
			Color rgba = HSVtoRGB(new Vector3(hsva.x, hsva.y, hsva.z));
			rgba.a = hsva.w;
			return rgba;
		}

		// ROTATE AROUND POINT //
		//
		public static Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle)
		{
			return angle * (point - pivot) + pivot;
		}

		// GET QUATERNION FROM VECTOR4 //
		//
		public static Quaternion GetQuaternionFromVector4(Vector4 v)
		{
			return Quaternion.Euler(new Vector3(v.x, v.y, v.z));
		}

		// GET SYSTEM PROPERTY //
		//
		public static Vector4 GetSystemProperty(AmpsBlueprint theBlueprint, int particleIndex, eCurveInputs property)
		{
			float f;	// Helper variable.
			Vector4 returnValue = Vector4.zero;

			switch (property)
			{
				case AmpsHelpers.eCurveInputs.EmitterTime:
					returnValue = ConvertFloatVector4(theBlueprint.ownerEmitter.emitterTime);
					break;
				case AmpsHelpers.eCurveInputs.LoopTime:
					returnValue = ConvertFloatVector4(theBlueprint.ownerEmitter.emitterLoopTime);
					break;
				case AmpsHelpers.eCurveInputs.ParticleTime:
					if (particleIndex >= 0) returnValue = ConvertFloatVector4(theBlueprint.ownerEmitter.particleTimes[particleIndex]);
					break;
				case AmpsHelpers.eCurveInputs.DeathCondition:
					if (particleIndex >= 0) returnValue = ConvertFloatVector4(theBlueprint.deathConditionStack.values[particleIndex]);
					break;
				case AmpsHelpers.eCurveInputs.DyingDuration:
					if (particleIndex >= 0) returnValue = ConvertFloatVector4(theBlueprint.deathDurationStack.values[particleIndex]);
					break;
				case AmpsHelpers.eCurveInputs.DyingTime:
					if (particleIndex >= 0)
					{
						if (theBlueprint.deathDurationStack.values[particleIndex] > 0 && theBlueprint.ownerEmitter.particleDyingTimes[particleIndex] > 0)
						{
							f = theBlueprint.ownerEmitter.particleDyingTimes[particleIndex] - theBlueprint.ownerEmitter.particleTimes[particleIndex];
							f = 1 - (f / theBlueprint.deathDurationStack.values[particleIndex]);
							returnValue = ConvertFloatVector4(f);
						}
						else returnValue = Vector4.zero;
					}
					break;
				case AmpsHelpers.eCurveInputs.CollisionTime:
					if (particleIndex >= 0) returnValue = ConvertFloatVector4(theBlueprint.ownerEmitter.collisionTimes[particleIndex]);
					break;
				case AmpsHelpers.eCurveInputs.SpawnRate:
					returnValue = ConvertFloatVector4(theBlueprint.spawnRateStack.value);
					break;
				case AmpsHelpers.eCurveInputs.CustomScalar:
					if (particleIndex >= 0) returnValue = ConvertFloatVector4(theBlueprint.customScalarStack.values[particleIndex]);
					break;
				case AmpsHelpers.eCurveInputs.FrameRate:
					f = 1 / Mathf.Clamp(theBlueprint.ownerEmitter.smoothDeltaTime, 0.016666f, 0.06666f);	// Limiting range to 15..60 fps.
					returnValue = ConvertFloatVector4((f - 15f) / 45);	// Normalizing.
					break;
				case AmpsHelpers.eCurveInputs.EmitterPosition:
					returnValue = theBlueprint.ownerEmitter.emitterPosition;
					break;
				case AmpsHelpers.eCurveInputs.EmitterRotation:
					returnValue = theBlueprint.ownerEmitter.emitterRotation;
					break;
				case AmpsHelpers.eCurveInputs.EmitterForward:
					returnValue = theBlueprint.ownerEmitter.emitterDirection;
					break;
				case AmpsHelpers.eCurveInputs.EmitterScale:
					returnValue = theBlueprint.ownerEmitter.emitterScale;
					break;
				case AmpsHelpers.eCurveInputs.EmitterAcceleration:
					returnValue = theBlueprint.ownerEmitter.emitterAcceleration;
					break;
				case AmpsHelpers.eCurveInputs.EmitterVelocity:
					returnValue = theBlueprint.ownerEmitter.emitterVelocity;
					break;
				case AmpsHelpers.eCurveInputs.CustomVector:
					if (particleIndex >= 0) returnValue = theBlueprint.customVectorStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.Acceleration:
					if (particleIndex >= 0) returnValue = theBlueprint.accelerationStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.Velocity:
					if (particleIndex >= 0) returnValue = theBlueprint.velocityStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.Position:
					if (particleIndex >= 0) returnValue = theBlueprint.positionStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.RotationRate:
					if (particleIndex >= 0) returnValue = theBlueprint.rotationRateStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.Rotation:
					if (particleIndex >= 0) returnValue = theBlueprint.rotationStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.Forward:
					if (particleIndex >= 0) returnValue = Quaternion.Euler(AmpsHelpers.ConvertVector4Vector3(theBlueprint.rotationStack.values[particleIndex])) * Vector3.forward;
					break;
				case AmpsHelpers.eCurveInputs.Scale:
					if (particleIndex >= 0) returnValue = theBlueprint.scaleStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.Color:
					if (particleIndex >= 0) returnValue = theBlueprint.colorStack.values[particleIndex];
					break;
				case AmpsHelpers.eCurveInputs.ParticleCount:
					if (theBlueprint.ownerEmitter.particleMarkers != null) returnValue = ConvertFloatVector4(theBlueprint.ownerEmitter.particleMarkers.ActiveCount);
					break;
				case AmpsHelpers.eCurveInputs.EmitterPositionLocal:
					returnValue = theBlueprint.ownerEmitter.transform.localPosition;
					break;
				case AmpsHelpers.eCurveInputs.EmitterRotationLocal:
					returnValue = theBlueprint.ownerEmitter.transform.localRotation.eulerAngles;
					break;
				case AmpsHelpers.eCurveInputs.NonZeroParticleCountTime:
					returnValue = ConvertFloatVector4(theBlueprint.ownerEmitter.nonZeroParticleCountTime);
					break;
			}

			return returnValue;
		}

		// AMPS WARNING //
		//
		public static void AmpsWarning()
		{
			Debug.Log("[" + Time.timeSinceLevelLoad.ToString("F6") + "]");
		}

		// AMPS WARNING //
		//
		public static void AmpsWarning(string s)
		{
			Debug.Log("[" + Time.timeSinceLevelLoad.ToString("F6") + "] " + s);
		}

		// AMPS WARNING //
		//
		public static void AmpsWarning(int i)
		{
			Debug.Log("[" + Time.timeSinceLevelLoad.ToString("F6") + "] " + i.ToString());
		}

		// PACK VECTOR2 //
		//
		public static float PackVector2(Vector2 input)
		{
			const int precision = 1024;

			Vector2 output = input;
			output.x = Mathf.Floor(output.x * (precision - 1));
			output.y = Mathf.Floor(output.y * (precision - 1));
			return (output.x * precision) + output.y;

			//return Mathf.Floor(input.x * precision) + (input.y - Mathf.Floor(input.y));
		}

		// UNPACK VECTOR2 //
		//
		public static Vector2 UnpackVector2(float input)
		{
			const int precision = 1024;

			Vector2 output = Vector2.zero;
			output.y = input % precision;
			output.x = Mathf.Floor(input / precision);
			return output / (precision - 1);

			//output.x = Mathf.Floor(input / precision);
			//output.y = input - Mathf.Floor(input);
			//return output;
		}

//============================================================================//
#if UNITY_EDITOR

//============================================================================//
#region GUI

		// DRAW CROSS HANDLE //
		//
		public static void DrawCrossHandle(Vector4 position, float size)
		{
			Vector3 v = new Vector3(position.x, position.y, position.z);

			Handles.DrawLine(v - new Vector3(size, 0, 0), v + new Vector3(size, 0, 0));
			Handles.DrawLine(v - new Vector3(0, size, 0), v + new Vector3(0, size, 0));
			Handles.DrawLine(v - new Vector3(0, 0, size), v + new Vector3(0, 0, size));
		}

		// DRAW POSITION HANDLE //
		//
		public static void DrawPositionHandle(Vector4 position, float size, float weight)
		{
			Vector3 v = new Vector3(position.x, position.y, position.z);

			Handles.DrawLine(v - new Vector3(size, 0, 0), v + new Vector3(size, 0, 0));
			Handles.DrawLine(v - new Vector3(0, size, 0), v + new Vector3(0, size, 0));
			Handles.DrawLine(v - new Vector3(0, 0, size), v + new Vector3(0, 0, size));

			Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, Handles.color.a * 0.2f);
			Handles.DrawSolidArc(v, Vector3.up, Vector3.forward, weight * 360, size);
		}

		// DRAW SAMPLING INDICATOR //
		//
		public static void DrawSamplingIndicator(Vector4 position, float size, float timeSinceLastSample)
		{
			Vector3 v = new Vector3(position.x, position.y, position.z);

			if (timeSinceLastSample < 0.4f) Handles.DotCap(1, v, Quaternion.identity, size);
			else Handles.RectangleCap(1, v, Quaternion.Euler(90, 0, 0), size);
		}

#endregion

#endif
	}
}