using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
	public Queue<VelocitySpeed> velocities = new();
	public float velocityDuation = 0.2f; // throwSmoothingDuration	기록할 시간
	public float velocityAdjust = 1.5f; // velocityScale			속도값 보정

	private Vector3 previousPosition;	// 이전 위치
	private Vector3 averageVelocity;    // 평균 속도

	void Start()
	{
		
		previousPosition = transform.position;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (!collision.collider.CompareTag("Ball")) return;
		Rigidbody ballRigid = collision.rigidbody;

		// 1. 배트 이동 속도 (0.2초 평균)
		Vector3 batVelocity = averageVelocity;

		float strength = batVelocity.magnitude * 3.0f;

		// 실제 타격 방향 = 배트의 정면 (Z축)
		Vector3 swingDir = transform.forward.normalized;
		Vector3 motionDir = batVelocity.normalized;

		// 기본 방향 + 속도 방향 혼합
		Vector3 hitDirection = (swingDir * 0.7f + motionDir * 0.3f).normalized;

		Vector3 ballVelocity = ballRigid.velocity;
		Vector3 finalForce = hitDirection * strength + ballVelocity * 0.1f;

		//ballRigid.AddForce(finalForce, ForceMode.Impulse);
		ballRigid.AddForce(finalForce);
		Debug.Log($"타격 방향: {hitDirection}, 힘: {strength}");
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
