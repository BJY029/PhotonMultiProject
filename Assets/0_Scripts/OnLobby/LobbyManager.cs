using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
	private string gameVersion = "1";
	public Text statusText;
	public Text nicknameText;
	public InputField nicknameInput;

	//master client�� ���� �ű��, �ش�Ǵ� �ٸ� �����鵵 �ű⵵�� �ϱ� ���� ����
	private void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
	}


	private void Start()
	{
		//���� ���� �õ�
		Connect();
		statusText.text = "Connection to server...";
	}

	public void Connect()
	{
		//������ �����ϰ�
		PhotonNetwork.GameVersion = gameVersion;
		//���� ������ �õ��Ѵ�.
		PhotonNetwork.ConnectUsingSettings();
	}

	//Master ������ ����� ���
	public override void OnConnectedToMaster()
	{
		//Player�� �г����� ���� ������ �����ִ� �÷��̾� ���� ��ȣ�� �ٿ��� �ӽ÷� �ο��Ѵ�.
		PhotonNetwork.NickName = "Player " + PhotonNetwork.PlayerList.Length;
		nicknameText.text = PhotonNetwork.NickName;

		//Master ������ ���� �� ��, �ٷ� �κ� ����õ��Ѵ�.
		statusText.text = "Connected. Joining lobby...";
		PhotonNetwork.JoinLobby();
	}

	//�κ� ����Ǹ� ȣ��Ǵ� �Լ�
	public override void OnJoinedLobby()
	{
		statusText.text = "Lobby joined.";
	}

	//�� �̸��� ��й�ȣ�� ���� ������, �ش� ������� ���� �����ϴ� �Լ�
	public void CreateRoomWithPassword(string RoomName, string password)
	{
		//�濡 ������ �ɼ��� �����ϴ� RoomOption ��ü ����
		RoomOptions options = new RoomOptions();
		//�ִ� �ο� ���� ����(4������ ����)
		options.MaxPlayers = 6;
		//CustomerRoomProperties�� Room�� ����� key-value �����͸� ������ �� �ִ� �ؽ����̺��̴�.
		options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
		{
			{"Password", password}//Ű "Password"�� �ش� ����� �����Ѵ�.
		};

		//Photon ������ �� ���� ��û�� ������.
			//RoomName : ������ �Է��� �� �̸�
			//options : ������ ������ Room �ɼ�
			//TypedLobby.Default : �⺻ �κ� ���� ���(�ش� ������ ������ �κ񿡼� �� ���� �� �ִ�.)
		PhotonNetwork.CreateRoom(RoomName, options, TypedLobby.Default);
	}

	//�濡 ���������� �����ϸ� ȣ��Ǵ� �Լ�
	public override void OnJoinedRoom()
	{
		Debug.Log("�� ���� �� ���� ����!, RoomScene���� �̵��մϴ�.");
		//Room ������ �̵��Ѵ�.
		PhotonNetwork.LoadLevel("RoomScene");
	}

	//�κ񿡼� �г��� ������ �ϸ� �ݿ����ִ� �Լ�
	public void OnInputEnd()
	{
		string name = nicknameInput.text;
		PhotonNetwork.NickName = name;
		nicknameText.text = PhotonNetwork.NickName;
	}
}
