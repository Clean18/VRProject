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

	public IXRSelectInteractor mainInteractor = null;
	public IXRSelectInteractor subInteractor = null;

	void Start()
	{
		previousPosition = transform.position;

		sound = GetComponent<AudioSource>();

	}

	void OnCollisionEnter(Collision collision)
	{
		if (!collision.collider.CompareTag("Ball")) return;

		// 사운드 재생
		sound.PlayOneShot(hitSound, UIManager.Instance.soundValue);

		Rigidbody ballRigid = collision.rigidbody;

		// 볼 속도, 빠따 속도
		Vector3 ballVelocity = ballRigid.velocity;
		Vector3 batVelocity = averageVelocity;

		float ballSpeed = ballVelocity.magnitude;
		float batSpeed = batVelocity.magnitude;

		// 볼속도 20%, 빠따 속도 80%
		float exitSpeed = (0.2f * ballSpeed) + (0.8f * batSpeed);
		// 보정
		//exitSpeed = Mathf.Clamp(exitSpeed, 5f, 60f);

		// 타격 방향
		Vector3 swingDir = transform.forward.normalized;
		Vector3 motionDir = batVelocity.normalized;
		Vector3 hitDirection = (swingDir * 0.3f + motionDir * 0.7f).normalized;

		// Y 방향 과도하게 아래로 튀지 않도록 보정
		if (hitDirection.y < -0.4f)
			hitDirection.y = -0.4f;
		hitDirection.Normalize();

		// 타구 속도 impulse = mass × velocity → 힘 = 방향 × 속도 × 질량
		Vector3 impulseForce = hitDirection * exitSpeed * ballRigid.mass;
		ballRigid.AddForce(impulseForce, ForceMode.Impulse);

		//Debug.Log($"[타구 속도] {exitSpeed:F2}, [방향] {hitDirection}");
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


}
