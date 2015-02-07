using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class MeshSamplerModule : BaseSamplerModule
	{
		public enum eMeshElements
		{
			VertexPosition,
			VertexNormal,
			TrianglePosition,
			TriangleNormal,
			TriangleRotation
		}
		public readonly string[] meshElementsDisplayData =
		{
			"Vertex position",
			"Vertex normal",
			"Triangle position",
			"Triangle normal",
			"Triangle rotation"
		};

		public enum eSamplingOrder
		{
			Sequential,
			Random,
			IndexBased
		}
		public readonly string[] samplingOrderDisplayData =
		{
			"Sequential",
			"Random",
			"Index based"
		};


		public MeshProperty sampledMesh;
		public DropdownProperty sampledMeshElement;
		public DropdownProperty samplingOrder;
		public BoolProperty isEmitterRelative;
		private Vector4[] vectors;			// The actually generated data for each particle.
		private int[] elementIndices;			// The mesh element indices assigned to each particle.
		private int lastAssignedElementIndex;
		private Mesh theMesh;

		// SOFT RESET //
		//
		override public void SoftReset()
		{
			base.SoftReset();

			if (ownerStack.isParticleStack)
			{
				vectors = new Vector4[ownerBlueprint.ownerEmitter.particleIds.Length];
				elementIndices = new int[ownerBlueprint.ownerEmitter.particleIds.Length];
				for (int i = 0; i < vectors.Length; i++)
				{
					vectors[i] = Vector4.zero;
					elementIndices[i] = 0;
				}
			}
			else
			{
				vectors = new Vector4[1];
				vectors[0] = Vector4.zero;
				elementIndices = new int[1];
				elementIndices[0] = 0;
			}

			lastAssignedElementIndex = 0;

			theMesh = sampledMesh.GetValue();
		}

		// INITIALIZE NEW PARTICLES //
		//
		override public void InitializeNewParticles()
		{
			// The base's code is duplicated bellow to save a funct call and a loop.
			// base.InitializeNewParticles();
			System.Random theRandom = new System.Random();

			if (theMesh == null) return;

			if (ownerBlueprint.ownerEmitter.newParticleIndices.Count > 0)
			{
				int elementCount = 0;
				if (sampledMeshElement.GetValue() == (int)eMeshElements.VertexNormal ||
					sampledMeshElement.GetValue() == (int)eMeshElements.VertexPosition)
				{
					elementCount = theMesh.vertices.Length;
				}
				else elementCount = theMesh.triangles.Length / 3;	// Divide by 3 because its a list of vertex indices.

				for (int i = 0; i < ownerBlueprint.ownerEmitter.newParticleIndices.Count; i++)
				{
					int theParticleIndex = ownerBlueprint.ownerEmitter.newParticleIndices[i];

					lastSampleTimes[theParticleIndex] = -1;
					isValidSample[theParticleIndex] = false;

					if (theMesh != null)
					{
						switch ((eSamplingOrder)samplingOrder.GetValue())
						{
							case eSamplingOrder.Sequential:
								elementIndices[theParticleIndex] = lastAssignedElementIndex;
								lastAssignedElementIndex++;
								if (lastAssignedElementIndex > elementCount - 1) lastAssignedElementIndex = 0;
								break;
							case eSamplingOrder.Random:
								elementIndices[theParticleIndex] = theRandom.Next(0, elementCount);
								break;
							case eSamplingOrder.IndexBased:
								if (ownerBlueprint.ownerEmitter.particleIds.Length > elementCount)
								{
									elementIndices[theParticleIndex] = theParticleIndex % elementCount;
								}
								else elementIndices[theParticleIndex] = theParticleIndex;
								break;
						}
					}
				}
			}
		}

		// MANAGE SAMPLING //
		//
		// Does the actual sampling.
		override public void ManageSampling(int particleIndex)
		{
			Vector3 v = Vector3.zero;
			bool shouldIndeedSample = (particleIndex < 0 && ShouldSample()) || (particleIndex >= 0 && ShouldSample(particleIndex));

			// Leave if it's not time yet to sample.
			if (shouldIndeedSample == false) return;

			// INVALID SAMPLE: Referenced GameObject is not available.
			if (theMesh == null)
			{
				isValidSample[particleIndex] = false;
				return;
			}

			// TODO: Regenerate element index if order is random or sequential.

			Vector3[] triangleVertexPositions = new Vector3[3];
			Vector3[] triangleVertexNormals = new Vector3[3];
			Vector3[] triangleVertexTangents = new Vector3[3];
			eMeshElements selectedMeshElement = (eMeshElements)sampledMeshElement.GetValue();

			switch (selectedMeshElement)
			{
				case eMeshElements.VertexPosition:
					v = theMesh.vertices[elementIndices[particleIndex]];
					break;
				case eMeshElements.VertexNormal:
					if (theMesh.normals.Length > 0) v = theMesh.normals[elementIndices[particleIndex]];
					else v = Vector3.zero;
					break;
				case eMeshElements.TrianglePosition:
					triangleVertexPositions[0] = theMesh.vertices[theMesh.triangles[elementIndices[particleIndex] * 3]];
					triangleVertexPositions[1] = theMesh.vertices[theMesh.triangles[elementIndices[particleIndex] * 3] + 1];
					triangleVertexPositions[2] = theMesh.vertices[theMesh.triangles[elementIndices[particleIndex] * 3] + 2];
					v = (triangleVertexPositions[0] + triangleVertexPositions[1] + triangleVertexPositions[2]) / 3;
					break;
				case eMeshElements.TriangleNormal:
					triangleVertexNormals[0] = theMesh.normals[theMesh.triangles[elementIndices[particleIndex] * 3]];
					triangleVertexNormals[1] = theMesh.normals[theMesh.triangles[elementIndices[particleIndex] * 3] + 1];
					triangleVertexNormals[2] = theMesh.normals[theMesh.triangles[elementIndices[particleIndex] * 3] + 2];
					v = (triangleVertexNormals[0] + triangleVertexNormals[1] + triangleVertexNormals[2]) / 3;
					break;
				case eMeshElements.TriangleRotation:
					triangleVertexNormals[0] = theMesh.normals[theMesh.triangles[elementIndices[particleIndex] * 3]];
					triangleVertexNormals[1] = theMesh.normals[theMesh.triangles[elementIndices[particleIndex] * 3] + 1];
					triangleVertexNormals[2] = theMesh.normals[theMesh.triangles[elementIndices[particleIndex] * 3] + 2];
					Vector3 triangleNormal = (triangleVertexNormals[0] + triangleVertexNormals[1] + triangleVertexNormals[2]) / 3;
					triangleNormal *= -1;
					triangleVertexTangents[0] = theMesh.tangents[theMesh.triangles[elementIndices[particleIndex] * 3]];
					triangleVertexTangents[1] = theMesh.tangents[theMesh.triangles[elementIndices[particleIndex] * 3] + 1];
					triangleVertexTangents[2] = theMesh.tangents[theMesh.triangles[elementIndices[particleIndex] * 3] + 2];
					Vector3 triangleTangent = (triangleVertexTangents[0] + triangleVertexTangents[1] + triangleVertexTangents[2]) / 3;

					Quaternion theQuaternion = Quaternion.LookRotation(triangleNormal, triangleTangent);
					v = theQuaternion.eulerAngles;
					break;
			}

			// Optional conversion to emitter space.
			if (isEmitterRelative.GetValue())
			{
				if (selectedMeshElement == eMeshElements.TrianglePosition ||
					selectedMeshElement == eMeshElements.VertexPosition)
				{
					v += ownerBlueprint.ownerEmitter.transform.position;
					v = AmpsHelpers.RotateAroundPoint(v, ownerBlueprint.ownerEmitter.emitterPosition, ownerBlueprint.ownerEmitter.transform.rotation);
				}
				else if (selectedMeshElement == eMeshElements.TriangleRotation)
				{
					v += ownerBlueprint.ownerEmitter.transform.rotation.eulerAngles;
				}
			}


			vectors[particleIndex] = new Vector4(v.x, v.y, v.z, 0);

			isValidSample[particleIndex] = true;
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data is NOT available.
		override public void Evaluate(ref float input)
		{
			if (sampledMesh.GetValue() != null) ManageSampling(0);

			// We only do anything if there is a valid sample to work with.
			if (isValidSample[0])
			{
#if UNITY_EDITOR
				exampleInput = new Vector4(input, input, input, input);
				ownerBlueprint.ownerEmitter.exampleInputParticleIndex = -1;
#endif

				input = Blend(input, vectors[0], weight.GetValue());
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref float[] input)
		{
			InitializeNewParticles();	// TODO: Not executing this on each update could cause problems when toggling modules?

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				if (sampledMesh.GetValue() != null) ManageSampling(particleIndex);

				if (isValidSample[particleIndex])
				{
#if UNITY_EDITOR
					if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex != -1)
					{
						exampleInput = new Vector4(input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex],
													input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex],
													input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex],
													input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex]);
					}
#endif

					input[particleIndex] = Blend(input[particleIndex], vectors[particleIndex], weight.GetValue(particleIndex));

				}
			}
		}

		// EVALUATE //
		//
		// Evaluate when particle specific data IS available.
		override public void Evaluate(ref Vector4[] input)
		{
			InitializeNewParticles();

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				if (sampledMesh.GetValue() != null) ManageSampling(particleIndex);

				if (isValidSample[particleIndex])
				{
#if UNITY_EDITOR
					if (ownerBlueprint.ownerEmitter.exampleInputParticleIndex != -1)
					{
						exampleInput = input[ownerBlueprint.ownerEmitter.exampleInputParticleIndex];
					}
#endif

					input[particleIndex] = Blend(input[particleIndex], vectors[particleIndex], weight.GetValue(particleIndex));
				}
			}
		}

		//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = AmpsHelpers.formatEnumString(eCategories.Shapes.ToString());
			type = "Mesh sampler";
			SetDefaultName();

			sampledMesh = ScriptableObject.CreateInstance<MeshProperty>();
			sampledMesh.Initialize("Mesh");
			AddProperty(sampledMesh, true);
			sampledMeshElement = ScriptableObject.CreateInstance<DropdownProperty>();
			sampledMeshElement.Initialize("Element", 0, theOwnerBlueprint);
			AddProperty(sampledMeshElement, true);
			samplingOrder = ScriptableObject.CreateInstance<DropdownProperty>();
			samplingOrder.Initialize("Order", 0, theOwnerBlueprint);
			AddProperty(samplingOrder, true);
			isEmitterRelative = ScriptableObject.CreateInstance<BoolProperty>();
			isEmitterRelative.Initialize("Emitter relative?", theOwnerBlueprint);
			AddProperty(isEmitterRelative, true);
			implementsVisualization = false;
		}


		//============================================================================//
		#region GUI

		// SHOW PROPERTIES //
		//
		override public void ShowProperties(ref bool shouldRepaint)
		{
			base.ShowProperties(ref shouldRepaint);

			BaseProperty previousSelection = selectedProperty;
			PropertyGroup("Mesh");
			sampledMesh.ShowProperty(ref selectedProperty, false);
			if (sampledMeshElement.displayData == null) sampledMeshElement.displayData = () => meshElementsDisplayData; // We have to do this here because delegates are not serialized.
			sampledMeshElement.ShowProperty(ref selectedProperty, false);
			if (samplingOrder.displayData == null) samplingOrder.displayData = () => samplingOrderDisplayData; // We have to do this here because delegates are not serialized.
			samplingOrder.ShowProperty(ref selectedProperty, false);

			if (isEmitterRelative == null)	// HACK
			{
				isEmitterRelative = ScriptableObject.CreateInstance<BoolProperty>();
				isEmitterRelative.Initialize("Emitter relative?", ownerBlueprint);
				AddProperty(isEmitterRelative, true);
			}
			isEmitterRelative.ShowProperty(ref selectedProperty, false);

			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
		#endregion
#endif
	}
}