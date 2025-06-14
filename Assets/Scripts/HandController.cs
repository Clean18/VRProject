using System.Collections;
using System.Collections.Generic;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class HandController : MonoBehaviour
{
    public Animator anim;
	public bool isSelect;

	public ActionBasedController controller;

	void Awake()
	{
		anim = GetComponentInChildren<Animator>();
		controller = GetComponent<ActionBasedController>();
	}

	void Update()
	{
		if (controller != null)
		{
			// 0 ~ 0.1f
			float gripValue = controller.selectActionValue.action.ReadValue<float>();
			anim.SetFloat("GrabValue", gripValue);
		}
	}
}
