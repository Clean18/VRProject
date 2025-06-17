using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
	void OnTriggerEnter(Collider other) => Hit(other.gameObject);

	void OnCollisionEnter(Collision collision) => Hit(collision.gameObject);

	void Hit(GameObject go)
	{
		if (go.CompareTag("HomerunCheck"))
		{
			// 홈런
			Debug.Log("홈런");
			UIManager.Instance.SetDisplayText("Home Run!!");
		}
		else if (go.CompareTag("FoulCheck"))
		{
			// 파울
			Debug.Log("파울");
			UIManager.Instance.SetDisplayText("Foul...");
		}
	}
}
