using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
	public static LobbyManager instance;

	private string gameVersion = "1";
	public Text statusText;
	public Text nicknameText;
	public InputField nicknameInput;

	public GameObject Warning_Password;

	//master client가 씬을 옮기면, 해당되는 다른 유저들도 옮기도록 하기 위한 설정
	private void Awake()
	{
		if (instance == null) instance = this;
		PhotonNetwork.AutomaticallySyncScene = true;
	}


	private void Start()
	{
		//서버 접속 시도
		Connect();
		statusText.text = "Connection to server...";
	}

	public void Connect()
	{
		//버전을 설정하고
		PhotonNetwork.GameVersion = gameVersion;
		//서버 접속을 시도한다.
		PhotonNetwork.ConnectUsingSettings();
	}

	//Master 서버에 연결된 경우
	public override void OnConnectedToMaster()
	{
		//Player의 닉네임을 현재 서버에 들어와있는 플레이어 수의 번호를 붙여서 임시로 부여한다.
		PhotonNetwork.NickName = "Player " + PhotonNetwork.PlayerList.Length;
		nicknameText.text = PhotonNetwork.NickName;

		//Master 서버에 연결 된 후, 바로 로비에 연결시도한다.
		statusText.text = "Connected. Joining lobby...";
		PhotonNetwork.JoinLobby();
	}

	//로비에 연결되면 호출되는 함수
	public override void OnJoinedLobby()
	{
		statusText.text = "Lobby joined.";
	}

	//방 이름과 비밀번호를 전달 받으면, 해당 정보들로 방을 생성하는 함수
	public void CreateRoomWithPassword(string RoomName, string password)
	{
		//방에 설정할 옵션을 정의하는 RoomOption 객체 생성
		RoomOptions options = new RoomOptions();
		//최대 인원 수를 설정(4명으로 설정)
		options.MaxPlayers = 6;
		//CustomerRoomProperties는 Room에 연결된 key-value 데이터를 저장할 수 있는 해시테이블이다.
		options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
		{
			{"Password", password}//키 "Password"에 해당 비번을 저장한다.
		};

		//로비에서는 CustomerRoomPropertiesForLobby에 포함된 키만 다른 유저의 RoomInfo에 저장된다.
		//따라서 다음과 같은 설정을 해주지 않으면, 다른 user가 해당 방의 정보를 이용할 때 해당 정보를 
		//이용할 수 없다.
		options.CustomRoomPropertiesForLobby = new string[] { "Password" };

		//Photon 서버에 방 생성 요청을 보낸다.
			//RoomName : 유저가 입력한 방 이름
			//options : 위에서 정의한 Room 옵션
			//TypedLobby.Default : 기본 로비에 방을 등록(해당 설정이 없으면 로비에서 안 보일 수 있다.)
		PhotonNetwork.CreateRoom(RoomName, options, TypedLobby.Default);
	}

	//방 참가를 시도하는 함수
	public void TryJoinRoom(RoomInfo roomInfo, string InputPassWord)
	{
		//우선 참가하고자 하는 방 정보에 저장되어 있는 패스워드 값을 가져온다.
		string correctPassword = roomInfo.CustomProperties["Password"]?.ToString();
		//Debug.Log("room password : " + correctPassword);

		//만약 입력된 패스워드와 실제 패스워드가 같은 경우
		if(InputPassWord == correctPassword)
		{
			//해당 방에 참가한다.
			PhotonNetwork.JoinRoom(roomInfo.Name);
		}
		else //일치 하지 않는 경우
		{
			//경고문을 띄워준다.
			PopUpAnimController.Instance.PopUpWarning(Warning_Password, "비밀번호 불일치!");
		}
	}

	//방에 성공적으로 참여하면 호출되는 함수
	public override void OnJoinedRoom()
	{
		Debug.Log("방 생성 및 참가 성공!, RoomScene으로 이동합니다.");
		//Room 씬으로 이동한다.
		PhotonNetwork.LoadLevel("RoomScene");
	}

	//로비에서 닉네임 설정을 하면 반영해주는 함수
	public void OnInputEnd()
	{
		string name = nicknameInput.text;
		PhotonNetwork.NickName = name;
		nicknameText.text = PhotonNetwork.NickName;
	}
}
