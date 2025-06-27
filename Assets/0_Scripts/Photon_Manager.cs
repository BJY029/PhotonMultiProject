using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

	private void Start()
	{

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
}
