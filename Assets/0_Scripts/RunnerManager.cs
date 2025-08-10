using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using StarterAssets;
using System.Collections;

public class RunnerManager : MonoBehaviourPun
{
	//싱글턴
	public static RunnerManager instance;

	//skill 관련 정보
	[SerializeField] private float SpecialSkillDelay = 150f;
	[SerializeField] private float SkillTimer = 5f;
	private float chargeTimer;
	private bool SkillCharged;
	private bool unbeatable;
	//skill 사용시 적용할 머테리얼 정보
	private Material originMaterial;
	private Material GoldMaterial;
	private SkinnedMeshRenderer skinnedMeshRenderer;


	private void Awake()
	{
		if(instance == null) instance = this;

		//자신의 경우에만 실행
		if (!photonView.IsMine) return;
		//모든 클라이언트에게 자신의 정보를 PlayerTracker의 딕셔너리에 삽입할 수 있또록 RPC로 호출
		photonView.RPC(nameof(OnRegister), RpcTarget.AllBuffered, photonView.ViewID);
	}


	//플레이어 생성 시 호출되는 RPC 함수
	[PunRPC]
	public void OnRegister(int viewID)
	{
		//전달받은 view id를 기반으로 photonview를 찾는다.
		PhotonView view = PhotonView.Find(viewID);
		//해당 view가 존재하고, 딕셔너리에 존재하지 않는 경우
		if(view != null && !PlayerTracker.instance.GetAlivePlayers().Contains(view.Owner)) {
			//해당 플레이어 정보를 각 클라이언트들이 저장한다.
			PlayerTracker.instance.Register(view.Owner, view.gameObject);
		}
	}

	//플레이어 삭제 시 호출되는 RPC 함수
	[PunRPC]
	public void OnUnregister(int viewID)
	{
		//viewID를 기반으로 photon view를 찾는다.
		PhotonView view = PhotonView.Find(viewID);
		if (view != null)
		{
			//view가 존재하면, 해당 view를 기반으로 플레이어를 딕셔너리에서 삭제한다.
			PlayerTracker.instance.Unregister(view.Owner);
		}
	}

	//해당 Runner의 체력
	[SerializeField]
	private float HeartsMaxValue = 100;
	private float CurrentHeart;

	//Runner의 체력 슬라이더 값 초기화
	private void Start()
	{
		Game_UIManager.instance.Hearts.maxValue = HeartsMaxValue;
		Game_UIManager.instance.Hearts.value = HeartsMaxValue;
		skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		originMaterial = skinnedMeshRenderer.material;
		GoldMaterial = Resources.Load<Material>("Gold");

		chargeTimer = SpecialSkillDelay + 1f;
		SkillCharged = false;
		unbeatable = false;

		CurrentHeart = HeartsMaxValue;
	}

	private void Update()
	{
		//나 자신만 실행
		if (!photonView.IsMine) return;
		//아직 쿨타임 도는 중이면
		if (chargeTimer < SpecialSkillDelay)
		{
			chargeTimer += Time.deltaTime;
			//관련 UI 처리
			Game_UIManager.instance.blurSkill.transform.localScale =
				new Vector3(1.0f, (SpecialSkillDelay - chargeTimer) / SpecialSkillDelay, 1.0f);
		}
		else
		{
			//쿨타임 돌았는데, UI가 안바뀐 경우
			if (!SkillCharged)
			{
				//UI 업데이트
				Game_UIManager.instance.SpecialSkillStarCharged();
				SkillCharged = true;
			}
		}

		//쿨타임 돌고 Q가 눌리면
		if (Input.GetKeyDown(KeyCode.Q) && SkillCharged && !RoleManager.instance.spawning)
			StartCoroutine(ActiveSpecialSkill());
	}

	IEnumerator ActiveSpecialSkill()
	{
		//스킬 관련 정보 업데이트
		chargeTimer = 0f;
		SkillCharged = false;
		Game_UIManager.instance.SpecialSkillStarInit();

		//관련 플레그 활성화
		unbeatable = true;

		//머테리얼 변경
		skinnedMeshRenderer.material = GoldMaterial;
		photonView.RPC(nameof(ChangeMaterialToGold), RpcTarget.Others, photonView.ViewID);
		
		//대기
		yield return new WaitForSeconds(SkillTimer);

		//머테리얼 원래대로
		skinnedMeshRenderer.material = originMaterial;
		photonView.RPC(nameof(ChangeMaterialToOrign), RpcTarget.Others, photonView.ViewID);
		//관련 플래그 비활성화
		unbeatable = false;
	}

	//머테리얼을 변경하는 RPC 함수
	[PunRPC]
	void ChangeMaterialToGold(int playerId)
	{
		PhotonView pv = PhotonView.Find(playerId);
		if (pv == null) return;
		GameObject Player = pv.gameObject;

		Player.GetComponent<RunnerManager>().skinnedMeshRenderer.material = GoldMaterial;
	}

	[PunRPC]
	void ChangeMaterialToOrign(int playerId)
	{
		PhotonView pv = PhotonView.Find(playerId);
		if (pv == null) return;
		GameObject Player = pv.gameObject;

		Player.GetComponent<RunnerManager>().skinnedMeshRenderer.material = originMaterial;
	}

	//RPC 함수
	//Seeker에게 총을 맞으면 호출된다.
	[PunRPC]
	public void GetDamagedBySeeker(float value)
	{
		if (unbeatable) return;

		//전달받은 값에 맞게 체력 감소 시킨다.
		CurrentHeart -= value;
		//만약 체력이 음수가 되면
		if(CurrentHeart <= 0)
		{
			//체력 값 초기화 후
			CurrentHeart = 0;
			Debug.Log("Runner가 Seeker에게 잡혔습니다.");
			//자기 자신만 실행
			if(photonView.IsMine)
			{
				//관련 UI를 비활성화 하고
				Game_UIManager.instance.UICanvas.SetActive(false);
				Game_UIManager.instance.Minimap.SetActive(false);
				//해당 플레이어의 카메라와 움직임 또한 제한한다.
				ThirdPersonController TPC =GetComponent<ThirdPersonController>();
				TPC._mainCamera.SetActive(false);
				TPC.enabled = false;

				//RPC로 해당 플레이어를 모든 클라이언트의 딕셔너리에서 제거시킨다.
				photonView.RPC(nameof(OnUnregister), RpcTarget.AllBuffered, photonView.ViewID);
				//관전 함수를 실행한다.
				SpectatorManager.instance.ActiveUIBeforeSpectating();
				//딕셔너리의 결과가 반영되기까지 약간의 딜레이를 준다.
				StartCoroutine(DestroyAfterDelay(0.1f));
			}
		}
		//체력바 또한 갱신한다.
		Game_UIManager.instance.Hearts.value = CurrentHeart;
	}

	[PunRPC]
	public void HealHearts(float value)
	{
		CurrentHeart += value;
        if (CurrentHeart > HeartsMaxValue)
        {
			CurrentHeart = HeartsMaxValue;
        }
		Game_UIManager.instance.Hearts.value = CurrentHeart;
	}

	//일정 이상 후 해당 플레이어를 네트워크상에서 삭제하는 함수
	IEnumerator DestroyAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		PhotonNetwork.Destroy(gameObject);
	}

}
