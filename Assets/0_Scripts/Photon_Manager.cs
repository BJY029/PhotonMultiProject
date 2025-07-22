using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using NUnit.Framework;
using Photon.Pun.Demo.Asteroids;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";
	
	public static Photon_Manager instance;

	public List<Player> alivePlayers = new List<Player> ();

	private void Awake()
	{
		if(instance == null)instance = this;
	}

	private void Start()
	{
		//현재 입장해서 생성된 플레이어가 master client이면
		if (PhotonNetwork.IsMasterClient)
		{
			//Dummy를 생성한다.
			DummySpawner.instance.LoadAndSpawnDummies();
		}
	}

	//연결 끊어진 경우
	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("다음의 이유로 서버 연결이 해제되었습니다 : " + cause);
	}


	//MasterClient가 변경되면 호출되는 함수
	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient가 변경되었습니다.");

		//MasterClient가 나라면
		if (PhotonNetwork.IsMasterClient)
		{
			//내 앱을 끈다.
			Application.Quit();
		}
	}

	//플레이어가 나갔을 때 모든 클라이언트에서 호출되는 함수
	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		//해당 플레이어를 Unregister 한다.
		PlayerTracker.instance.Unregister(otherPlayer);
	}
}
