using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[Header("Start Info")]
	public GameObject uiInfo;
	public GameObject uiInfoImage1;
	public GameObject uiInfoImage2;
	public GameObject bat;
	public TMP_Text uiInfoButtonText;
	public Transform batSpawnPosition;

	[Header("Pitching Machine")]
	public PitchingMachine pitchingMachine;
	public Transform strikeZone;
	public GameObject uiPitchingSetting;
	// Ball Speed
	public Slider speedSlider;
	public TMP_Text speedText; // 속도 : 00
	private float speedValue;
	// Sound
	public Slider soundSlider;
	public TMP_Text soundText;
	public float soundValue;
	// Ball TargetMode
	public Toggle targetModeToggle;
	public GameObject randomTarget;
	public bool isCenter = true;
	// Ball Speed Text
	public TMP_Text shootCountText;

	[Header("Display")]
	public TMP_Text displayText;

	[Header("Test")]
	public TMP_Text batSpeedText;

	public static UIManager Instance { get; private set; }

	void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(gameObject);
	}

	void Start()
	{
		// 피칭머신 세팅
		PitchingMachineInit();

		SetDisplayText("");
	}

	void PitchingMachineInit()
	{
		// 스트라이크존 비활성화
		strikeZone.gameObject.SetActive(false);
		// 슬라이어들 초기값 세팅
		speedSlider.onValueChanged.AddListener(OnSpeedValue);
		speedSlider.value = 0.01f;

		soundSlider.onValueChanged.AddListener(OnSoundValue);
		soundSlider.value = 0.5f;

		targetModeToggle.onValueChanged.AddListener(OnTargetModeToggle);
		targetModeToggle.isOn = false;
	}

	public void OnInfoButton()
	{
		Debug.Log("시작 정보 버튼 클릭");
		// 1번이 활성화면 1번닫고 2번 활성화
		if (uiInfoImage1.activeSelf)
		{
			uiInfoImage1.SetActive(false);
			uiInfoButtonText.text = "배트 소환";
			uiInfoImage2.SetActive(true);
		}
		// 2번이 활성화면 2번 닫고 배트 소환 후 UI 닫기
		else if (uiInfoImage2.activeSelf)
		{
			uiInfoImage2.SetActive(false);
			Instantiate(bat, batSpawnPosition.position, batSpawnPosition.rotation);
			uiInfo.SetActive(false);
		}
	}

	// 버튼 누르면 세팅 ui 사라지고 3초 카운트 후 슛
	// 스트라이크존 or 배트에 닿으면 UI 다시 활성화
	// 에디터에서 연결됨
	public void OnShootButton() => StartCoroutine(Shoot());

	IEnumerator Shoot()
	{
		// 스트라이크존 카메라 높이에 비례해서 높이 지정 및 활성화
		Vector3 stPos = strikeZone.transform.position;
		float camY = Camera.main.transform.position.y;
		// TODO : 스트라이크존 높이 괜찮은지 확인하기
		strikeZone.transform.position = new Vector3(stPos.x, camY * 0.75f, stPos.z);

		// Setting 비활성화
		uiPitchingSetting.SetActive(false);

		shootCountText.text = "3";
		// 스트라이크존 활성화
		strikeZone.gameObject.SetActive(true);

		yield return new WaitForSeconds(1f);

		shootCountText.text = "2";
		yield return new WaitForSeconds(1f);

		shootCountText.text = "1";
		yield return new WaitForSeconds(1f);

		// 슛
		pitchingMachine.BallShoot(isCenter, speedValue);
	}

	public void OffPitchingSetting() => StartCoroutine(OffPitchingSettingRoutine());

	IEnumerator OffPitchingSettingRoutine()
	{
		Debug.Log("5초 대기중...");
		// 여기서 5초 대기
		yield return new WaitForSeconds(5f);

		uiPitchingSetting.SetActive(true);
		strikeZone.gameObject.SetActive(false);
	}

	public void OnSpeedValue(float value)
	{
		Debug.Log($"스피드 슬라이더 값 변경 : {value}");
		// 텍스트 변경 정수만
		speedText.text = $"속도 : {(value * 100):F0}";
		// 스피드는 최소 12.3 ~ 50
		speedValue = Mathf.Lerp(12.3f, 50f, value);
	}

	public void OnSoundValue(float value)
	{
		Debug.Log($"사운드 슬라이더 값 변경 : {value}");
		soundText.text = $"사운드 : {(value * 100):F0}";
		soundValue = value;
	}

	public void OnTargetModeToggle(bool isOn)
	{
		Debug.Log($"타겟 모드 토글 값 변경 : {(isOn == true ? "랜덤" : "중앙")}");
		randomTarget.SetActive(isOn);
		isCenter = !isOn;
	}

	public void DebugText(string text) => batSpeedText.text = text;

	public void SetDisplayText(string text) => displayText.text = text;
}
