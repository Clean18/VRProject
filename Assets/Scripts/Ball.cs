using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
	private bool isHit;

	void OnEnable() => isHit = false;

	void OnDisable() => isHit = false;

	void OnTriggerEnter(Collider other) => Hit(other.gameObject);

	void OnCollisionEnter(Collision collision) => Hit(collision.gameObject);

	void Hit(GameObject go)
	{
		if (isHit) return;

		isHit = true;
		Debug.Log("볼에 뭔가 맞음");
		UIManager.Instance.OffPitchingSetting();

		if (go.CompareTag("HomerunCheck"))
		{
			// 홈런
			UIManager.Instance.SetDisplayText("Home Run!!");
		}
		else if (go.CompareTag("PoulCheck"))
		{
			// 파울
			UIManager.Instance.SetDisplayText("Poul...");
		}

	}
}
