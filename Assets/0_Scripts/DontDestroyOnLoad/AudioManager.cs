using System;
using System.Collections;
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
	public AudioClip GameChaseBgmClip;
	public AudioClip ResultBgmClip;


	[Header("----SFXClips----")]
	public AudioClip StartGameClip;
	public AudioClip[] ClockTickingClips;

	public bool IsMute = false;
	private float fadeinDuration = 3f;


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

	public void InitBGMSoruce()
	{
		bgmSoruce.Stop();
		bgmSoruce.clip = null;
	}

	//로비 BGM을 재생하는 함수
	public void PlayLobbyBgm(float Volume = 1.0f)
	{
		bgmSoruce.clip = LobbyBgmClip;
		SetAudioVolume(AudioMixerType.BGM, Volume);
		bgmSoruce.Play();
	}

	//룸 BGM을 재생하는 함수
	public void PlayReadyBgm(float Volume = 1.0f)
	{
		bgmSoruce.clip = ReadyBgmClip;
		SetAudioVolume(AudioMixerType.BGM, Volume);
		bgmSoruce.Play();
	}


	public void PlayGameMainBGM(float Volume = 1.0f)
	{
		bgmSoruce.clip = GameBgmClip;
		SetAudioVolume(AudioMixerType.BGM, Volume);
		bgmSoruce.Play();
	}

	public void PlayChaseBGMWithFade(float Volume = 1.0f)
	{
		bgmSoruce.clip = GameChaseBgmClip;
		StartCoroutine(FadeIn(bgmSoruce, fadeinDuration));
	}

	//페이드인 효과를 주는 코루틴
	//코루틴 효과가 AudioMiexer에 영향을 주면 안되기 때문에,
	//AudioSource 자제의 소리를 조정하여 페이드인 효과를 준다.
	IEnumerator FadeIn(AudioSource audioSource, float duration)
	{
		float currentTime = 0f;
		float startVolume = 0f;

		audioSource.volume = 0f;
		audioSource.Play();

		while(currentTime < duration)
		{
			currentTime += Time.unscaledDeltaTime;
			audioSource.volume = Mathf.Lerp(startVolume, 1f, currentTime/duration);
			yield return null;
		}

		audioSource.volume = 1f;
	}

	public void PlayResultBGM(float Volume = 1.0f)
	{
		bgmSoruce.clip = ResultBgmClip;
		SetAudioVolume(AudioMixerType.BGM, Volume);
		bgmSoruce.Play();
	}

	public void PlayStartGameIntro()
	{
		sfxSource.PlayOneShot(StartGameClip);
	}

	public void PlayRandomTickSound()
	{
		int idx = UnityEngine.Random.Range(0, ClockTickingClips.Length);
		sfxSource.PlayOneShot(ClockTickingClips[idx]);
	}

}
