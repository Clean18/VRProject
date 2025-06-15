using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
	private bool isHit;

	void OnEnable() => isHit = false;

	void OnDisable() => isHit = false;

	void OnTriggerEnter(Collider other) => Hit();

	void OnCollisionEnter(Collision collision) => Hit();

	void Hit()
	{
		if (isHit) return;
		isHit = true;
		Debug.Log("볼에 뭔가 맞음");
		UIManager.Instance.OffPitchingSetting();
	}
}
