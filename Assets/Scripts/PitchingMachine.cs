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
	public float ballReturnTime = 30f;

	// 공속도 text
	public TMP_Text ballSpeedText;

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
		// UI 버튼을 통해서 발사하게 변경
		//if (shootRoutine == null)
		//{
		//	shootRoutine = StartCoroutine(Shoot());
		//}
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
			if (VelocityCalculate(ball.transform.position, strikeZones[currentTarget - 1].position, shootPower, out Vector3 result)) // 스트라이크존 기준
			//if (VelocityCalculate(ball.transform.position, catcherPosition.position, shootPower, out Vector3 result)) // 포수 기준
			{
				// 텍스트
				if (ballSpeedText != null)
					ballSpeedText.text = string.Format("{0:F1} km/h", (result.magnitude * 3.6f));

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

		return true;
	}

	public void BallShoot(bool isCenter, float speed)
	{
		Rigidbody ball = GetBall();
		int ranIndex = Random.Range(0, 9);
		Vector3 targetPos = isCenter == true ?
			strikeZones[4].position // 중앙
			: strikeZones[ranIndex].position; // 랜덤

		Debug.Log($"{(isCenter == true ? "중앙" : $"랜덤 {ranIndex + 1}번")} 슛 발사 Speed : {speed}");
		Debug.Log($"targetPos : {targetPos}");

		if (VelocityCalculate(ball.transform.position, targetPos, speed, out Vector3 result))
		{
			if (ballSpeedText != null)
				ballSpeedText.text = string.Format("{0:F1} km/h", (result.magnitude * 3.6f));

			ball.AddForce(result * ball.mass, ForceMode.Impulse);
			ball.AddTorque(transform.right * speed * ball.inertiaTensor.magnitude, ForceMode.Impulse);

			StartCoroutine(ReturnBall(ball, ballReturnTime));
		}
	}
}
