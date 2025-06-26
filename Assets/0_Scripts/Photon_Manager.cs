using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

	public GameObject camera;

	private void Start()
	{
		SpawnPlayer();
	}

	//연결 끊어진 경우
	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("다음의 이유로 서버 연결이 해제되었습니다 : " + cause);
	}


	//MasterClient가 변경되면 호출되는 함수
	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient가 변경되었습니다.");

		//MasterClient가 나라면
		if (PhotonNetwork.IsMasterClient)
		{
			//내 앱을 끈다.
			Application.Quit();
		}
	}

	//플레이어를 생성하는 함수
	void SpawnPlayer()
	{
		//랜덤한 위치에 플레이어 프리팹 생성
		Vector3 spawnPosition = new Vector3(Random.Range(-85.0f, -90f), 2.0f, Random.Range(10.0f, 30.0f));
		PhotonNetwork.Instantiate("RobotKyle", spawnPosition, Quaternion.identity);
		//씬에는 카메라가 하나는 존재해야 하며, Audio Listener가 하나 존재해야 하기 때문에, 기본 카메라를 설정해 놓고
		//플레이어 하나 이상이 들어온 경우, 기본 카메라를 비활성화 시킨다.
		if (camera != null) camera.SetActive(false);
	}
}
