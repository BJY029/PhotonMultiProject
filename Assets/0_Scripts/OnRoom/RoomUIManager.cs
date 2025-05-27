using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class RoomUIManager : MonoBehaviourPunCallbacks
{
	//플레이어 정보를 저장하는 prefab
	public GameObject PlayerItemPrefab;
	//prefab을 생성하는 위치 선언
	public Transform playerListContent;
	//방에 들어온 플레이어들을 관리하는 딕셔너리
	Dictionary<int, RoomPlayerUI> playerUIs = new();

	private void Start()
	{
		RefreshPlayerList();
	}

	//플레이어를 표시해주는 UI를 초기화해주는 함수
	void RefreshPlayerList()
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
	}
}
