using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PitchingMachine : MonoBehaviour
{
    public Transform ballSpawnPosition;
	public Transform[] strikeZones = new Transform[9]; // 9개 위치
	[Range(1, 9)]
	public int currentTarget;
	public GameObject ballPrefab;

	public float shootDelayTime = 1f;
	public float shootPower = 30f;
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
			//ball.velocity = ballSpawnPosition.forward * shootPower; // 느린데 포물선그림
			//ball.velocity = strikeZones[0].forward * shootPower;

			CalculateBallisticVelocity(ball.transform.position, strikeZones[currentTarget - 1].position, shootPower, out Vector3 result);
			ball.velocity = result;

			// 5초 뒤 풀로
			StartCoroutine(ReturnBall(ball, 5f));

			yield return shootDelay;
		}
	}

	IEnumerator ReturnBall(Rigidbody ball, float count)
	{
		// 볼 풀로
		yield return new WaitForSeconds(count);
		ball.velocity = Vector3.zero;
		ball.angularVelocity = Vector3.zero;
		ball.gameObject.SetActive(false);
	}

	bool CalculateBallisticVelocity(Vector3 start, Vector3 target, float speed, out Vector3 result)
	{
		result = Vector3.zero;

		Vector3 dir = target - start;
		Vector3 dirXZ = new Vector3(dir.x, 0f, dir.z);
		float distance = dirXZ.magnitude;
		float height = dir.y;
		float gravity = Mathf.Abs(Physics.gravity.y);

		float speed2 = speed * speed;
		// 속도⁴ - 중력 * (중력 * 거리² + 높이차이 * 속도²)
		float underSqrt = speed2 * speed2 - gravity * (gravity * distance * distance + 2 * height * speed2);

		if (underSqrt < 0)
		{
			return false; // 도달 불가
		}

		float root = Mathf.Sqrt(underSqrt);
		float angle = Mathf.Atan((speed2 - root) / (gravity * distance));

		// 방향 적용
		Vector3 dirNorm = dirXZ.normalized;
		Vector3 velocity = dirNorm * speed * Mathf.Cos(angle) + Vector3.up * speed * Mathf.Sin(angle);
		result = velocity;
		return true;
	}


	void OnDrawGizmos()
	{
		if (ballSpawnPosition == null || strikeZones == null || strikeZones.Length == 0)
			return;

		Vector3 start = ballSpawnPosition.position;
		Vector3 target = strikeZones[currentTarget - 1].position;

		if (!CalculateBallisticVelocity(start, target, shootPower, out Vector3 velocity))
			return;

		// 포물선 궤적 그리기
		Gizmos.color = Color.red;
		Vector3 prev = start;
		float simulationTime = 2.0f;
		float step = 0.05f;
		Vector3 gravity = Physics.gravity;

		for (float t = 0; t < simulationTime; t += step)
		{
			Vector3 pos = start + velocity * t + 0.5f * gravity * t * t;
			Gizmos.DrawLine(prev, pos);
			prev = pos;
		}

		// 시작점, 도착점 표시
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(start, 0.05f);
		Gizmos.DrawSphere(target, 0.05f);
	}

}
