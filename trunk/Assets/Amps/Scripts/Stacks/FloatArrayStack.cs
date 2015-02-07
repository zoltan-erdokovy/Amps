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
	public class FloatArrayStack : BaseStack
	{
		public float[] values;

#if UNITY_EDITOR
		// INITIALIZE //
		//
		override public void Initialize(AmpsBlueprint theOwnerBlueprint, AmpsHelpers.eStackFunction sf, string mt)
		{
			base.Initialize(theOwnerBlueprint, sf, mt);

			if (ownerBlueprint.ownerEmitter != null) values = new float[ownerBlueprint.ownerEmitter.particleIds.Length];
			isParticleStack = true;
			isVector3Stack = false;
		}
#endif

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (ownerBlueprint.ownerEmitter != null) values = new float[ownerBlueprint.ownerEmitter.particleIds.Length];
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// We discard the stack's previous values.
			values = new float[ownerBlueprint.ownerEmitter.particleIds.Length];

			foreach (BaseModule m in modules)
			{
				if (m.isEnabled) m.Evaluate(ref values);
			}
		}
	}
}