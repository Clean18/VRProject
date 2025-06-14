using System.Collections;
using System.Collections.Generic;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System.Runtime.InteropServices.WindowsRuntime;

public class HandController : MonoBehaviour
{
    public Animator anim;
	private bool isSelect;
	public bool IsSelect
	{
		get { return isSelect; }
		set
		{
			isSelect = value;
			anim.SetBool("IsSelect", isSelect);
		}
	}

	public ActionBasedController controller;
	public XRRayInteractor rayInteractor;

	void Awake()
	{
		anim = GetComponentInChildren<Animator>();
		controller = GetComponent<ActionBasedController>();
		rayInteractor = GetComponentInChildren<XRRayInteractor>();
		if (rayInteractor != null)
		{
			rayInteractor.uiHoverEntered.AddListener(OnUIHover);
			rayInteractor.uiHoverExited.AddListener(OffUIHover);
		}
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

	public void OnUIHover(UIHoverEventArgs args) => IsSelect = true;

	public void OffUIHover(UIHoverEventArgs args) => IsSelect = false;
}
