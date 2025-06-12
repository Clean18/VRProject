using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitchingMachine : MonoBehaviour
{
    public Transform ballSpawnPosition;
	public GameObject ballPrefab;

	public float shootDelayTime = 1f;
	public float shootPower = 50f;
    Coroutine shootRoutine;
	WaitForSeconds shootDelay;

	void Start()
	{
		shootDelay = new WaitForSeconds(shootDelayTime);
	}

	void Update()
	{
		if (shootRoutine == null)
		{
			shootRoutine = StartCoroutine(Shoot());
		}
	}

	IEnumerator Shoot()
	{
		while (true)
		{
			// 정면 방향으로 발사
			GameObject ball = Instantiate(ballPrefab, ballSpawnPosition.position, ballSpawnPosition.rotation);
			Rigidbody rigid = ball.GetComponent<Rigidbody>();

			// AddForce
			//rigid.AddForce(ballSpawnPosition.forward * shootPower, ForceMode.Impulse); // 너무빠름

			// Velocity
			rigid.velocity = ballSpawnPosition.forward * shootPower; // 느린데 포물선그림

			yield return shootDelay;
		}
	}
}
