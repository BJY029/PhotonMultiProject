using Photon.Pun;
using UnityEngine;
using System.Collections;

public class SeekerManager : MonoBehaviourPun
{
	//싱글턴
	public static SeekerManager Instance;

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

		CurrentHeart = HeartsMaxValue;
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

	IEnumerator DestroyAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		gameObject.GetComponent<RagdollController>().OnDeath();
		GameResultManager.instance.photonView.RPC("EndGame", RpcTarget.All, "Runner");
		//PhotonNetwork.Destroy(gameObject);
	}
}
