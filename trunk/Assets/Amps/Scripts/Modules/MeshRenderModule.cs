using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace Amps
{
	[System.Serializable]
	public class MeshRenderModule : BaseRenderModule
	{
		public ScalarProperty inputMeshCount;
		public MeshProperty[] inputMeshes;

		private int particleCount;
		private int lastParticleCount;
		private int[] meshIndices;		// Which mesh is associated with a particle.
		private Mesh[] meshCache;		// A cache of mesh objects, one for each particle.

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			meshIndices = new int[ownerBlueprint.ownerEmitter.particleIds.Length];
			meshCache = new Mesh[ownerBlueprint.ownerEmitter.particleIds.Length];
			for (int i = 0; i < meshIndices.Length; i++)
			{
				meshIndices[i] = -1;
				meshCache[i] = new Mesh();
			}
			
		}

		// INITIALIZE NEW PARTICLES //
		//
		override public void InitializeNewParticles()
		{
			System.Random theRandom = new System.Random();

			if (ownerBlueprint.ownerEmitter.newParticleIndices.Count > 0)
			{
				for (int i = 0; i < ownerBlueprint.ownerEmitter.newParticleIndices.Count; i++)
				{
					//meshIndices[i] = Random.Range(0, inputMeshes.Length);
					meshIndices[i] = theRandom.Next(0, inputMeshes.Length);
				}
			}
		}

		// EVALUATE //
		//
		override public void Evaluate()
		{
			bool shouldRecreateMesh;
			Vector3 position;
			Vector3 pivotOffset;
			Vector3 rotation;
			Quaternion quaternionRotation;
			Vector3 scale;
			Color color;
			System.Random theRandom = new System.Random();

			InitializeNewParticles();

			if (ownerBlueprint.ownerEmitter.particleMarkers == null) return;		// To fix null ref log spam after script rebuild.

			ownerBlueprint.ownerEmitter.activeParticleIndices = ownerBlueprint.ownerEmitter.particleMarkers.GetActiveIndices();
			particleCount = ownerBlueprint.ownerEmitter.activeParticleIndices.Length;
			shouldRecreateMesh = particleCount != lastParticleCount;

			if (shouldRecreateMesh)
			{
				ownerBlueprint.ownerEmitter.emitterMesh.Clear();
				lastParticleCount = particleCount;
			}

			CombineInstance[] combine = new CombineInstance[particleCount];
			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				position = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.ownerEmitter.blueprint.positionStack.values[particleIndex]);	// Raw position stack result.
				position = AmpsHelpers.RotateAroundPoint(position, ownerBlueprint.ownerEmitter.transform.position, Quaternion.Inverse(ownerBlueprint.ownerEmitter.transform.rotation));
				position -= ownerBlueprint.ownerEmitter.emitterPosition;	// Compensating for emitter position, turning position world relative.

				pivotOffset = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.pivotOffsetStack.values[particleIndex]);

				rotation = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.rotationStack.values[particleIndex]); // Raw particle stack result.
				quaternionRotation = Quaternion.Inverse(ownerBlueprint.ownerEmitter.transform.rotation) * Quaternion.Euler(rotation);

				scale = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.scaleStack.values[particleIndex]);
				color = ownerBlueprint.colorStack.values[particleIndex];

				if (meshIndices[particleIndex] < 0) meshIndices[particleIndex] = theRandom.Next(0, inputMeshes.Length);
				Mesh pickedMesh = inputMeshes[meshIndices[particleIndex]].GetValue();

				meshCache[particleIndex].Clear();
				combine[i].mesh = meshCache[particleIndex];
				if (pickedMesh != null)
				{
					combine[i].mesh.vertices = pickedMesh.vertices;
					combine[i].mesh.normals = pickedMesh.normals;
					combine[i].mesh.uv = pickedMesh.uv;
					combine[i].mesh.triangles = pickedMesh.triangles;
					combine[i].mesh.tangents = pickedMesh.tangents;

					Color[] vertexColors = new Color[pickedMesh.vertices.Length];
					for (int j = 0; j < pickedMesh.vertices.Length; j++) vertexColors[j] = color;
					combine[i].mesh.colors = vertexColors;

					combine[i].transform = Matrix4x4.TRS(position + pivotOffset, quaternionRotation, scale);
				}
				else
				{
					combine[i].transform = new Matrix4x4();
				}

				if (shouldRecreateMesh)	// Things go in here which don't change unless particle count changes.
				{
				}
			}

			finalMesh.CombineMeshes(combine, true, true);
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = "";
			type = "Mesh renderer";
			SetDefaultName();

			inputMeshCount = ScriptableObject.CreateInstance<ScalarProperty>();
			inputMeshCount.Initialize("Mesh count", 1, theOwnerBlueprint);
			inputMeshCount.SetDataModes(true, false, false, false, false, false);
			inputMeshCount.isInteger = true;
			AddProperty(inputMeshCount, false);
			inputMeshes = new MeshProperty[1];
			inputMeshes[0] = ScriptableObject.CreateInstance<MeshProperty>();
			inputMeshes[0].Initialize("Mesh 0");
			// We don't add mesh[0] to the property array because we will handle
			// the dynamic array of meshes in our own CopyProperties().

			//SoftReset();	// To initialize the private variables.
		}

		// COPY PROPERTIES //
		//
		override public void CopyProperties(BaseModule originalModule, AmpsBlueprint theOwnerBlueprint)
		{
			base.CopyProperties(originalModule, theOwnerBlueprint);

			MeshRenderModule om = originalModule as MeshRenderModule;
			if (om != null)
			{
				//meshCount.CopyProperty(om.meshCount, theOwnerBlueprint);
				inputMeshes = new MeshProperty[om.inputMeshes.Length];

				for (int i = 0; i < om.inputMeshes.Length; i++)
				{
					inputMeshes[i] = ScriptableObject.CreateInstance<MeshProperty>();
					inputMeshes[i].Initialize("Mesh " + i);
					AddProperty(inputMeshes[i], false);
					inputMeshes[i].CopyProperty(om.inputMeshes[i], theOwnerBlueprint);
				}
			}
		}

//============================================================================//
#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);
			BaseProperty previousSelection = selectedProperty;
			PropertyGroup("");

			inputMeshCount.constant = inputMeshes.Length;
			inputMeshCount.ShowProperty(ref selectedProperty, false);
			if (inputMeshCount.GetValue() > 0 && inputMeshCount.GetValue() != inputMeshes.Length)
			{
				MeshProperty[] newMeshes = new MeshProperty[(int)inputMeshCount.GetValue()];

				for (int i = 0; i < newMeshes.Length; i++)
				{
					if (i <= inputMeshes.Length - 1)	newMeshes[i] = inputMeshes[i];
					else
					{
						newMeshes[i] = ScriptableObject.CreateInstance<MeshProperty>();
						newMeshes[i].Initialize("Mesh " + i);
						AddProperty(newMeshes[i], false);
					}
					
				}
				inputMeshes = newMeshes;
			}

			foreach (MeshProperty m in inputMeshes)
			{
				m.ShowProperty(ref selectedProperty, false);
			}
			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
#endregion
#endif
	}
}