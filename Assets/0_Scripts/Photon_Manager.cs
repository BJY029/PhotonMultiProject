using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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

	//MasterClient가 나갈 때 호출될 함수
	public void OnMasterWantsToLeave()
	{
		//MasterClient만 수행
		if (PhotonNetwork.IsMasterClient)
		{
			//해당 방의 옵션 설정(닫고, 비활성화)
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.CurrentRoom.IsVisible = false;
			//다른 모든 클라이언트들도 강제 추방
			photonView.RPC("ForceLeaveRoom", RpcTarget.Others);
			if (PhotonNetwork.InRoom)  // 현재 방에 있는 상태인지 확인
			{
				//MasterClient를 방에서 없앰
				PhotonNetwork.LeaveRoom();
			}
		}
	}

	//다른 클라이언트들이 실행하는 강제 추방 시스템
	[PunRPC]
	void ForceLeaveRoom()
	{
		//해당 플레이어가 방에 있으면
		if (PhotonNetwork.InRoom)  // 현재 방에 있는 상태인지 확인
		{
			//방을 떠나고
			PhotonNetwork.LeaveRoom();
			//관련 UI를 띄우기 위해서 다음 코드 실행
			SceneStateManager.instance.ForcedToLeaveRoom = true;
		}
	}

	//일반 클라이언트가 방을 떠나려고 할 때
	public void ClientLeaveRoom()
	{
		//Masterclient는 실행하지 않음
		if (PhotonNetwork.IsMasterClient) return;
		//역할 정보와 준비 정보를 null로 초기화 시키고 적용한다.
		//해당 초기화는, 해당 플레이어가 방을 나갔다가 같은 방에 다시 들어왔을 때 발생될 버그를 없애기 위함
		var playerProps = new ExitGames.Client.Photon.Hashtable
		{
			{"Role", null},
			{"IsReady", false }
		};
		PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
		//해당 플레이어를 방에서 없앰
		PhotonNetwork.LeaveRoom();
	}

	//방을 떠날 때 호출되는 함수
	public override void OnLeftRoom()
	{
		//마우스 커서를 풀고
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		//해당 플레이어를 PlayerTracker에서 삭제시킨다.
		PlayerTracker.instance?.Unregister(PhotonNetwork.LocalPlayer);
		//그리고 최종적으로 LobbyScene을 불러온다.
		SceneManager.LoadScene("LobbyScene");
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
		photonView.RPC("ForceLeaveRoom", RpcTarget.Others);
	}

	//플레이어가 나갔을 때 모든 클라이언트에서 호출되는 함수
	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
		{
			if (otherPlayer.CustomProperties.TryGetValue("Role", out object role))
			{
				//Seeker인 경우
				if (role.ToString() == "Seeker")
				{
					//따라서, 게임을 종료하고 Seeker로 게임 우승을 처리한다.
					GameResultManager.instance.photonView.RPC("EndGame", RpcTarget.All, "Runner");
				}
			}
		}
	}


}
