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