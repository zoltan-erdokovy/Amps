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
		if (GetComponent<Rigidbody>() != null)
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

				GetComponent<Rigidbody>().AddForce(currentNudgeForce * Time.deltaTime, ForceMode.Acceleration);
				
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