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

	// 오브젝트 풀
	public int poolCount = 10;
	public List<Rigidbody> ballPool = new();

	void Awake()
	{
		CreateBall();
	}

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

	void CreateBall()
	{
		for (int i = 0; i < poolCount; i++)
		{
			var ball = Instantiate(ballPrefab, ballSpawnPosition.position, ballSpawnPosition.rotation);
			ball.SetActive(false);
			var ballRigid = ball.GetComponent<Rigidbody>();
			ballPool.Add(ballRigid);
		}
	}

	Rigidbody GetBall()
	{
		foreach (var ballRigid in ballPool)
		{
			if (!ballRigid.gameObject.activeSelf)
			{
				ballRigid.velocity = Vector3.zero;
				ballRigid.angularVelocity = Vector3.zero;
				ballRigid.transform.position = ballSpawnPosition.position;
				ballRigid.transform.rotation = ballSpawnPosition.rotation;
				ballRigid.gameObject.SetActive(true);
				return ballRigid;
			}
		}
		// 꺼낼 볼 없으면 생성
		var ball = Instantiate(ballPrefab, ballSpawnPosition.position, ballSpawnPosition.rotation);
		var newRigid = ball.GetComponent<Rigidbody>();
		ballPool.Add(newRigid);
		return newRigid;
	}

	IEnumerator Shoot()
	{
		while (true)
		{
			// 정면 방향으로 발사
			Rigidbody ball = GetBall();

			// AddForce
			//ball.AddForce(ballSpawnPosition.forward * shootPower, ForceMode.Impulse); // 너무빠름

			// Velocity
			ball.velocity = ballSpawnPosition.forward * shootPower; // 느린데 포물선그림

			// 5초 뒤 풀로
			StartCoroutine(ReturnBall(ball, 5f));

			yield return shootDelay;
		}
	}

	IEnumerator ReturnBall(Rigidbody ball, float count)
	{
		yield return new WaitForSeconds(count);
		ball.velocity = Vector3.zero;
		ball.angularVelocity = Vector3.zero;
		ball.gameObject.SetActive(false);
	}
}
