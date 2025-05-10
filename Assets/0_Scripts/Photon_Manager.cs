using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
	[Tooltip("방의 최대 플레이어 참가 수.")]
	[SerializeField]
	private byte maxPlayersPerRoom = 4;

    private string gameVersion = "1";

	public GameObject camera;

	//해당 설정을 통해 PhotonNetwork.LoadLevel() 호출 시,
	//마스터 클라이언트가 씬을 로드하면, 해당 방에 있는 다른 클라이언트들도 같은 씬을 로드하게 만든다.
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
		//이미 Photon 서버에 연결되어 있는 경우
		if (PhotonNetwork.IsConnected)
		{
			//랜던 방 입장을 시도한다.
			PhotonNetwork.JoinRandomRoom();
		}
		else //서버에 아직 연결되지 않은 경우
		{
			//버전을 설정하고
			PhotonNetwork.GameVersion = gameVersion;
			//서버 접속을 시도한다.
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	//Master 서버에 연결된 경우
	public override void OnConnectedToMaster()
	{
		Debug.Log("포톤 마스터 서버에 연결하였습니다.");

		//랜덤 방에 참여 시도
		PhotonNetwork.JoinRandomRoom();
	}

	//연결 끊어진 경우
	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("다음의 이유로 서버 연결이 해제되었습니다 : " + cause);
	}

	//방 참가에 실패한 경우
	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log("방 참가에 실패했습니다. 방을 새로 만듭니다.");

		//새로운 방을 생성한다.
		PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom});
	}

	//방에 정상적으로 접속한 경우
	public override void OnJoinedRoom()
	{
		Debug.Log("룸에 접속하였습니다.");
		//해당 방에 플레이어 스폰
		SpawnPlayer();
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient가 변경되었습니다.");

		if (PhotonNetwork.IsMasterClient)
		{
			DummyController.instance.ChangeState();
		}
	}

	//플레이어를 생성하는 함수
	void SpawnPlayer()
	{
		//랜덤한 위치에 플레이어 프리팹 생성
		Vector3 spawnPosition = new Vector3(Random.Range(-5.0f, 5f), 0.0f, Random.Range(-3.0f, 3.0f));
		PhotonNetwork.Instantiate("RobotKyle", spawnPosition, Quaternion.identity);
		//씬에는 카메라가 하나는 존재해야 하며, Audio Listener가 하나 존재해야 하기 때문에, 기본 카메라를 설정해 놓고
		//플레이어 하나 이상이 들어온 경우, 기본 카메라를 비활성화 시킨다.
		if (camera != null) camera.SetActive(false);
	}
}
