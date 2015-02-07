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
