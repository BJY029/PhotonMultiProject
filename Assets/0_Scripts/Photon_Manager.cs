using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
	[Tooltip("���� �ִ� �÷��̾� ���� ��.")]
	[SerializeField]
	private byte maxPlayersPerRoom = 4;

    private string gameVersion = "1";

	public GameObject camera;

	//�ش� ������ ���� PhotonNetwork.LoadLevel() ȣ�� ��,
	//������ Ŭ���̾�Ʈ�� ���� �ε��ϸ�, �ش� �濡 �ִ� �ٸ� Ŭ���̾�Ʈ�鵵 ���� ���� �ε��ϰ� �����.
	private void Awake()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	private void Start()
	{
		Connect();
	}

	public void Connect()
	{
		//�̹� Photon ������ ����Ǿ� �ִ� ���
		if (PhotonNetwork.IsConnected)
		{
			//���� �� ������ �õ��Ѵ�.
			PhotonNetwork.JoinRandomRoom();
		}
		else //������ ���� ������� ���� ���
		{
			//������ �����ϰ�
			PhotonNetwork.GameVersion = gameVersion;
			//���� ������ �õ��Ѵ�.
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	//Master ������ ����� ���
	public override void OnConnectedToMaster()
	{
		Debug.Log("���� ������ ������ �����Ͽ����ϴ�.");

		//���� �濡 ���� �õ�
		PhotonNetwork.JoinRandomRoom();
	}

	//���� ������ ���
	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("������ ������ ���� ������ �����Ǿ����ϴ� : " + cause);
	}

	//�� ������ ������ ���
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log("�� ������ �����߽��ϴ�. ���� ���� ����ϴ�.");

		//���ο� ���� �����Ѵ�.
		PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom});
	}

	//�濡 ���������� ������ ���
	public override void OnJoinedRoom()
	{
		Debug.Log("�뿡 �����Ͽ����ϴ�.");
		//�ش� �濡 �÷��̾� ����
		SpawnPlayer();
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient�� ����Ǿ����ϴ�.");

		if (PhotonNetwork.IsMasterClient)
		{
			DummyController.instance.ChangeState();
		}
	}

	//�÷��̾ �����ϴ� �Լ�
	void SpawnPlayer()
	{
		//������ ��ġ�� �÷��̾� ������ ����
		Vector3 spawnPosition = new Vector3(Random.Range(-5.0f, 5f), 0.0f, Random.Range(-3.0f, 3.0f));
		PhotonNetwork.Instantiate("RobotKyle", spawnPosition, Quaternion.identity);
		//������ ī�޶� �ϳ��� �����ؾ� �ϸ�, Audio Listener�� �ϳ� �����ؾ� �ϱ� ������, �⺻ ī�޶� ������ ����
		//�÷��̾� �ϳ� �̻��� ���� ���, �⺻ ī�޶� ��Ȱ��ȭ ��Ų��.
		if (camera != null) camera.SetActive(false);
	}
}
