using UnityEngine;
using System.Collections;

public class RigidBodyNudger : MonoBehaviour
{
	public float nudgeIntervalMin = 0.5f;
	public float nudgeIntervalMax = 1.0f;
	private float timeUntilNudge = 0;
	public float nudgeLength = 2f;

	public float nudgeForce = 1;
	private Vector3 currentNudgeForce = Vector3.zero;

	void Start()
	{
	}

	void OnBecameVisible()
	{
		enabled = true;
	}

	void OnBecameInvisible()
	{
		enabled = false;
	}

	void Update()
	{
		if (rigidbody != null)
		{
			if (timeUntilNudge < 0)
			{
				if (currentNudgeForce == Vector3.zero)
				{
					//if (transform.localPosition == Vector3.zero)
					//{
					//    currentNudgeForce = currentNudgeForce + new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
					//    if (currentNudgeForce != Vector3.zero) currentNudgeForce = Vector3.Normalize(currentNudgeForce);
					//}
					//else currentNudgeForce = transform.position - transform.parent.transform.position;
					currentNudgeForce = transform.parent.transform.position - transform.position;
					currentNudgeForce = Vector3.Normalize(currentNudgeForce);
					currentNudgeForce = Vector3.Lerp(currentNudgeForce, new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)), 0.3f);
					currentNudgeForce = Vector3.Normalize(currentNudgeForce);
					
					currentNudgeForce = currentNudgeForce * nudgeForce;
					//Debug.DrawLine(transform.position, transform.parent.transform.position);
				}

				rigidbody.AddForce(currentNudgeForce * Time.deltaTime, ForceMode.Acceleration);
				
				if (timeUntilNudge < -nudgeLength)
				{
					timeUntilNudge = Random.Range(nudgeIntervalMin, nudgeIntervalMax);
					currentNudgeForce = Vector3.zero;
				}
			}

			timeUntilNudge -= Time.deltaTime;
		}
	}
}