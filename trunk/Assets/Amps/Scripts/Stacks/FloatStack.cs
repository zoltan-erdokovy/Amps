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