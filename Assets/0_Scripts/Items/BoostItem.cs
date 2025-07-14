using UnityEngine;
using Photon.Pun;
using System.Collections;
using StandardAssets.Characters.ThirdPerson.PunDemos;
using StarterAssets;

public class BoostItem : MonoBehaviourPun
{
	//회전 속도
	[SerializeField]
	private float rotationSpeedX, rotationSpeedY = 0.5f, rotationSpeedZ;
	//진폭
	[SerializeField]
	private float amplitude = 0.05f;
	//속도
	[SerializeField]
	private float frequency = 1f;
	//기본 위치
	[SerializeField]
	private Vector3 startPos;
	//적용할 부스터 시간
	[SerializeField]
	private float BoostTime = 7f;

	//접촉한 플레이어
	private GameObject playerObj;
	private void Start()
	{
		startPos = transform.position;
	}

	//회전 적용
	private void Update()
	{
		transform.Rotate(rotationSpeedX, rotationSpeedY, rotationSpeedZ);
		floatingEffect();
	}

	private void floatingEffect()
	{
		float yOffset = Mathf.Sin(Time.deltaTime * frequency) * amplitude;
		transform.position = startPos + new Vector3(0, yOffset, 0);
	}


	private void OnTriggerEnter(Collider other)
	{
		//Runner 혹은 Seeker가 접촉한ㄱ ㅕㅇ우
		GameObject playerObj = other.gameObject;
		if (!playerObj.CompareTag("Runner") && !playerObj.CompareTag("Seeker")) return;
		//해당 부스터를 적용하기 위한 코루틴을 실행시키는 함수를 RPC로 호출한다.
		playerObj.GetComponent<PhotonView>()?.RPC("RPC_ApplyBoost", playerObj.GetComponent<PhotonView>().Owner, BoostTime);
		//해당 아이템을 처리하기 위해 RPC 호출
		ItemManager.Instance.photonView.RPC("RPC_PickUpItem", RpcTarget.MasterClient, playerObj.transform.position);
	}
}
