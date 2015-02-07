using UnityEngine;
using System.Collections;

namespace Amps
{
	[System.Serializable]
	public class QuadRenderModule : BaseRenderModule
	{
		// E ORIENTATION //
		//
		public enum eOrientation
		{
			Rotated,
			CameraFacing,
			RotatedCameraFacing,
			VectorAlignedCameraFacing
		};

		// ORIENTATION DISPLAY DATA //
		//
		public readonly string[] orientationDisplayData =
		{
			"Rotated",
			"Camera facing",
			"Rotated camera facing",
			"Vector aligned camera facing"
		};

		// E NORMAL MODE //
		//
		public enum eNormalMode
		{
			Skip,
			Flat
		};

		// NORMAL MODE DISPLAY DATA //
		//
		public readonly string[] normalModeDisplayData =
		{
			"Skip",
			"Flat"
		};

		// E TANGENT MODE //
		//
		public enum eTangentMode
		{
			Skip,
			Generate,
			CustomVectorXYZW
		};

		// TANGENT MODE DISPLAY DATA //
		//
		public readonly string[] tangentModeDisplayData =
		{
			"Skip",
			"Generate",
			"Custom Vector (XYZW)"
		};

		// E UV2 MODE //
		//
		public enum eUV2Mode
		{
			Skip,
			CustomVectorXY,
			CustomVectorXYZW
		};

		// UV2 MODE DISPLAY DATA //
		//
		public readonly string[] uv2ModeDisplayData =
		{
			"Skip",
			"Custom Vector (XY)",
			"Custom Vector (XYZW)"
		};

		// E DOUBLE SIDED COLOR MODE //
		//
		public enum eDoubleSidedColorMode
		{
			ColorStack,
			CustomVectorStack
		};

		// DOUBLE SIDED COLOR MODE DISPLAY DATA //
		//
		public readonly string[] doubleSidedColorModeDisplayData =
		{
			"Use Color",
			"Use Custom Vector"
		};

		public DropdownProperty orientation;
		public DropdownProperty normalMode;
		public DropdownProperty tangentMode;
		public DropdownProperty uv2Mode;
		public BoolProperty isDoubleSided;
		public DropdownProperty doubleSidedColorMode;
		public VectorProperty alignmentVector;

		private Vector3[] vertices;
		private Vector2[] uvs;
		private Vector2[] uv2s;
		private Vector3[] normals;
		private Vector4[] tangents;
		private Color[] colors;
		private int[] triangles;
		private int particleCount;
		private int lastParticleCount;

		// EVALUATE //
		//
		override public void Evaluate()
		{
			Vector4 customVector;
			Vector3 position;
			Vector3 pivotOffset;
			Vector3 rotation;
			Quaternion quaternionRotation;
			Vector3 scale;
			Color color;
			bool shouldRecreateMesh;
			bool shouldBeDoubleSided;
			Quaternion camFacingRotation;
			int vertexIndexSteps;
			int triangleIndexSteps;

			if (ownerBlueprint == null ||
				ownerBlueprint.ownerEmitter == null ||
				ownerBlueprint.ownerEmitter.particleMarkers == null) return;		// To fix null ref log spam after script rebuild.

			if (finalMesh == null) SoftReset();

			ownerBlueprint.ownerEmitter.activeParticleIndices = ownerBlueprint.ownerEmitter.particleMarkers.GetActiveIndices();
			particleCount = ownerBlueprint.ownerEmitter.activeParticleIndices.Length;
			shouldRecreateMesh = (particleCount != lastParticleCount);
			shouldBeDoubleSided = isDoubleSided.GetValue();
			if (shouldBeDoubleSided)
			{
				vertexIndexSteps = 8;
				triangleIndexSteps = 12;
			}
			else
			{
				vertexIndexSteps = 4;
				triangleIndexSteps = 6;
			}
			// TODO: Investigate why this line fails at runtime around 2 * 4:
			//shouldRecreateMesh = triangles.Length != particleCount * vertexIndexSteps;

			if (shouldRecreateMesh)
			{
				finalMesh.Clear();

				vertices = new Vector3[particleCount * vertexIndexSteps];
				normals = new Vector3[particleCount * vertexIndexSteps];
				tangents = new Vector4[particleCount * vertexIndexSteps];
				uvs = new Vector2[particleCount * vertexIndexSteps];
				uv2s = new Vector2[particleCount * vertexIndexSteps];
				colors = new Color[particleCount * vertexIndexSteps];
				triangles = new int[particleCount * triangleIndexSteps];

				lastParticleCount = particleCount;
			}

			int particleIndex;
			for (int i = 0; i < ownerBlueprint.ownerEmitter.activeParticleIndices.Length; i++)
			{
				particleIndex = ownerBlueprint.ownerEmitter.activeParticleIndices[i];
				customVector = ownerBlueprint.customVectorStack.values[particleIndex];

				position = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.ownerEmitter.blueprint.positionStack.values[particleIndex]);	// Raw position stack result.
				position = AmpsHelpers.RotateAroundPoint(position, ownerBlueprint.ownerEmitter.transform.position, Quaternion.Inverse(ownerBlueprint.ownerEmitter.transform.rotation));
				position -= ownerBlueprint.ownerEmitter.emitterPosition;	// Compensating for emitter position, turning position world relative.

				pivotOffset = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.pivotOffsetStack.values[particleIndex]);

				rotation = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.rotationStack.values[particleIndex]); // Raw particle stack result.
				quaternionRotation = Quaternion.Inverse(ownerBlueprint.ownerEmitter.transform.rotation) * Quaternion.Euler(rotation);

				scale = AmpsHelpers.ConvertVector4Vector3(ownerBlueprint.scaleStack.values[particleIndex]);

				color = ownerBlueprint.colorStack.values[particleIndex];

				// Setting up the quad, particle is at the center.
				vertices[i * vertexIndexSteps] = (position + pivotOffset) + (new Vector3(scale.x, scale.y, 0) / 2);
				vertices[i * vertexIndexSteps + 1] = (position + pivotOffset) + (new Vector3(-scale.x, scale.y, 0) / 2);
				vertices[i * vertexIndexSteps + 2] = (position + pivotOffset) + (new Vector3(-scale.x, -scale.y, 0) / 2);
				vertices[i * vertexIndexSteps + 3] = (position + pivotOffset) + (new Vector3(scale.x, -scale.y, 0) / 2);

				switch (orientation.GetValue())
				{
					case (int)eOrientation.Rotated:
						// Rotating the (pivot offset) vertices around particle position.
						vertices[i * vertexIndexSteps] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps], position, quaternionRotation);
						vertices[i * vertexIndexSteps + 1] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 1], position, quaternionRotation);
						vertices[i * vertexIndexSteps + 2] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 2], position, quaternionRotation);
						vertices[i * vertexIndexSteps + 3] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 3], position, quaternionRotation);
						break;

					case (int)eOrientation.CameraFacing:
						camFacingRotation = Quaternion.LookRotation(ownerBlueprint.ownerEmitter.currentCameraTransform.forward, ownerBlueprint.ownerEmitter.currentCameraTransform.up);
						camFacingRotation = Quaternion.Inverse(ownerBlueprint.ownerEmitter.transform.rotation) * camFacingRotation;
						// TODO: Research if camera-particle position diff vector is better.

						// Rotating all vertices around the center so the quad faces the camera.
						vertices[i * vertexIndexSteps] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 1] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 1], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 2] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 2], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 3] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 3], position, camFacingRotation);
						break;

					case (int)eOrientation.RotatedCameraFacing:
						camFacingRotation = Quaternion.LookRotation(ownerBlueprint.ownerEmitter.currentCameraTransform.forward, ownerBlueprint.ownerEmitter.currentCameraTransform.up);
						camFacingRotation = Quaternion.Inverse(ownerBlueprint.ownerEmitter.transform.rotation) * camFacingRotation;
						camFacingRotation = camFacingRotation * Quaternion.Euler(rotation);

						// Rotating all vertices around the center so the quad faces the camera.
						vertices[i * vertexIndexSteps] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 1] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 1], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 2] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 2], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 3] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 3], position, camFacingRotation);
						break;

					case (int)eOrientation.VectorAlignedCameraFacing:
						//Vector3 upVector = Vector3.Normalize(alignmentVector.GetValue());
						//Vector3 forwardVector = ownerBlueprint.ownerEmitter.currentCameraTransform.forward;
						////ownerBlueprint.ownerEmitter.currentCameraTransform.up;
						//camFacingRotation = Quaternion.LookRotation(forwardVector, upVector);
						////camFacingRotation = Quaternion.Inverse(ownerBlueprint.ownerEmitter.transform.rotation) * camFacingRotation;

						camFacingRotation = Quaternion.LookRotation(ownerBlueprint.ownerEmitter.currentCameraTransform.forward, Vector3.up);
						Vector3 eulerRotation = camFacingRotation.eulerAngles;
						eulerRotation.x = 0;	// Discard rotation around x.
						eulerRotation.z = 0;	// Discard rotation around z.
						camFacingRotation = Quaternion.Euler(eulerRotation);

						// Rotating all vertices around the center so the quad faces the camera.
						vertices[i * vertexIndexSteps] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 1] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 1], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 2] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 2], position, camFacingRotation);
						vertices[i * vertexIndexSteps + 3] = AmpsHelpers.RotateAroundPoint(vertices[i * vertexIndexSteps + 3], position, camFacingRotation);
						break;
				}

				if (shouldBeDoubleSided)
				{
					vertices[i * vertexIndexSteps + 4] = vertices[i * vertexIndexSteps];
					vertices[i * vertexIndexSteps + 5] = vertices[i * vertexIndexSteps + 1];
					vertices[i * vertexIndexSteps + 6] = vertices[i * vertexIndexSteps + 2];
					vertices[i * vertexIndexSteps + 7] = vertices[i * vertexIndexSteps + 3];
				}

				colors[i * vertexIndexSteps] = color;
				colors[i * vertexIndexSteps + 1] = color;
				colors[i * vertexIndexSteps + 2] = color;
				colors[i * vertexIndexSteps + 3] = color;
				if (shouldBeDoubleSided)
				{
					if ((eDoubleSidedColorMode)doubleSidedColorMode.GetValue() == eDoubleSidedColorMode.ColorStack)
					{
						colors[i * vertexIndexSteps + 4] = colors[i * vertexIndexSteps];
						colors[i * vertexIndexSteps + 5] = colors[i * vertexIndexSteps + 1];
						colors[i * vertexIndexSteps + 6] = colors[i * vertexIndexSteps + 2];
						colors[i * vertexIndexSteps + 7] = colors[i * vertexIndexSteps + 3];

					}
					else
					{
						colors[i * vertexIndexSteps + 4] = customVector;
						colors[i * vertexIndexSteps + 5] = customVector;
						colors[i * vertexIndexSteps + 6] = customVector;
						colors[i * vertexIndexSteps + 7] = customVector;
					}
				}

				switch (normalMode.GetValue())
				{
					case (int)eNormalMode.Skip:
						break;
					case (int)eNormalMode.Flat:
						Vector3 calculatedNormal = Vector3.Cross((vertices[i * vertexIndexSteps + 2] - vertices[i * vertexIndexSteps]), (vertices[i * vertexIndexSteps + 1] - vertices[i * vertexIndexSteps]));
						calculatedNormal = Vector3.Normalize(calculatedNormal);
						normals[i * vertexIndexSteps] = calculatedNormal;
						normals[i * vertexIndexSteps + 1] = calculatedNormal;
						normals[i * vertexIndexSteps + 2] = calculatedNormal;
						normals[i * vertexIndexSteps + 3] = calculatedNormal;
						break;
				}
				if (shouldBeDoubleSided)
				{
					normals[i * vertexIndexSteps + 4] = normals[i * vertexIndexSteps] * -1;
					normals[i * vertexIndexSteps + 5] = normals[i * vertexIndexSteps + 1] * -1;
					normals[i * vertexIndexSteps + 6] = normals[i * vertexIndexSteps + 2] * -1;
					normals[i * vertexIndexSteps + 7] = normals[i * vertexIndexSteps + 3] * -1;
				}

				switch (uv2Mode.GetValue())
				{
					case (int)eUV2Mode.Skip:
						break;
					case (int)eUV2Mode.CustomVectorXY:
						uv2s[i * vertexIndexSteps] = new Vector2(customVector.x, customVector.y);
						uv2s[i * vertexIndexSteps + 1] = new Vector2(customVector.x, customVector.y);
						uv2s[i * vertexIndexSteps + 2] = new Vector2(customVector.x, customVector.y);
						uv2s[i * vertexIndexSteps + 3] = new Vector2(customVector.x, customVector.y);
						break;
					case (int)eUV2Mode.CustomVectorXYZW:	// BUG: Doesn't really work...
						Vector2 calculatedUv2 = Vector2.zero;
						calculatedUv2.x = AmpsHelpers.PackVector2(new Vector2(customVector.x, customVector.y));
						calculatedUv2.y = AmpsHelpers.PackVector2(new Vector2(customVector.z, customVector.w));
						uv2s[i * vertexIndexSteps] = calculatedUv2;
						uv2s[i * vertexIndexSteps + 1] = calculatedUv2;
						uv2s[i * vertexIndexSteps + 2] = calculatedUv2;
						uv2s[i * vertexIndexSteps + 3] = calculatedUv2;
						break;
				}
				if (shouldBeDoubleSided)
				{
					uv2s[i * vertexIndexSteps + 4] = uv2s[i * vertexIndexSteps];
					uv2s[i * vertexIndexSteps + 5] = uv2s[i * vertexIndexSteps + 1];
					uv2s[i * vertexIndexSteps + 6] = uv2s[i * vertexIndexSteps + 2];
					uv2s[i * vertexIndexSteps + 7] = uv2s[i * vertexIndexSteps + 3];
				}

				switch (tangentMode.GetValue())
				{
					case (int)eTangentMode.Skip:
						break;
					case (int)eTangentMode.Generate:
						//Vector3 rawTangent = Vector3.Normalize(vertices[i * 4 + 3] - vertices[i * 4 + 1]);
						Vector3 rawTangent = Vector3.Normalize(vertices[i * 4 + 0] - vertices[i * 4 + 1]);
						Vector4 calculatedTangent = new Vector4(rawTangent.x, rawTangent.y, rawTangent.z, -1);
						tangents[i * vertexIndexSteps] = calculatedTangent;
						tangents[i * vertexIndexSteps + 1] = calculatedTangent;
						tangents[i * vertexIndexSteps + 2] = calculatedTangent;
						tangents[i * vertexIndexSteps + 3] = calculatedTangent;
						break;
					case (int)eTangentMode.CustomVectorXYZW:
						tangents[i * vertexIndexSteps] = customVector;
						tangents[i * vertexIndexSteps + 1] = customVector;
						tangents[i * vertexIndexSteps + 2] = customVector;
						tangents[i * vertexIndexSteps + 3] = customVector;
						break;
				}
				if (shouldBeDoubleSided)
				{
					// TODO: Should it be multiplied by -1?
					tangents[i * vertexIndexSteps + 4] = tangents[i * vertexIndexSteps];
					tangents[i * vertexIndexSteps + 5] = tangents[i * vertexIndexSteps + 1];
					tangents[i * vertexIndexSteps + 6] = tangents[i * vertexIndexSteps + 2];
					tangents[i * vertexIndexSteps + 7] = tangents[i * vertexIndexSteps + 3];
				}


				if (shouldRecreateMesh)	// Things go in here which don't change unless particle count changes.
				{
					uvs[i * vertexIndexSteps] = new Vector2(1, 1);
					uvs[i * vertexIndexSteps + 1] = new Vector2(0, 1);
					uvs[i * vertexIndexSteps + 2] = new Vector2(0, 0);
					uvs[i * vertexIndexSteps + 3] = new Vector2(1, 0);
					if (shouldBeDoubleSided)
					{
						uvs[i * vertexIndexSteps + 4] = new Vector2(1, 1);
						uvs[i * vertexIndexSteps + 5] = new Vector2(0, 1);
						uvs[i * vertexIndexSteps + 6] = new Vector2(0, 0);
						uvs[i * vertexIndexSteps + 7] = new Vector2(1, 0);
					}

					//First triangle.
					triangles[i * triangleIndexSteps] = i * vertexIndexSteps;
					triangles[i * triangleIndexSteps + 1] = i * vertexIndexSteps + 2;
					triangles[i * triangleIndexSteps + 2] = i * vertexIndexSteps + 1;
					//Second triangle.
					triangles[i * triangleIndexSteps + 3] = i * vertexIndexSteps + 2;
					triangles[i * triangleIndexSteps + 4] = i * vertexIndexSteps;
					triangles[i * triangleIndexSteps + 5] = i * vertexIndexSteps + 3;

					if (shouldBeDoubleSided)
					{
						//Third triangle.
						triangles[i * triangleIndexSteps + 6] = i * vertexIndexSteps + 5;
						triangles[i * triangleIndexSteps + 7] = i * vertexIndexSteps + 6;
						triangles[i * triangleIndexSteps + 8] = i * vertexIndexSteps + 4;
						//Forth triangle.
						triangles[i * triangleIndexSteps + 9] = i * vertexIndexSteps + 7;
						triangles[i * triangleIndexSteps + 10] = i * vertexIndexSteps + 4;
						triangles[i * triangleIndexSteps + 11] = i * vertexIndexSteps + 6;
					}
				}
			}

			// TODO: Bubble sort with limited run time.
			finalMesh.vertices = vertices;
			finalMesh.uv = uvs;
			finalMesh.uv2 = uv2s;
			finalMesh.normals = normals;
			finalMesh.tangents = tangents;
			finalMesh.colors = colors;
			finalMesh.triangles = triangles;
		}

//============================================================================//
#if UNITY_EDITOR

		// INITIALIZE //
		//
		override public void Initialize(BaseStack theOwnerStack, AmpsBlueprint theOwnerBlueprint)
		{
			base.Initialize(theOwnerStack, theOwnerBlueprint);

			subMenuName = "";
			type = "Quad renderer";
			SetDefaultName();

			orientation = ScriptableObject.CreateInstance<DropdownProperty>();
			orientation.Initialize("Orientation", 1, theOwnerBlueprint);
			AddProperty(orientation, false);
			alignmentVector = ScriptableObject.CreateInstance<VectorProperty>();
			alignmentVector.Initialize("Alignment vector", new Vector4(0, 1, 0, 0), theOwnerBlueprint);
			alignmentVector.hideW = true;
			AddProperty(alignmentVector, false);
			normalMode = ScriptableObject.CreateInstance<DropdownProperty>();
			normalMode.Initialize("Normal mode", 0, theOwnerBlueprint);
			AddProperty(normalMode, false);
			tangentMode = ScriptableObject.CreateInstance<DropdownProperty>();
			tangentMode.Initialize("Tangent mode", 0, theOwnerBlueprint);
			AddProperty(tangentMode, false);
			uv2Mode = ScriptableObject.CreateInstance<DropdownProperty>();
			uv2Mode.Initialize("UV2 mode", 0, theOwnerBlueprint);
			AddProperty(uv2Mode, false);
			isDoubleSided = ScriptableObject.CreateInstance<BoolProperty>();
			isDoubleSided.Initialize("Double sided?", false, theOwnerBlueprint);
			AddProperty(isDoubleSided, false);
			doubleSidedColorMode = ScriptableObject.CreateInstance<DropdownProperty>();
			doubleSidedColorMode.Initialize("Backside color mode", 0, theOwnerBlueprint);
			AddProperty(doubleSidedColorMode, false);
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

			if (orientation.displayData == null) orientation.displayData = () => orientationDisplayData; // We have to do this here because delegates are not serialized.
			orientation.ShowProperty(ref selectedProperty, false);

			if (alignmentVector == null)	// HACK
			{
				alignmentVector = ScriptableObject.CreateInstance<VectorProperty>();
				alignmentVector.Initialize("Alignment vector", new Vector4(0, 1, 0, 0), ownerBlueprint);
				alignmentVector.hideW = true;
				AddProperty(alignmentVector, false);
			}
			if (orientation.GetValue() == (int)eOrientation.VectorAlignedCameraFacing) alignmentVector.ShowProperty(ref selectedProperty, false);

			if (normalMode.displayData == null) normalMode.displayData = () => normalModeDisplayData; // We have to do this here because delegates are not serialized.
			normalMode.ShowProperty(ref selectedProperty, false);
			if (tangentMode.displayData == null) tangentMode.displayData = () => tangentModeDisplayData; // We have to do this here because delegates are not serialized.
			tangentMode.ShowProperty(ref selectedProperty, false);
			if (uv2Mode.displayData == null) uv2Mode.displayData = () => uv2ModeDisplayData; // We have to do this here because delegates are not serialized.
			uv2Mode.ShowProperty(ref selectedProperty, false);

			bool previousIsDoubleSided = isDoubleSided.GetValue();
			isDoubleSided.ShowProperty(ref selectedProperty, false);
			bool currentIsDoubleSided = isDoubleSided.GetValue();
			if (currentIsDoubleSided != previousIsDoubleSided) ownerBlueprint.ownerEmitter.SoftReset();

			if (currentIsDoubleSided)
			{
				if (doubleSidedColorMode.displayData == null) doubleSidedColorMode.displayData = () => doubleSidedColorModeDisplayData; // We have to do this here because delegates are not serialized.
				doubleSidedColorMode.ShowProperty(ref selectedProperty, false);
			}
			if (selectedProperty != previousSelection) shouldRepaint = true;
		}
#endregion
#endif
	}
}