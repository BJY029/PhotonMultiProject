using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

using WebSocketSharp;

public class LobbyUIManager : MonoBehaviour
{
	//싱글턴화
	public static LobbyUIManager Instance;
	//LobbyManager는 싱글턴으로 만들지 않는다.
	public LobbyManager LobbyManager;
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	public InputField NicknameField;
	public Button SettingsBtn;
	public Button JoinRoomBtn;
	public Button CreateRoomBtn;
	public Button ExitBtn;

	public GameObject CreateRoomFrame;
	public GameObject JoinRoomFrame;
	public GameObject PassWordFrame;
	public GameObject SettingsFrame;

	public Slider BGMSlider;
	public Slider SFXSlider;
	public Toggle MUTE;

	public GameObject Warning_Create;
	public GameObject Warning_MaxPlayer;
	public GameObject Warning_NullPass;
	public GameObject Warning_MasterClientLeave;

	public InputField RoomName;
	public InputField Password;

	private RoomInfo CurrentRoomInfo;

	private void Start()
	{
		//싱글턴이 정상적으로 적용되기 위해서 active 한 상태로 게임이 시작되므로
		//방해되지 않게 현재 localScale을 0으로 설정한 상태
		//따라서 localScale을 다시 1로 설정해준다.
		CreateRoomFrame.transform.localScale = Vector3.one;
		JoinRoomFrame.transform.localScale = Vector3.one;
		PassWordFrame.transform.localScale= Vector3.one;
		SettingsFrame.transform.localScale = Vector3.zero;
		//그리고 비활성화한다.
		CreateRoomFrame.gameObject.SetActive(false);
		JoinRoomFrame.gameObject.SetActive(false);
		PassWordFrame.gameObject.SetActive(false);

		//강제 추방된 경우
		if (SceneStateManager.instance.ForcedToLeaveRoom)
		{
			Warning_MasterClientLeave.SetActive(true);
			SceneStateManager.instance.ForcedToLeaveRoom = false;
		}
		else
		{
			Warning_MasterClientLeave.SetActive(false);
		}
	}

	//Create 버튼이 눌린경우, 다음 함수가 실행된다.
	public void OnClickedCreateRoom()
	{
		if (!CreateRoomFrame.activeSelf)
		{
			CreateRoomFrame.SetActive(true);
		}
	}

	//x 버튼이 눌린 경우 다음 함수가 실행된다.
	public void OnExitCreateRoom()
	{
		if (CreateRoomFrame.activeSelf)
		{
			CreateRoomFrame.SetActive(false);
		}
	}

	public void OnClickedJoinRoom()
	{
		if (!JoinRoomFrame.activeSelf)
		{
			JoinRoomFrame.SetActive(true);
		}
	}

	public void OnExitJoinRoom()
	{
        if (JoinRoomFrame.activeSelf)
        {
			JoinRoomFrame.SetActive(false);
        }
		if (PassWordFrame.activeSelf)
		{
			PassWordFrame.SetActive(false);
		}
    }

	//설정창 열기 버튼이 눌리면 실행될 함수
	public void OnClickedSettingsBtn()
	{
		//각 볼륨 크기를 받아와서 슬라이더 value에 적용시켜준다.
		BGMSlider.value = GetVolume(AudioMixerType.BGM);
		SFXSlider.value = GetVolume(AudioMixerType.SFX);

		if (AudioManager.instance.IsMute) MUTE.isOn = true;
		else MUTE.isOn = false;

		NicknameField.interactable = false;
		SettingsBtn.interactable = false;
		JoinRoomBtn.interactable = false;
		CreateRoomBtn.interactable = false;
		ExitBtn.interactable = false;

		//설정창을 띄운다.(크기를 1로 설정한다.)
		SettingsFrame.transform.localScale = Vector3.one;
	}

	//설정창 나가기 버튼이 눌리면 실행될 함수
	public void OnExitSettings()
	{
		NicknameField.interactable = true;
		SettingsBtn.interactable = true;
		JoinRoomBtn.interactable = true;
		CreateRoomBtn.interactable = true;
		ExitBtn.interactable = true;

		SettingsFrame.transform.localScale = Vector3.zero;
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

	//특정 오디오 타입의 볼륨 값을 가져오는 함수(이는 AudioManager의 함수를 그대로 가져온 것)
	//즉 딱히 필요없는 함수
	public float GetVolume(AudioMixerType mixerType)
	{
		float value;
		if(AudioManager.instance.audioMixer.GetFloat(mixerType.ToString(), out value))
		{
			return Mathf.Pow(10f, value / 20f);
		}
		Debug.LogError("Can't find Volume mixer Type : " + mixerType.ToString());
		return 1f;
	}

	public void OnRefreshRooms()
	{
		PhotonNetwork.JoinLobby();
	}

	//방 참가 버튼이 눌리면 호출되는 함수
	public void	OnPassWord(RoomInfo roomInfo)
	{
		//참가하고자 하는 방 정보를 저장해 놓는다.
		CurrentRoomInfo = roomInfo;
		//패스워들 입력하는 UI를 켠 후
		if (!PassWordFrame.activeSelf)
		{
			PassWordFrame.SetActive(true);
			//해당 inputfield의 값을 초기화시켜준다.
			InputField field = PassWordFrame.GetComponentInChildren<InputField>();
			field.text = "";
			field.ActivateInputField();  // 커서 자동 위치
		}
	}

	//password가 입력 된 후, enter key 혹은 커서가 없어지면 호출된다.
	//Input Field의 On End Edit 에서 호출된다.
	public void EnterPassWord()
	{
		//우선 PasswordFrame에서 InputField를 찾는다.
		InputField field = PassWordFrame.GetComponentInChildren<InputField>();
		//만약 해당 InputField에 적힌 값이 없는 경우
		if(field.text.IsNullOrEmpty())
		{
			//경고문을 띄운다.
			PopUpAnimController.Instance.PopUpWarning(Warning_NullPass, "Enter password!");
			return;
		}

		//패스워드를 정리하고
		string password = field.text.Trim();
		//방 참가를 위해 인자로 넘겨준다.
		LobbyManager.instance.TryJoinRoom(CurrentRoomInfo, password);
	}

	//패스워드 입력 창 닫기를 누른 경우 호출되는 함수
	public void OnExitPassword()
	{
		if (PassWordFrame.activeSelf)
		{
			PassWordFrame.SetActive(false);
		}
	}

	//방 생성 창에서 정보를 입력하고, Create 버튼을 누르면 호출되는 함수
	public void OnClickedCreateBtn()
	{
		//모든 요소가 입력되지 않은 상태라면, Warning 메시지를 생성한다.
		if (RoomName.text.IsNullOrEmpty() || Password.text.IsNullOrEmpty())
		{
			PopUpAnimController.Instance.PopUpWarning(Warning_Create, "Enter all required fields.");
			return;
		}

		LobbyManager.CreateRoomWithPassword(RoomName.text, Password.text.Trim());
	}

	public void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 종료
#else
        Application.Quit();  // 빌드된 게임에서 종료
#endif
	}
}
