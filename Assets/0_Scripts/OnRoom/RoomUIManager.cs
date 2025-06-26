using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System;

public class RoomUIManager : MonoBehaviourPunCallbacks
{

	//플레이어 정보를 저장하는 prefab
	public GameObject PlayerItemPrefab;
	//prefab을 생성하는 위치 선언
	public Transform playerListContent;
	//방에 들어온 플레이어들을 관리하는 딕셔너리
	Dictionary<int, RoomPlayerUI> playerUIs = new();

	//시작 버튼
	public Button StartBtn;


	private void Start()
	{
		//만약 내가 master client이면
		if (PhotonNetwork.IsMasterClient)
		{
			//게임을 시작할 권한을 주고(게임 시작 버튼을 보여준다)
			StartBtn.gameObject.SetActive(true);
			//아직 Ready 여부가 확인되지 않았기 때문에, 버튼 상호작용은 막아둔다.
			StartBtn.interactable = false;
		}
		else StartBtn.gameObject.SetActive(false);

		RefreshPlayerList();
	}

	//플레이어를 표시해주는 UI를 초기화해주는 함수
	public void RefreshPlayerList()
	{
		//현재 방에 들어온 플레이어들의 정보를 나타내는 UI 요소들을 모두 파괴한다.
		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}
		
		//딕셔너리를 초기화한다.
		playerUIs.Clear();
		
		//PhotonNetwork에서 플레이어 리스트를 받아온다.
		foreach(Player player in PhotonNetwork.PlayerList)
		{
			//프리팹을 생성하고
			GameObject item = Instantiate(PlayerItemPrefab, playerListContent);
			//해당 프리펩에 달린 RoomPlayerUI 스크립트를 가져온다.
			RoomPlayerUI ui = item.GetComponent<RoomPlayerUI>();
			//해당 스크립트를 통해 UI 요소를 초기화 한다.
			ui.SetUp(player);
			//그리고 딕셔너리에 플레이어를 삽입한다.
			playerUIs.Add(player.ActorNumber, ui);
		}

		//만약 해당 플레이어가 master client이고, 모든 플레이어가 준비 상태이면
		if (PhotonNetwork.IsMasterClient && CheckPlayerReady())
		{
			//게임 시작 버튼 상호작용을 해제해준다.
			StartBtn.interactable = true;
		}
		else StartBtn.interactable = false;
	}

	//플레이어들의 레디 상태를 확인하는 함수
	public bool CheckPlayerReady()
	{
		//각 플레이어에 저장된
		foreach(Player player in PhotonNetwork.PlayerList)
		{
			//개별 상태 값이 저장된 딕셔너리에,
			//IsReady를 키 값으로 가지고 있지 않거나, 키 값을 가지고 있는데 값이 false인 경우
			if(!player.CustomProperties.ContainsKey("IsReady") || !(bool)player.CustomProperties["IsReady"])
			{
				//false를 반환한다.
				return false;
			}
		}
		//모든 플레이어의 레디상태가 true인 경우 true를 반환
		return true;
	}

	//플레어어가 방에 들어온 경우, 호촐되는 콜백함수
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		RefreshPlayerList();
	}

	//플레이어가 방에서 나간 경우, 호출되는 콜백 함수
	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		RefreshPlayerList();
	}

	//특정 플레이어의 Properties이 Update되면 호출되는 함수
	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		//만약 해당 플레이어가 딕셔너리에 저장되어 있는 경우
		if(playerUIs.TryGetValue(targetPlayer.ActorNumber, out var ui))
		{
			//해당 UI의 정보를 초기화한다.
			ui.SetUp(targetPlayer);
		}

		//만약 플레이어의 Ready 여부가 변경되면, 플레이어들의 Ready 여부를 확인 해서
		if (PhotonNetwork.IsMasterClient && CheckPlayerReady())
		{
			//게임 시작 버튼을 활성화시켜준다.
			StartBtn.interactable = true;
		}
		else StartBtn.interactable = false;
	}

	//만약 master client가 게임을 나가거나 특정 이유로 변경된 경우
	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient가 변경되었습니다.");

		//새로운 master client 에게 게임 시작 권한을 부여 후
		if (PhotonNetwork.IsMasterClient)
		{
			StartBtn.gameObject.SetActive(true);

			//현재 ready 여부에 따라서 게임 시작 버튼 활성화 여부를 결정한다.
			if (CheckPlayerReady())
			{
				StartBtn.interactable = true;
			}
			else StartBtn.interactable = false;
		}
	}
}
