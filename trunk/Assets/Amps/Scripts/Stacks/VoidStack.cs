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
	public class VoidStack : BaseStack
	{
#if UNITY_EDITOR
		// INITIALIZE //
		//
		override public void Initialize(AmpsBlueprint theOwnerBlueprint, AmpsHelpers.eStackFunction sf, string mt)
		{
			base.Initialize(theOwnerBlueprint, sf, mt);

			isParticleStack = false;
			isVector3Stack = false;
		}
#endif

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			if (stackFunction == AmpsHelpers.eStackFunction.Render)
			{
				int i = 0;
				CombineInstance[] subMeshes = new CombineInstance[modules.Count];

				foreach (BaseModule m in modules)
				{
					CombineInstance ci = new CombineInstance();
					if (m.isEnabled)
					{
						BaseRenderModule rm = m as BaseRenderModule;
						rm.Evaluate();
						if (rm.finalMesh != null) ci.mesh = rm.finalMesh;
						else ci.mesh = new Mesh();
					}
					else ci.mesh = new Mesh();

					subMeshes[i] = ci;
					i++;
				}

				if (ownerBlueprint.ownerEmitter.emitterMesh == null) Debug.Log("ownerBlueprint.ownerEmitter.emitterMesh == null");
				ownerBlueprint.ownerEmitter.emitterMesh.CombineMeshes(subMeshes, false, false);
			}
			else
			{
				foreach (BaseModule m in modules)
				{
					if (m.isEnabled) m.Evaluate();
				}
			}
		}
	}
}