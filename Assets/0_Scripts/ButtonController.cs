using Photon.Pun;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
	//Menu 창
	[SerializeField] private GameObject Menu;
	//각종 UI 요소들
	public Slider BGMSlider;
	public Slider SFXSlider;
	public Slider SensitivitySlider;
	public Toggle MUTE;

	//감도 조절을 위해 저장하는 플레이어 조정 스크립트
	private ThirdPersonController TPC = null;

	private void Start()
	{
		//Menu 창이 처음에 크기가 0으로 초기화되어 있기 때문에, 크기를 1로 만든다.
		Menu.transform.localScale = Vector3.one;
		//그리고 Menu를 비활성화 한다.
		Menu.SetActive(false);
	}

	private void Update()
	{
		//ESC키가 눌리면
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//Menu 창을 토글한다.
			ToggleMenu();
		}
	}

	//Menu 창을 토글하는 함수
	public void ToggleMenu()
    {
		//Menu 창이 켜져있으면
		if (Menu.activeSelf)
		{
			//Menu를 비활성화 하고
			Menu.SetActive(false);
			//마우스 잠금 및 커서를 안보이게 처리한다.
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			TPC.enabled = true;
		}
		else//Menu 창이 꺼져있으면
		{
			InitValues();
			//Menu를 활성화하고
			Menu.SetActive(true);
			//마우스 잠금 해제 및 커서를 보이게 처리한다.
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			TPC.enabled = false;
		}
    }

	//메뉴창이 켜질 때 호출되는 함수
	public void InitValues()
	{
		//각 볼륨 크기를 받아와서 슬라이더 value에 적용시켜준다.
		BGMSlider.value = AudioManager.instance.GetVolume(AudioMixerType.BGM);
		SFXSlider.value = AudioManager.instance.GetVolume(AudioMixerType.SFX);

		if (AudioManager.instance.IsMute) MUTE.isOn = true;
		else MUTE.isOn = false;

		//감도 설정을 하기 위해, 플레이어 움직임 조절 스크립트를 받아온다.
		//null인 경우에만 찾아서 가져온다.
		if(TPC == null)
		{
			TPC = RoleManager.instance.getPlayerObj().GetComponent<ThirdPersonController>();
		}
		//감도 값을 실제 감도 값으로 초기화한다.
		SensitivitySlider.value = TPC.getSensitivity();
	}

	//EXIT 버튼의 OnButtonClicke에 적용될 이벤트
    public void ExitButtonClicked()
    {
		//앱을 나간다.
		//추후에는 로비로 나가기로 변경한다.
		Application.Quit();
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

	//감도 슬라이더에 연결될 함수
	public void OnSensitivitySliderChanged()
	{
		TPC.setSensitivity(SensitivitySlider.value);
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
}
