using UnityEngine;
using Photon.Pun;

public class RunnerManager : MonoBehaviourPun
{
	//싱글턴
	public static RunnerManager instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//해당 Runner의 체력
	[SerializeField]
	private float HeartsMaxValue = 100;
	private float CurrentHeart;

	//Runner의 체력 슬라이더 값 초기화
	private void Start()
	{
		Game_UIManager.instance.Hearts.maxValue = HeartsMaxValue;
		Game_UIManager.instance.Hearts.value = HeartsMaxValue;

		CurrentHeart = HeartsMaxValue;
	}

	//RPC 함수
	//Seeker에게 총을 맞으면 호출된다.
	[PunRPC]
	public void GetDamagedBySeeker(float value)
	{
		//전달받은 값에 맞게 체력 감소 시킨다.
		CurrentHeart -= value;
		//만약 체력이 음수가 되면
		if(CurrentHeart < 0)
		{
			//체력 값 초기화 후
			CurrentHeart = 0;
			Debug.Log("Runner가 Seeker에게 잡혔습니다.");
			//해당 Runner를 파괴시킨다(이는 변경할 예정)
			if(photonView.IsMine)
			{
				Game_UIManager.instance.Hearts.value = CurrentHeart;
				PhotonNetwork.Destroy(gameObject);
			}
		}
		//체력바 또한 갱신한다.
		Game_UIManager.instance.Hearts.value = CurrentHeart;
	}

	[PunRPC]
	public void HealHearts(float value)
	{
		CurrentHeart += value;
        if (CurrentHeart > HeartsMaxValue)
        {
			CurrentHeart = HeartsMaxValue;
        }
		Game_UIManager.instance.Hearts.value = CurrentHeart;
	}

}
