using Photon.Pun;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
	//Seeker가 춤을 추는 프리팹 오브젝트
    public GameObject[] SeekersDance;
	//Runner가 춤을 추는 프리팹 오브젝트
    public GameObject[] RunnersDance;
	//Seeker가 졌을 때 생성될 프리팹 오브젝트
    public GameObject SeekerLose;
	//Runner가 졌을 때 생성 될 프리팹 오브젝트
    public GameObject RunnerLose;

	//각 승자와 패자 생성 위치
    public Transform WinnerSpawnLoc;
    public Transform LooserSpawnLoc;

	//승자가 누군지 저장
	private string Winner;

	private void Start()
	{
		//승자가 누군지 파악 후
		GetWinner();
		//알맞게 스폰
		SpawnCharacters();
	}

	//RoomProperties를 사용하여 승자를 받아온다.
	void GetWinner()
	{
		//다음과 같이 CustomProperties에서 우승자 정보를 받아와서 출력한다.
		object winner;
		if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Winner", out winner))
		{
			Winner = winner.ToString();
		}
	}

	//각 승자 맞게 캐릭터를 스폰한다.
	void SpawnCharacters()
	{
		int RandIdx = UnityEngine.Random.Range(0, SeekersDance.Length);

		if (Winner == "Runner")
		{
			Instantiate(RunnersDance[RandIdx], WinnerSpawnLoc.position, WinnerSpawnLoc.rotation);
			Instantiate(SeekerLose, LooserSpawnLoc.position, LooserSpawnLoc.rotation);
		}
		else if (Winner == "Seeker")
		{
			Instantiate(SeekersDance[RandIdx], WinnerSpawnLoc.position, WinnerSpawnLoc.rotation);
			Instantiate(RunnerLose, LooserSpawnLoc.position, LooserSpawnLoc.rotation);
		}
		else Debug.LogWarning("Non Winner ERROR..");
	}
}
