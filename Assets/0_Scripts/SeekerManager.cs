using Photon.Pun;
using UnityEngine;

public class SeekerManager : MonoBehaviour
{
	//싱글턴
	public static SeekerManager Instance;

	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	//Seeker 체력
	[SerializeField]
	private float HeartsMaxValue = 100;
	private float CurrentHeart;

	private void Start()
	{
		//체력 갱신
		Game_UIManager.instance.Hearts.maxValue = HeartsMaxValue;
		Game_UIManager.instance.Hearts.value = HeartsMaxValue;

		CurrentHeart = HeartsMaxValue;
	}

	//Dummy를 쏠 경우 호출되는 함수
	public void GetDamagedOnDummy(float value)
	{
		//전달 받은 값에 의해서 체력이 감소된다.
		CurrentHeart -= value;
		if (CurrentHeart < 0)
		{
			CurrentHeart = 0;
			Debug.Log("Seeker가 자멸했습니다.");
			Game_UIManager.instance.Hearts.value = CurrentHeart;
			PhotonNetwork.Destroy(gameObject);
		}
		Game_UIManager.instance.Hearts.value = CurrentHeart;
	}
}
