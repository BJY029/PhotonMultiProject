using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class RoomUIManager : MonoBehaviourPunCallbacks
{
	//�÷��̾� ������ �����ϴ� prefab
	public GameObject PlayerItemPrefab;
	//prefab�� �����ϴ� ��ġ ����
	public Transform playerListContent;
	//�濡 ���� �÷��̾���� �����ϴ� ��ųʸ�
	Dictionary<int, RoomPlayerUI> playerUIs = new();

	private void Start()
	{
		RefreshPlayerList();
	}

	//�÷��̾ ǥ�����ִ� UI�� �ʱ�ȭ���ִ� �Լ�
	void RefreshPlayerList()
	{
		//���� �濡 ���� �÷��̾���� ������ ��Ÿ���� UI ��ҵ��� ��� �ı��Ѵ�.
		foreach(Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}
		
		//��ųʸ��� �ʱ�ȭ�Ѵ�.
		playerUIs.Clear();
		
		//PhotonNetwork���� �÷��̾� ����Ʈ�� �޾ƿ´�.
		foreach(Player player in PhotonNetwork.PlayerList)
		{
			//�������� �����ϰ�
			GameObject item = Instantiate(PlayerItemPrefab, playerListContent);
			//�ش� �����鿡 �޸� RoomPlayerUI ��ũ��Ʈ�� �����´�.
			RoomPlayerUI ui = item.GetComponent<RoomPlayerUI>();
			//�ش� ��ũ��Ʈ�� ���� UI ��Ҹ� �ʱ�ȭ �Ѵ�.
			ui.SetUp(player);
			//�׸��� ��ųʸ��� �÷��̾ �����Ѵ�.
			playerUIs.Add(player.ActorNumber, ui);
		}
	}

	//�÷��� �濡 ���� ���, ȣ�͵Ǵ� �ݹ��Լ�
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		RefreshPlayerList();
	}

	//�÷��̾ �濡�� ���� ���, ȣ��Ǵ� �ݹ� �Լ�
	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		RefreshPlayerList();
	}

	//Ư�� �÷��̾��� Properties�� Update�Ǹ� ȣ��Ǵ� �Լ�
	public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
		//���� �ش� �÷��̾ ��ųʸ��� ����Ǿ� �ִ� ���
		if(playerUIs.TryGetValue(targetPlayer.ActorNumber, out var ui))
		{
			//�ش� UI�� ������ �ʱ�ȭ�Ѵ�.
			ui.SetUp(targetPlayer);
		}
	}
}
