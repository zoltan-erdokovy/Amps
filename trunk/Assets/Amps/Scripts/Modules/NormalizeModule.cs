using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	[System.Serializable]
	public class NormalizeModule : BaseGenericModule
	{
		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			input = 0;
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				input[particleIndex] = 0;
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			Vector4 finalValue = Vector4.zero;

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				if (ownerStack.isVector3Stack)
				{
					Vector3 v = new Vector3(input[particleIndex].x, input[particleIndex].y, input[particleIndex].z);
					v = v.normalized;
					finalValue = AmpsHelpers.ConvertVector3Vector4(v, 0);
				}
				else
				{
					finalValue = input[particleIndex].normalized;
				}

				input[particleIndex] = Blend(input[particleIndex], finalValue, weight.GetValue(particleIndex));
			}
		}

//============================================================================//
#if UNITY_EDITOR

		// INITALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Math.ToString());
			type = "Normalize";
			SetDefaultName();
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, theOwnerBlueprint);
		//}

		//// REFERENCE PROPERTIES //
		////
		//override public void ReferenceProperties(BaseModule originalModule)
		//{
		//    base.ReferenceProperties(originalModule);
		//}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);
		}

#endregion

#endif
	}
}