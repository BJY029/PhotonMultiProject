using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Result_UIManager : MonoBehaviourPunCallbacks
{
    //게임 우승자를 표시해주는 UI
    public GameObject WinnerUI;
    //MasterClient에게 표시되는 대기룸으로 돌아가는 버튼
    public GameObject BackToRoomBtn;
    //일반 Client에게 표시되는 안내 텍스트
    public GameObject WaitingMasterClientText;


	//Setting에 사용될 UI
	public Button GearBtn;
	public GameObject SettingsFrame;
	public Slider BGMSlider;
	public Slider SFXSlider;
	public Toggle MUTE;
	public Button BackReadyRoom;

	private void Start()
	{
		SettingsFrame.transform.localScale = Vector3.zero;
		//마우스 커서 락을 해제시키고
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

        //시간 속도 또한 정상화
        Time.timeScale = 1.0f;

        //우승자를 확인하는 함수 실행
		GetWinner();

        //만약 MaterClient라면
        if (PhotonNetwork.IsMasterClient)
        {
            //관련 버튼을 활성화 하고
            BackToRoomBtn.SetActive(true);
            //해당 버튼에 대기 룸으로 돌아갈 때 해야 하는 처리들을 연결시켜준다.
            BackToRoomBtn.GetComponent<Button>().onClick.AddListener(BackToRoom);
            WaitingMasterClientText.SetActive(false);
        }
        else //master client가 아니면 간단한 안내 텍스트를 활성화시킨다.
        {
			BackToRoomBtn.SetActive(false);
			WaitingMasterClientText.SetActive(true);
		}
	}

	//만약 master client가 게임을 나가거나 특정 이유로 변경된 경우
	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient가 변경되었습니다.");

		//새로운 master client 에게 게임 시작 권한을 부여 후
		if (PhotonNetwork.IsMasterClient)
		{
			//관련 버튼을 활성화 하고
			BackToRoomBtn.SetActive(true);
			//해당 버튼에 대기 룸으로 돌아갈 때 해야 하는 처리들을 연결시켜준다.
			BackToRoomBtn.GetComponent<Button>().onClick.AddListener(BackToRoom);
			WaitingMasterClientText.SetActive(false);
		}
	}

	//우승자를 표시하는 함수
	void GetWinner()
    {
        //다음과 같이 CustomProperties에서 우승자 정보를 받아와서 출력한다.
        object winner;
        if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Winner", out winner))
        {
            Text winnerText = WinnerUI.GetComponentInChildren<Text>();
            winnerText.text = "Winner is " + winner.ToString();
        }
    }

    //대기룸으로 돌아가기 위해 실행되는 함수
    void BackToRoom()
    {
        //대기 룸에 저장된 각 플레이어 정보들을 초기화시킨다.(모든 클라이언트에게 실행되어야 함)
        photonView.RPC(nameof(initPlayerProperties), RpcTarget.All);
        //그리고 대기 룸 자체에 저장된 정보를 초기화 시킨다.
        InitCustomProperties();

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
			PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.DestroyAll();
		}
        //그리고 대기 룸으로 이동한다.
        PhotonNetwork.LoadLevel("RoomScene");
    }

    //각 플레이어 정보를 초기화 하는 함수
    [PunRPC]
    void initPlayerProperties()
    {
        //역할 정보와 준비 정보를 null로 초기화 시키고 적용한다.
		var playerProps = new ExitGames.Client.Photon.Hashtable
		{
			{"Role", null},
			{"IsReady", false },
			{"IsMaster", false }
		};
		PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
	}

    //방 정보를 초기화 하는 함수
    void InitCustomProperties()
    {
        //우승자 정보를 초기화 하고 적용한다.
        var resetProps = new ExitGames.Client.Photon.Hashtable
        {
            {"Winner", null },
            {"StartTime", null }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(resetProps);
	}


	//설정창 열기 버튼이 눌리면 실행될 함수
	public void OnClickedSettingsBtn()
	{
		//각 볼륨 크기를 받아와서 슬라이더 value에 적용시켜준다.
		BGMSlider.value = AudioManager.instance.GetVolume(AudioMixerType.BGM);
		SFXSlider.value = AudioManager.instance.GetVolume(AudioMixerType.SFX);
		//현재 Mute 여부에 따라서 Trigger 여부를 설정한다.
		if (AudioManager.instance.IsMute) MUTE.isOn = true;
		else MUTE.isOn = false;

		//설정창을 띄운다.(크기를 1로 설정한다.)
		SettingsFrame.transform.localScale = Vector3.one;
		//설정차이 띄워지면 뒷 배경의 버튼 클릭을 막는다.
		GearBtn.interactable = false;
		//StartBtn이 활성화된 경우에만 비활성화시킨다.
		if (BackReadyRoom.gameObject.activeSelf)
			BackReadyRoom.interactable = false;
	}

	//설정창 나가기 버튼이 눌리면 실행될 함수
	public void OnExitSettings()
	{
		//다시 설정창을 없애고
		SettingsFrame.transform.localScale = Vector3.zero;
		//버튼들을 활성화시킨다.
		GearBtn.interactable = true;
		if (BackReadyRoom.gameObject.activeSelf)
			BackReadyRoom.interactable = true;
	}

	//BGM 슬라이더에 연결될 함수
	public void OnBGMSliderChanged()
	{
		AudioManager.instance.SetAudioVolume(AudioMixerType.BGM, BGMSlider.value);
	}

	//SFX 슬라이더에 연결될 함수
	public void OnSFXSliderChanged()
	{
		AudioManager.instance.SetAudioVolume(AudioMixerType.SFX, SFXSlider.value);
	}

	//Mute Toggle에 연결될 함수
	public void MuteToggle()
	{
		if (MUTE.isOn)
		{
			AudioManager.instance.SetAudioVolume(AudioMixerType.Master, -80f);
			AudioManager.instance.IsMute = true;
		}
		else
		{
			AudioManager.instance.SetAudioVolume(AudioMixerType.Master, 0f);
			AudioManager.instance.IsMute = false;
		}
	}

	//로비로 돌아가기 버튼에 적용될 함수
	public void BackToLobby()
	{
		initPlayerProperties();
		PhotonNetwork.LeaveRoom();
	}

	//플레이어가 방을 떠날 때 실행될 함수
	public override void OnLeftRoom()
	{
		//로비 씬으로 가도록 설정
		SceneManager.LoadScene("LobbyScene");
	}
}
