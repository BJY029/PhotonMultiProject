using Photon.Pun;
using UnityEngine;
using System.Collections;
using StarterAssets;
using System;

public class SeekerManager : MonoBehaviourPun
{
	//Seeker 스킬 사용시 실행될 이벤트들
	public static event Action<bool> OnRevealChange;
	//싱글턴
	public static SeekerManager Instance;

	//Seeker 스킬 관련 정보들
	[SerializeField] private float SpecialSkillDelay = 150f;
	[SerializeField] private float SkillTimer = 5f;
	private float chargeTimer;
	private bool SkillCharged;


	private void Awake()
	{
		if (Instance == null) Instance = this;

		if (!photonView.IsMine) return;
		photonView.RPC(nameof(OnRegister), RpcTarget.AllBuffered, photonView.ViewID);
	}

	[PunRPC]
	public void OnRegister(int viewID)
	{
		PhotonView view = PhotonView.Find(viewID);
		if (view != null && !PlayerTracker.instance.GetAlivePlayers().Contains(view.Owner))
		{
			PlayerTracker.instance.Register(view.Owner, view.gameObject);
		}
	}

	[PunRPC]
	public void OnUnregister(int viewID)
	{
		PhotonView view = PhotonView.Find(viewID);
		if (view != null)
		{
			PlayerTracker.instance.Unregister(view.Owner);
		}
	}

	//Seeker 체력
	[SerializeField]
	private float HeartsMaxValue = 100;
	private float CurrentHeart;

	private void Start()
	{
		//체력 갱신
		Game_UIManager.instance.Hearts.maxValue = HeartsMaxValue;
		Game_UIManager.instance.Hearts.value = HeartsMaxValue;

		chargeTimer = SpecialSkillDelay + 1f;
		SkillCharged = false;

		CurrentHeart = HeartsMaxValue;
	}

	private void Update()
	{
		//자기 자신만 실행
		if (!photonView.IsMine) return;
		//아직 쿨타임이 안돌았으면
		if(chargeTimer < SpecialSkillDelay)
		{
			chargeTimer += Time.deltaTime;
			//쿨타임에 맞춰서 blur 크기 조정
			Game_UIManager.instance.blurSkill.transform.localScale = 
				new Vector3(1.0f, (SpecialSkillDelay - chargeTimer) / SpecialSkillDelay, 1.0f);
		}
		else
		{
			//쿨타임이 다 돌았는데, UI가 안바뀐 경우
			if (!SkillCharged)
			{
				//UI 업데이트
				Game_UIManager.instance.SpecialSkillScopeCharged();
				SkillCharged = true;
			}
		}

		//Q가 눌리고, 스킬이 준비된 경우
		if (Input.GetKeyDown(KeyCode.Q) && SkillCharged && !RoleManager.instance.spawning)
			StartCoroutine(ActiveSpecialSkill());
	}

	IEnumerator ActiveSpecialSkill()
	{
		//스킬 관련 정보 초기화
		chargeTimer = 0f;
		SkillCharged = false;
		Game_UIManager.instance.SpecialSkillScopeInit();
		
		//관련 UI 띄움
		Game_UIManager.instance.photonView.RPC("SeekerActiveSkillAlert", RpcTarget.Others);
		//등록된 이벤트 실행
		OnRevealChange?.Invoke(true);
		//대기후
		yield return new WaitForSeconds(SkillTimer);
		//이벤트 끄기
		OnRevealChange?.Invoke(false);
	}

	//Dummy를 쏠 경우 호출되는 함수
	[PunRPC]
	public void GetDamagedOnDummy(float value)
	{
		//전달 받은 값에 의해서 체력이 감소된다.
		CurrentHeart -= value;
		if (CurrentHeart <= 0)
		{
			CurrentHeart = 0;
			Debug.Log("Seeker가 자멸했습니다.");
			Game_UIManager.instance.Hearts.value = CurrentHeart;
			Game_UIManager.instance.Minimap.SetActive(false);

			photonView.RPC(nameof(OnUnregister), RpcTarget.AllBuffered, photonView.ViewID);
			StartCoroutine(DestroyAfterDelay(0.1f));
		}
		Game_UIManager.instance.Hearts.value = CurrentHeart;
	}

	//회복 함수
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

	[PunRPC]
	public void ActiveMadMod()
	{
		SeekerGun sg = gameObject.GetComponent<SeekerGun>();
		sg.chargeDelay /= 2;

		ThirdPersonController tpc = gameObject.GetComponent<ThirdPersonController>();
		tpc.ChangeStaminaToMadMod();
	}
	IEnumerator DestroyAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		gameObject.GetComponent<RagdollController>().OnDeath();
		GameResultManager.instance.photonView.RPC("EndGame", RpcTarget.All, "Runner");
		//PhotonNetwork.Destroy(gameObject);
	}
}
