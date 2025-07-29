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

	//강제종료 플래그
	private bool F4 = false;
	//안전한 종료를 위한 플래그
	private bool isLeaving = false;


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


	//방을 떠나면 중이면, LeaveRoom을 중복호출하지 않도록 하기 위해 다음 함수 사용
	private void LeaveRoomSafely()
	{
		if (!isLeaving && PhotonNetwork.InRoom)
		{
			isLeaving = true;
			PhotonNetwork.LeaveRoom();
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
			//해당 플레이어의 정보를 초기화하고
			InitPlayerCustomProperties();
			//다른 모든 클라이언트들도 강제 추방
			photonView.RPC("ForceLeaveRoom", RpcTarget.Others);
			if (PhotonNetwork.InRoom)  // 현재 방에 있는 상태인지 확인
			{
				//MasterClient를 방에서 없앰
				LeaveRoomSafely();
			}
		}
	}

	//다른 클라이언트들이 실행하는 강제 추방 시스템
	[PunRPC]
	void ForceLeaveRoom()
	{
		//강제추방 플래그 활성화
		F4 = true;
		//해당 플레이어가 방에 있으면
		if (PhotonNetwork.InRoom)  // 현재 방에 있는 상태인지 확인
		{
			InitPlayerCustomProperties();
			//방을 떠나고
			LeaveRoomSafely();
			//관련 UI를 띄우기 위해서 다음 코드 실행
			SceneStateManager.instance.ForcedToLeaveRoom = true;
		}
	}

	//일반 클라이언트가 방을 떠나려고 할 때
	public void ClientLeaveRoom()
	{
		//Masterclient는 실행하지 않음
		if (PhotonNetwork.IsMasterClient) return;

		if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object role))
		{
			//Seeker인 경우
			if (role.ToString() == "Seeker")
			{
				//따라서, 게임을 종료하고 Seeker로 게임 우승을 처리한다.
				GameResultManager.instance.photonView.RPC("EndGame", RpcTarget.Others, "Runner");
			}
		}

		InitPlayerCustomProperties();
		//해당 플레이어를 방에서 없앰
		LeaveRoomSafely();
	}

	//방을 떠날 때 호출되는 함수
	public override void OnLeftRoom()
	{
		//마우스 커서를 풀고
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		//자기 자신을 플레이어 리스트에서 제거(필요 없긴 함)
		PlayerTracker.instance?.Unregister(PhotonNetwork.LocalPlayer);

		//InitPlayerCustomProperties();
		//그리고 최종적으로 LobbyScene을 불러온다.
		SceneManager.LoadScene("LobbyScene");
	}


	//연결 끊어진 경우
	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("다음의 이유로 서버 연결이 해제되었습니다 : " + cause);
	}

	//플레이어가 방을 떠날 때 모든 클라이언트들에서 호출되는 함수
	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		//만약 강제종료 중이라면, 아래 검사를 실시하지 않는다.
		if (F4) return;
		//나간 플레이어를 플레이어 리스트에서 제거
		PlayerTracker.instance?.Unregister(otherPlayer);
		//나간 플레이어가 Seeker인 경우 처리하기 위한 코드
		if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
		{
			if (otherPlayer.CustomProperties.TryGetValue("Role", out object role))
			{
				//Seeker인 경우
				if (role.ToString() == "Seeker")
				{
					//나간 플레이어가 MasterClient이면 승패 처리 하지 않음
					if (otherPlayer.CustomProperties.TryGetValue("IsMaster", out object isMaster)
					&& isMaster is bool && (bool)isMaster) return;

						//따라서, 게임을 종료하고 Seeker로 게임 우승을 처리한다.
						GameResultManager.instance.photonView.RPC("EndGame", RpcTarget.All, "Runner");
				}
			}
		}
	}

	//MasterClient가 변경되면 호출되는 함수
	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		//강제 종료 플래그 활성화
		F4 = true;
		//MasterClient를 부여 받은 플레이어가 차례대로 다른 클라이언트들을 모두 추방시키고
		//자신을 마지막으로 내보내는 코드
		if (PhotonNetwork.LocalPlayer.Equals(newMasterClient))
		{
			Debug.Log("강제종료된 Master → 내가 방을 폭파함");
			//관련 방 설정을 진행하고
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.CurrentRoom.IsVisible = false;
			//다른 클라이언트들 강제 추방
			photonView.RPC("ForceLeaveRoom", RpcTarget.Others);
			//자기 자신의 정보 초기화
			InitPlayerCustomProperties();
			//자기 자신 나가기
			LeaveRoomSafely();
			//관련 UI 활성화 플래그
			SceneStateManager.instance.ForcedToLeaveRoom = true;
		}
	}

	//플레이어의 CustomProperties를 초기화 하는 함수
	private void InitPlayerCustomProperties()
	{
		//이미 방을 떠난 경우 수행하지 않는다.
		if (!PhotonNetwork.InRoom) return;
		//역할 정보와 준비 정보를 null로 초기화 시키고 적용한다.
		//해당 초기화는, 해당 플레이어가 방을 나갔다가 같은 방에 다시 들어왔을 때 발생될 버그를 없애기 위함
		var playerProps = new ExitGames.Client.Photon.Hashtable
		{
			{"Role", null},
			{"IsReady", false },
			{"IsMaster", false }
		};
		PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
	}
}
