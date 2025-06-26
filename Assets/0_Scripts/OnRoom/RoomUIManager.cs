using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System;

public class RoomUIManager : MonoBehaviourPunCallbacks
{

	//�÷��̾� ������ �����ϴ� prefab
	public GameObject PlayerItemPrefab;
	//prefab�� �����ϴ� ��ġ ����
	public Transform playerListContent;
	//�濡 ���� �÷��̾���� �����ϴ� ��ųʸ�
	Dictionary<int, RoomPlayerUI> playerUIs = new();

	//���� ��ư
	public Button StartBtn;


	private void Start()
	{
		//���� ���� master client�̸�
		if (PhotonNetwork.IsMasterClient)
		{
			//������ ������ ������ �ְ�(���� ���� ��ư�� �����ش�)
			StartBtn.gameObject.SetActive(true);
			//���� Ready ���ΰ� Ȯ�ε��� �ʾұ� ������, ��ư ��ȣ�ۿ��� ���Ƶд�.
			StartBtn.interactable = false;
		}
		else StartBtn.gameObject.SetActive(false);

		RefreshPlayerList();
	}

	//�÷��̾ ǥ�����ִ� UI�� �ʱ�ȭ���ִ� �Լ�
	public void RefreshPlayerList()
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

		//���� �ش� �÷��̾ master client�̰�, ��� �÷��̾ �غ� �����̸�
		if (PhotonNetwork.IsMasterClient && CheckPlayerReady())
		{
			//���� ���� ��ư ��ȣ�ۿ��� �������ش�.
			StartBtn.interactable = true;
		}
		else StartBtn.interactable = false;
	}

	//�÷��̾���� ���� ���¸� Ȯ���ϴ� �Լ�
	public bool CheckPlayerReady()
	{
		//�� �÷��̾ �����
		foreach(Player player in PhotonNetwork.PlayerList)
		{
			//���� ���� ���� ����� ��ųʸ���,
			//IsReady�� Ű ������ ������ ���� �ʰų�, Ű ���� ������ �ִµ� ���� false�� ���
			if(!player.CustomProperties.ContainsKey("IsReady") || !(bool)player.CustomProperties["IsReady"])
			{
				//false�� ��ȯ�Ѵ�.
				return false;
			}
		}
		//��� �÷��̾��� ������°� true�� ��� true�� ��ȯ
		return true;
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

		//���� �÷��̾��� Ready ���ΰ� ����Ǹ�, �÷��̾���� Ready ���θ� Ȯ�� �ؼ�
		if (PhotonNetwork.IsMasterClient && CheckPlayerReady())
		{
			//���� ���� ��ư�� Ȱ��ȭ�����ش�.
			StartBtn.interactable = true;
		}
		else StartBtn.interactable = false;
	}

	//���� master client�� ������ �����ų� Ư�� ������ ����� ���
	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient�� ����Ǿ����ϴ�.");

		//���ο� master client ���� ���� ���� ������ �ο� ��
		if (PhotonNetwork.IsMasterClient)
		{
			StartBtn.gameObject.SetActive(true);

			//���� ready ���ο� ���� ���� ���� ��ư Ȱ��ȭ ���θ� �����Ѵ�.
			if (CheckPlayerReady())
			{
				StartBtn.interactable = true;
			}
			else StartBtn.interactable = false;
		}
	}
}
