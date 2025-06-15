using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Start Info")]
    public GameObject uiInfo;
    public GameObject uiInfoImage1;
    public GameObject uiInfoImage2;
    public GameObject bat;
    public TMP_Text uiInfoButtonText;
    public Transform batSpawnPosition;

    [Header("Other")]
    public int temp;

    public void OnShootButton()
    {
        Debug.Log("공발사!");
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
}
