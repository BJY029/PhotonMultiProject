using UnityEngine;
using UnityEngine.Audio;

//AudioMixer의 Volume 값을 설정하기 위해 사용하는 변수를 enum으로 설정
public enum AudioMixerType
{
	Master, BGM, SFX
}
public class AudioManager : MonoBehaviour
{
	//싱글턴으로 설정
    public static AudioManager instance;
	private void Awake()
	{
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	//오디오 믹서
	[Header("----Mixer----")]
	public AudioMixer audioMixer;

	//BGM과 SFX를 재생할 소스
	[Header("----Sources----")]
	public AudioSource bgmSoruce;
	public AudioSource sfxSource;

	//사용될 각 클립들(아직 오디오 소스를 구하는 중)
	[Header("----BgmClips----")]
	public AudioClip LobbyBgmClip;
	public AudioClip ReadyBgmClip;
	public AudioClip GameBgmClip;
	public AudioClip[] ResultBgmClip;

	
	//특정 오디오 믹서 타입의 볼륨을 정해진 값으로 설정하는 함수
	public void SetAudioVolume(AudioMixerType type, float volume)
	{
		audioMixer.SetFloat(type.ToString(), Mathf.Log10(volume) * 20);
	}

	//특정 오디오 믹서 타입의 volume 값을 가져오는 함수
	public float GetVolume(AudioMixerType mixerType)
	{
		float value;
		if (AudioManager.instance.audioMixer.GetFloat(mixerType.ToString(), out value))
		{
			return Mathf.Pow(10f, value / 20f);
		}
		Debug.LogError("Can't find Volume mixer Type : " + mixerType.ToString());
		return 1f;
	}

	//로비 BGM을 재생하는 함수
	public void PlayLobbyBgm(float Volume = 1.0f)
	{
		bgmSoruce.clip = LobbyBgmClip;
		SetAudioVolume(AudioMixerType.BGM, Volume);
		bgmSoruce.Play();
	}

}
