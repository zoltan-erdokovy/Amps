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

public class CameraCarrier : MonoBehaviour
{
	public float dragSensitivity = 2f;
	public float rotationLimitX = 28;
	public float rotationLimitY = 18;
	public Vector3 topPosition = new Vector3(0f, 0f, 0f);
	public Vector3 middlePosition = new Vector3(0f, 0f, 0f);
	public Vector3 bottomPosition = new Vector3(0f, 0f, 0f);
	public float stepSize = 2;
	public int stepAmount = 4;

	private bool lmbHeld = false;
	private bool rmbHeld = false;
	private bool mmbHeld = false;
	private int currentStep = 0;
	private Vector3 eulerRotation = Vector3.zero;
	private Vector3 positionOffset = Vector3.zero;
	private float positionBlendAlpha;
	private Vector3 currentPosition;

	void Start()
	{
		positionOffset = middlePosition;
	}

	void Update()
	{

	}

	void LateUpdate()
	{
		lmbHeld = Input.GetMouseButton(0);
		rmbHeld = Input.GetMouseButton(1);
		mmbHeld = Input.GetMouseButton(2);

		if (lmbHeld && !rmbHeld && !mmbHeld)
		{
			eulerRotation.x = Mathf.Clamp(eulerRotation.x + Input.GetAxis("Mouse Y") * -dragSensitivity, -rotationLimitX, rotationLimitX);
			eulerRotation.y = Mathf.Clamp(eulerRotation.y + Input.GetAxis("Mouse X") * dragSensitivity, -rotationLimitY, rotationLimitY);
			transform.rotation = Quaternion.Euler(eulerRotation);

			positionBlendAlpha = Mathf.Abs(eulerRotation.x / rotationLimitX);
			if (eulerRotation.x > 0) positionOffset = Vector3.Lerp(middlePosition, topPosition, positionBlendAlpha);
			else positionOffset = Vector3.Lerp(middlePosition, bottomPosition, positionBlendAlpha);
		}

		currentPosition = transform.position;
		if ((Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(KeyCode.RightArrow)) && currentStep < stepAmount) // Wheel up / Right arrow.
		{
			currentPosition.x += stepSize;
			currentStep++;
		}
		else if ((Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(KeyCode.LeftArrow)) && currentStep > 0) // Wheel down / Left arrow.
		{
			currentPosition.x -= stepSize;
			currentStep--;
		}
		currentPosition.y = positionOffset.y;
		currentPosition.z = positionOffset.z;
		transform.position = currentPosition;
	}
}
