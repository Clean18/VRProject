using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

public class PitchingMachine : MonoBehaviour
{
    public Transform ballSpawnPosition;
	public Transform[] strikeZones = new Transform[9]; // 스트라이크존 9개 위치
	[Range(1, 9)]
	public int currentTarget; // 공을 던질 스트라이크존 번호
	public Transform catcherPosition; // 포수 위치
	public GameObject ballPrefab;

	public float shootDelayTime = 1f;	// 던지는 속도
	public float shootPower = 30f;		// 던지는 힘
    Coroutine shootRoutine;
	WaitForSeconds shootDelay;

	// 오브젝트 풀
	public int poolCount = 10;
	public List<Rigidbody> ballPool = new();
	public Dictionary<Rigidbody, TrailRenderer> trailDic = new();
	public float ballReturnTime = 9f;

	// 공속도 text
	public TMP_Text ballSpeedTest;

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
			var ballTrail = ball.GetComponent<TrailRenderer>();
			ballPool.Add(ballRigid);
			trailDic.Add(ballRigid, ballTrail);
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
		var newTrail = ball.GetComponent<TrailRenderer>();
		ballPool.Add(newRigid);
		trailDic.Add(newRigid, newTrail);
		return newRigid;
	}

	IEnumerator Shoot()
	{
		while (true)
		{
			// 정면 방향으로 발사
			Rigidbody ball = GetBall();

			// 속도가 느려도 존에 닿을 수 있게
			//if (VelocityCalculate(ball.transform.position, strikeZones[currentTarget - 1].position, shootPower, out Vector3 result)) // 스트라이크존 기준
			if (VelocityCalculate(ball.transform.position, catcherPosition.position, shootPower, out Vector3 result)) // 포수 기준
			{
				// 텍스트
				if (ballSpeedTest != null)
					ballSpeedTest.text = string.Format("{0:F1} km/h", (result.magnitude * 3.6f));

				//// 볼 속도
				//ball.velocity = result;
				//// 볼 회전속도
				//ball.angularVelocity = transform.right * shootPower;

				// 힘 적용
				ball.AddForce(result * ball.mass, ForceMode.Impulse);
				// 볼 회전력 적용
				ball.AddTorque(transform.right * shootPower * ball.inertiaTensor.magnitude, ForceMode.Impulse);

				// ballReturnTime 초 뒤 풀로 돌아감
				StartCoroutine(ReturnBall(ball, ballReturnTime));
			}

			yield return shootDelay;
		}
	}

	IEnumerator ReturnBall(Rigidbody ball, float count)
	{
		// 볼 풀로
		yield return new WaitForSeconds(count);
		if (trailDic.TryGetValue(ball, out TrailRenderer trail))
			trail.Clear();

		ball.gameObject.SetActive(false);
	}

	bool VelocityCalculate(Vector3 start, Vector3 target, float speed, out Vector3 result)
	{
		// 포물선 운동
		// 시작위치(start)에서 도착위치(target)까지 도달하기 위한 초기 속도 벡터를 계산
		result = Vector3.zero;

		// 시작점에서 목표점까지의 방향 벡터
		Vector3 dir = target - start;

		// 수평 방향(xz 평면)의 방향 벡터 (Y 높이는 제거)
		Vector3 horizonDirection = new Vector3(dir.x, 0f, dir.z);

		// 수평 거리 (XZ 평면 거리)
		float distance = horizonDirection.magnitude;

		// 수직 높이 차이 (목표지점의 y - 시작점의 y)
		float height = dir.y;

		// 중력 가속도 절대값 9.81f
		float gravity = Mathf.Abs(Physics.gravity.y);

		// 속도의 제곱 (v²)
		float speed2 = speed * speed;

		// 포물선 발사각 계산의 판별식 부분 (제곱근 아래 항)
		// = v⁴ - g * (g * d² + 2 * h * v²)
		float underSqrt = speed2 * speed2 - gravity * (gravity * distance * distance + 2 * height * speed2);

		// 0보다 낮으면 도달 불가
		if (underSqrt < 0) return false;

		// 판별식의 루트 계산
		float root = Mathf.Sqrt(underSqrt);

		// 포물선 발사각 θ 계산 (낮은 각도 선택)
		float angle = Mathf.Atan((speed2 - root) / (gravity * distance));

		// 수평 방향 단위 벡터 계산 (방향만 남김)
		Vector3 dirNorm = horizonDirection.normalized;

		// 최종 초기 속도 벡터 = 수평 + 수직 분해
		// dirNorm * speed * cos(θ) → 수평 속도
		// Vector3.up * speed * sin(θ) → 수직 속도
		Vector3 velocity = dirNorm * speed * Mathf.Cos(angle) + Vector3.up * speed * Mathf.Sin(angle);

		// 최종 결과로 출력
		result = velocity;

		// 성공적으로 계산되었음을 반환
		return true;
	}



	void OnDrawGizmos()
	{
		if (ballSpawnPosition == null || strikeZones == null || strikeZones.Length == 0)
			return;

		Vector3 start = ballSpawnPosition.position;
		//Vector3 target = strikeZones[currentTarget - 1].position; // 스트라이크존 기준
		Vector3 target = catcherPosition.position; // 포수 기준

		if (!VelocityCalculate(start, target, shootPower, out Vector3 velocity))
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
