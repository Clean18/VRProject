using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// 배트 이동 기록용
public struct VelocitySpeed
{
	public Vector3 velocity;
	public float time;

	public VelocitySpeed(Vector3 _velocity, float _time)
	{
		this.velocity = _velocity;
		this.time = _time;
	}
}
public class CustomGrabInteractable : XRGrabInteractable
{
	// 배트가 움직이는 속도 평균내기
	private Queue<VelocitySpeed> velocities = new();
	[Header("Custom Property")]
	public float velocityDuation = 0.2f;    // throwSmoothingDuration	기록할 시간
	public float velocityAdjust = 1.5f;     // velocityScale			속도값 보정

	private Vector3 previousPosition;   // 이전 위치
	private Vector3 averageVelocity;    // 평균 속도

	private AudioSource sound;
	public AudioClip hitSound;  // 타격 사운드

	[Header("물리 추가 보정 값")]
	public float bonusPower;

	public bool isHit;
	

	void Start()
	{
		previousPosition = transform.position;

		sound = GetComponent<AudioSource>();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (!collision.collider.CompareTag("Ball")) return;

		// 여러번 타격되지 않게 막기
		if (isHit == true) return;

		isHit = true;
		Invoke("SetIsHit", 3);

		// 사운드 재생
		sound.PlayOneShot(hitSound, UIManager.Instance.soundValue);

		Rigidbody ballRigid = collision.rigidbody;

		// 볼 속도, 빠따 속도
		Vector3 ballVelocity = ballRigid.velocity;
		Vector3 batVelocity = averageVelocity;

		float ballSpeed = ballVelocity.magnitude;
		float batSpeed = batVelocity.magnitude;

		// 볼속도 30%, 빠따 속도 70%
		float exitSpeed = (0.3f * ballSpeed) + (0.7f * batSpeed);
		// 번트 or 배트를 스트라이크존에 두면 기본 물리엔진으로만 처리
		if (batSpeed < 0.2f)
		{
			//UIManager.Instance.DebugText($"batSpeed : {batSpeed}\n배트가 느림");
			return;
		}
		//UIManager.Instance.DebugText($"exitSpeed({exitSpeed:F3})\n= ballSpeed({ballSpeed:F3})\n+ batSpeed({batSpeed:F3})");

		// 타격 방향
		Vector3 swingDir = Vector3.back;
		Vector3 motionDir = batVelocity.normalized;
		Vector3 hitDirection = (swingDir * 0.3f + motionDir * 0.7f).normalized;

		// 중심 기준 방향
		Vector3 centerDir = Vector3.back;

		// XZ 평면 기준 각도 계산
		Vector3 centerXZ = new Vector3(centerDir.x, 0f, centerDir.z).normalized;
		Vector3 hitXZ = new Vector3(hitDirection.x, 0f, hitDirection.z).normalized;

		float signedAngle = Vector3.SignedAngle(centerXZ, hitXZ, Vector3.up);
		float absAngle = Mathf.Abs(signedAngle);

		// 안타 허용 범위
		float safeAngleMin = -45f;
		float safeAngleMax = 45f;

		// 범위 밖이면 안타 각도로 보정
		if (signedAngle < safeAngleMin || signedAngle > safeAngleMax)
		{
			// 방향 유지하면서, 가장자리 각도로 보정
			float clampedAngle = Mathf.Clamp(signedAngle, safeAngleMin, safeAngleMax);
			Quaternion rot = Quaternion.AngleAxis(clampedAngle, Vector3.up);
			Vector3 limitXZ = rot * centerXZ;

			// 원래 높이는 유지
			hitDirection = new Vector3(limitXZ.x, hitDirection.y, limitXZ.z).normalized;
			//UIManager.Instance.DebugText("안타 보정");
		}

		//UIManager.Instance.DebugText($"hitDirection\n{hitDirection}\n= swingDir\n{swingDir}\n+ motionDir\n{motionDir}");
		hitDirection.Normalize();

		// 타구 속도 impulse = mass × velocity → 힘 = 방향 × 속도 × 질량
		Vector3 impulseForce = hitDirection * exitSpeed * ballRigid.mass * bonusPower;
		//UIManager.Instance.DebugText($"타격 파워 : {exitSpeed}\n타격 방향 : {hitDirection}\n최종 방향 : {impulseForce}\n최종 파워 : {impulseForce.magnitude}");

		ballRigid.AddForce(impulseForce, ForceMode.Impulse);
	}

	void Update()
	{
		if (!isSelected) return;

		// 현재 프레임의 정보 넣기
		Vector3 currentPosition = transform.position;
		Vector3 currentVelocity = (currentPosition - previousPosition) / Time.deltaTime;
		previousPosition = currentPosition;

		float currentTime = Time.time;
		velocities.Enqueue(new VelocitySpeed(currentVelocity, currentTime));

		// throwSmoothingDuration 보다 오래된 속도정보 삭제
		while (velocities.Count > 0 && currentTime - velocities.Peek().time > velocityDuation)
		{
			velocities.Dequeue();
		}

		// 평균 속도 계산
		averageVelocity = Vector3.zero;
		foreach (var sample in velocities)
		{
			averageVelocity += sample.velocity;
		}

		if (velocities.Count > 0)
		{
			averageVelocity /= velocities.Count;
		}

		// velocityScale 로 최종 속도 보정
		averageVelocity *= velocityAdjust;
		//Debug.Log($"{velocityDuation}초 기준 평균 속도 : {averageVelocity}");
	}

	void SetIsHit() => isHit = false;
}
