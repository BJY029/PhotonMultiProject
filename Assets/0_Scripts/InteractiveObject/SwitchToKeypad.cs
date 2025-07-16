using Photon.Pun;
using StarterAssets;
using UnityEngine;

public class SwitchToKeypad : MonoBehaviourPun
{
	//싱글턴화
	public static SwitchToKeypad instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//KeyPad와 상호작용 중인 플레이어 오브젝트를 저장
	private GameObject player;

	//KeyPad에 달린 카메라
	private Camera interactiveCam;
	//본래 Player를 비추는 카메라
	private Camera requestedCam;
	//KeyPad 전용 Canvas
	public GameObject canvas;
	//현재 상호작용 정보를 저장하는 플래그
	public bool isInteraction = false;


	private void Start()
	{
		//KeyPad 카메라 
		interactiveCam = transform.GetComponentInChildren<Camera>();
	}

	private void Update()
	{
		//현재 상호작용 중이고, E가 눌린 경우
		if(isInteraction && Input.GetKeyDown(KeyCode.E))
		{
			//메인 카메라로 돌아가는 함수 실행
			SwitchToMainCam();
		}
	}

	//KeyPad를 변경시키는 함수
	public void Mode_KeyPad(Camera cam, GameObject player)
	{
		//상호작용 중인 플레이어의 카메라를 저장
		requestedCam = cam;
		//상호작용 중인 플레이어 저장
		this.player = player;

		//플레이어 카메라 비활성화
		requestedCam.enabled = false;
		//플레이어 컨트롤러 비활성화(수정 필요)
		player.GetComponent<ThirdPersonController>().enabled = false;
		//KeyPad 카메라 활성화
		interactiveCam.enabled = true;
		//상호작용 플래그 변경
		isInteraction = true;
		
		//KeyPad 전용 캔버스 활성화
		canvas.SetActive(true);
		//커서 락 풀고 표시
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		//플레이어 조준선 비활성화
		Game_UIManager.instance.CrossHair.SetActive(false);
		//KeyPad 텍스트 초기화
		photonView.RPC("RPC_ClearInput", RpcTarget.All);
	}

	//다시 플레이어 카메라로 돌아가는 함수
	public void SwitchToMainCam()
	{
		//KeyPad 카메라 비활성화
		interactiveCam.enabled = false;
		//플레이어 카메라 활성화
		requestedCam.enabled = true;
		//플레이어 컨트롤러 활성화
		player.GetComponent<ThirdPersonController>().enabled = true;
		//상호작용 플레그 변경
		isInteraction = false;
		
		//각종 저장 내역 초기화
		requestedCam = null;
		player = null;

		//KeyPad 캔버스 비활성화
		canvas.SetActive(false);
		//커서 락 및 비표시
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		//조준선 활성화
		Game_UIManager.instance.CrossHair.SetActive(true);
	}
}
