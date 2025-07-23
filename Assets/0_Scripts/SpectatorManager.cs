using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpectatorManager : MonoBehaviour
{
	//싱글턴
    public static SpectatorManager instance;

	private void Awake()
	{
		if(instance == null) instance = this;
		//해당 스크립트는 카메라에 붙게되며, 초반에는 비활성화 되어있음
		gameObject.SetActive(false);
	}

	//현재 살아있는 플레이어들을 저장하는 리스트
	private List<Player> alivePlayers = new List<Player> ();
	//현재 보고 있는 플레이어 리스트 인덱스
	private int currentIdx = 0;
	//플레이어 정보
	private Player currentTargetPlayer;
	private string currentPlayerName;
	private GameObject currentTargetObject;
	private Camera currentTargetCamera;
	private float wallDistance = 3f;

	//관련 카메라 정보
	private float SmoothSpeed = 10f;
	public LayerMask collisionLayer;
	public Vector3 offset = new Vector3(0, 5, -5);
	public Vector3 rot = new Vector3(30f, 0f, 0f); // 위에서 15도 내려다보는 시점

	private Vector3 finalCameraPos;

	//활성화 될 경우 실행되는 함수
	private void OnEnable()
	{
		//해당 스크립트가 존재하는 경우
		if (PlayerTracker.instance != null)
		{
			//해당 스크립트의 이벤트에 다음 함수를 추가시켜준다.
			PlayerTracker.instance.OnAlivePlayersChanged += RefreshAlivePlayers;
		}
	}

	//비활성화 될 경우 실행되는 함수
	private void OnDisable()
	{
		//해당 스크립트가 존재하는 경우
		if (PlayerTracker.instance != null)
		{
			//해당 스크립트의 이벤트에 다음 함수를 제거
			PlayerTracker.instance.OnAlivePlayersChanged -= RefreshAlivePlayers;
		}
	}

	//해당 함수는 새로운 플레이어가 등록되거나 해제될 때 이벤트로 호출되는 함수이다.
	void RefreshAlivePlayers()
	{
		//우선 현재 생존하고 있는 플레이어를 가져오고
		alivePlayers = PlayerTracker.instance.GetAlivePlayers();

		//만약 생존한 플레이어가 없으면 처리를 더이상 하지 않는다.
		if(alivePlayers.Count == 0)
		{
			Debug.Log("No Alive Players");
			currentTargetObject = null;
			return;
		}

		//만약 현재 currentTargetPlayer가 리스트에 없으면
			//즉, 방금 죽어서 해당 함수를 처음 실행하는 상태로 null로 되어있는 경우 
			//혹은, 보고있는 플레이어가 사망하게 된 경우
		if (!alivePlayers.Contains(currentTargetPlayer))
		{
			//다음과 같이 currentIdx를 0으로 초기화 해서 해당 플레이어를 관전하도록 설정한다.
			currentIdx = 0;
			currentTargetPlayer = alivePlayers[currentIdx];
			currentTargetObject = PlayerTracker.instance.GetPlayerObject(currentTargetPlayer);
			currentTargetCamera = currentTargetObject.GetComponentInChildren<Camera>(true);
		}
	}

	//관전으로 넘어갈 때 호출되는 함수
	public void ActiveUIBeforeSpectating()
	{
		gameObject.SetActive(true);
		StartCoroutine(ActiveUI());
	}

	//일정 이상 UI를 출력 후, 없애주는 간단한 코루틴
	IEnumerator ActiveUI()
	{
		Game_UIManager.instance.DeadText.SetActive (true);
		yield return new WaitForSeconds(2f);
		Game_UIManager.instance.DeadText.SetActive(false);
		Game_UIManager.instance.specText.SetActive(true);
	}

	//눌리는 화살표에 따라 관전 대상을 변경하는 함수
	private void Update()
	{
		//만약 현재 살아있는 플레이어가 없을 경우
		if (alivePlayers.Count == 0)
		{
			RefreshAlivePlayers();
			//Debug.LogError("List empty");
			return;
		}

		//만약 대상 오브젝트가 없는 경우
		if(currentTargetCamera == null)
		{
			//다시 시도해보고
			ChangeTarget();
			//그래도 없으면 return
			if (currentTargetCamera == null)
			{
				return;
			}
		}
		
		//카메라의 위치와 회전을, 관전하는 플레이어 카메라의 실제 위치와 회전 방향을 따르도록 한다.
		//transform.position = Vector3.Lerp(transform.position, currentTargetCamera.transform.position, Time.deltaTime * SmoothSpeed);
		UpdateCamPosition();
		transform.rotation = Quaternion.Slerp(transform.rotation, currentTargetCamera.transform.rotation , Time.deltaTime * SmoothSpeed);

		//왼쪽 혹은 오른쪽 화살표가 눌리면 모듈러 연산을 통해 리스트의 인덱스를 변경해서
		//목표 타깃을 변경한다.
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			currentIdx = (currentIdx + 1) % alivePlayers.Count;
			ChangeTarget();
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			currentIdx = (currentIdx - 1 + alivePlayers.Count) % alivePlayers.Count;
			ChangeTarget();
		}
	}

	//카메라 위치를 업데이트하는 함수
	private void UpdateCamPosition()
	{
		//발사할 Ray 정의 및 발사 방향 계산
		RaycastHit hit;
		Vector3 rayDirection = (currentTargetCamera.transform.position - currentTargetObject.transform.position).normalized;
		float rayDistance = wallDistance; //기본 거리 설정
		//Ray가 발사해서 wall 레이어에 닿으면
		if (Physics.Raycast(currentTargetObject.transform.position, rayDirection, out hit, rayDistance, collisionLayer))
			rayDistance = hit.distance - 1f; //해당 레이어의 거리에서 -1만큼만 오프셋을 주고 거리를 계산하고
		//최종 카메라 위치를 계산한다.
		finalCameraPos = currentTargetObject.transform.position - currentTargetObject.transform.forward * rayDistance + Vector3.up * 3f;
		transform.position = Vector3.Lerp(transform.position, finalCameraPos, Time.deltaTime * SmoothSpeed);
	}

	private void ChangeTarget()
	{
		currentTargetPlayer = alivePlayers[currentIdx];
		currentPlayerName = currentTargetPlayer.NickName;
		Game_UIManager.instance.specText.GetComponentInChildren<Text>().text = "Spectating : " + currentPlayerName;
		currentTargetObject = PlayerTracker.instance.GetPlayerObject(currentTargetPlayer);
		//관전하는 플레이어 오브젝트의 꺼진 카메라를 찾는다.
		currentTargetCamera = currentTargetObject.GetComponentInChildren<Camera>(true);
	}
}
