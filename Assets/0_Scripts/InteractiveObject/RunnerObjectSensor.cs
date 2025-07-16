using UnityEngine;

public class RunnerObjectSensor : MonoBehaviour
{
	//발사할 Ray 거리
	[SerializeField]
	private float RayDistance = 10f;
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
		//만약 현재 상호작용 중인 경우, Ray 발사를 중지한다.
		if (SwitchToKeypad.instance.isInteraction)
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
				//새롭게 캔버스를 설정한다.
				currentTarget = hitObject;
				currentCanvas = currentTarget.GetComponentInChildren<Canvas>(true);
				if (currentCanvas != null)
					currentCanvas.gameObject.SetActive(true);
			}
			//키 E가 눌렸고, 목표 오브젝트가 존재하며, 상호작용 중이 아니라면!
			if (Input.GetKeyDown(KeyCode.E) && currentTarget != null && !hasTriggeredInteraction)
			{
				//상호작용 플래그를 재설정하고
				hasTriggeredInteraction = true; // 중복 방지
				
				//해당 키패드에 붙어있는 KeyPad의 스크립트의 Mod_KeyPad를 호출한다.
				//해당 스크립트 부분은, 추후에 다른 상호작용 오브젝트가 생길 경우 손봐야 할 필요가 있다.

				currentTarget.GetComponent<SwitchToKeypad>().Mode_KeyPad(cam, gameObject);

				Debug.Log("Mode_KeyPad");
			}
		}
		else
		{
			//감지된 Layer가 없으면 초기화
			HideCurrentCanvas();
			hasTriggeredInteraction = false; // 감지 해제 시 다시 허용
		}
	}

	//캔버스를 초기화 하는 함수
	void HideCurrentCanvas()
	{
		if(currentCanvas != null)
		{
			currentCanvas.gameObject.SetActive(false);
			currentCanvas = null;
			currentTarget = null;
		}
	}
}
