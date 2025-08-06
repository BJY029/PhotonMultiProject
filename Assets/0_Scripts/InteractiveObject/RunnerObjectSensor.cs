using Photon.Pun;
using UnityEngine;

public class RunnerObjectSensor : MonoBehaviourPun
{


	//발사할 Ray 거리
	[SerializeField]
	private float RayDistance = 1.5f;
	//Runner 플레이어 카메라
	public Camera cam;
	//감지할 레이어
	public LayerMask interactableLayer;
	//현재 목표 타깃
	private GameObject currentTarget;
	//현재 목표 타깃의 캔버스
	private Canvas currentCanvas;

	private void Start()
	{
		//초기화
		currentTarget = null;
		currentCanvas = null;
	}

	//현재 KeyPad와 상호작용 정보를 저장하는 플래그
	private bool hasTriggeredInteraction = false;

	private void Update()
	{
		if (!photonView.IsMine) return;
		// 자동 상호작용 종료 감지
		if (hasTriggeredInteraction && currentTarget != null)
		{
			var switchKeypad = currentTarget.GetComponent<SwitchToKeypad>();
			if (switchKeypad != null && !switchKeypad.isInteraction)
			{
				hasTriggeredInteraction = false; // 다시 상호작용 가능
				InitCurrentCanvas(); // Ray가 다시 닿도록 초기화
			}
		}

		//만약 현재 상호작용 중인 경우, Ray 발사를 중지한다.
		if (hasTriggeredInteraction)
		{
			HideCurrentCanvas();
			return;
		}

		//Runner Cam 기준 Ray를 발사해서 마우스 위치 반환
		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		//정해진 조건으로 Ray를 발사해서
		if (Physics.Raycast(ray, out hit, RayDistance, interactableLayer))
		{
			//오브젝트를 받아온다.
			GameObject hitObject = hit.collider.gameObject;

			//만약 충돌한 오브젝트가 현재 오브젝트와 다른경우(null일 경우)
			if (hitObject != currentTarget)
			{
				//현재 재생중인 캔버스를 초기화 후
				HideCurrentCanvas();
				InitCurrentCanvas();
				//새롭게 캔버스를 설정한다.
				currentTarget = hitObject;
				currentCanvas = currentTarget.GetComponentInChildren<Canvas>(true);
				if (currentCanvas != null)
					currentCanvas.gameObject.SetActive(true);
			}
			//키 E가 눌렸고, 목표 오브젝트가 존재하며, 상호작용 중이 아니라면!
			if (Input.GetKeyDown(KeyCode.E) && currentTarget != null && !hasTriggeredInteraction)
			{
				//만약 Ray한 오브젝트가 KeyPad인 경우
				if (currentTarget.CompareTag("KeyPad"))
				{
					//상호작용 플래그를 재설정하고
					hasTriggeredInteraction = true; // 중복 방지

					//해당 키패드에 붙어있는 KeyPad의 스크립트의 Mod_KeyPad를 호출한다.
					//해당 스크립트 부분은, 추후에 다른 상호작용 오브젝트가 생길 경우 손봐야 할 필요가 있다.

					currentTarget.GetComponent<SwitchToKeypad>().Mode_KeyPad(cam, gameObject);

					Debug.Log("Mode_KeyPad");
				}
				//Ray의 오브젝트가 Labtop인 경우
				else if (currentTarget.CompareTag("Labtop"))
				{
					//해당 오브젝트의 PhotonView를 불러와서
					PhotonView pv = currentTarget.GetComponent<PhotonView>();
					//해당 PhotonView가 모든 클라이언트를 대상으로 해당 함수를 실행하도록 한다.
					pv.RPC("ChangeToTurnoffMat", RpcTarget.All);
					//그리고, 모든 노트북이 꺼졌는지 확인한다.
					GameResultManager.instance.photonView.RPC("CheckLabtop", RpcTarget.MasterClient);
				}
			}
		}
		else
		{
			//감지된 Layer가 없으면 초기화
			HideCurrentCanvas();
			InitCurrentCanvas();
			hasTriggeredInteraction = false; // 감지 해제 시 다시 허용
		}

	}

	//캔버스를 초기화 하는 함수
	void HideCurrentCanvas()
	{
		if(currentCanvas != null)
		{
			currentCanvas.gameObject.SetActive(false);
		}
	}

	void InitCurrentCanvas()
	{
		currentCanvas = null;
		currentTarget = null;
	}
}
