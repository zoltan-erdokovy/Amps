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
	public class BaseRenderModule : BaseModule
	{
		public MaterialProperty particleMaterial;
		[System.NonSerialized]
		public Mesh finalMesh;

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (finalMesh == null) finalMesh = new Mesh();
			else finalMesh.Clear();
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			// Do rendering.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float input)
		{
			// This one shouldn't be called.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref float[] input)
		{
			// This one shouldn't be called.
		}

		// EVALUATE //
		//
		override public void Evaluate(ref Vector4[] input)
		{
			// This one shouldn't be called.
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			particleMaterial = ScriptableObject.CreateInstance<MaterialProperty>();
			particleMaterial.Initialize("Material");
			AddProperty(particleMaterial, false);
		}

		//// COPY PROPERTIES //
		////
		//override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		//{
		//    base.CopyProperties(originalModule, theOwnerBlueprint);

		//    BaseRenderModule om = originalModule as BaseRenderModule;
		//    if (om != null)
		//    {
		//        particleMaterial.CopyProperty(om.particleMaterial, theOwnerBlueprint);
		//    }
		//}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			Material previousMaterial = particleMaterial.GetValue();
			particleMaterial.ShowProperty(ref selectedProperty, false);
			Material currentMaterial = particleMaterial.GetValue();
			if (currentMaterial != previousMaterial) ownerBlueprint.ownerEmitter.UpdateMaterials();
		}

#endregion
#endif
	}
}