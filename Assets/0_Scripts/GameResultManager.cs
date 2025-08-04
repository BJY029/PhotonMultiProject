using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResultManager : MonoBehaviourPun
{
	//싱글턴화
    public static GameResultManager instance;
	private bool isEnding = false;

	private void Awake()
	{
		if (instance == null) instance = this;
	}

	//각 노트북에 들어가는 스크립트들의 리스트
	public List<SwitchLabtop> Labtops = new List<SwitchLabtop>();
	//게임 종료 후 대기 시간
	private float WaitSecond = 6.7f;

	//MasterClient만 실행하는 함수
	//모든 노트북이 꺼졌는지 확인한다.
    [PunRPC]
    void CheckLabtop()
	{
		//MasterClient만 확인 진행
		if (!PhotonNetwork.IsMasterClient) return;
		//해당 리스트의 모든 노트북 플래그를 확인해서
		//하나라도 켜져있으면 아무 처리 진행 안함
		foreach(SwitchLabtop labtop in Labtops)
		{
			if (!labtop.checkFlag) return;
		}
		

		//그리고, EndGame 함수를 모든 클라이언트에게 실행하라고 명령한다.
		photonView.RPC("EndGame", RpcTarget.All, "Runner");
	}

	//게임이 종료되면 실행되는 함수
	[PunRPC]
	void EndGame(string Winner)
	{
		if(isEnding) return;
		isEnding = true;

		//관련 UI를 재생하고
		Game_UIManager.instance.GameOver();
		AudioManager.instance.PlayResultBGM(AudioManager.instance.GetVolume(AudioMixerType.BGM));
		//시간 속도를 0.1배로 변경
		Time.timeScale = 0.1f;

		//MasterClient는 해당 코루틴을 실행시킨다.
		if(PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
		{
			//만약 모든 노트북이 꺼졌으면
			//CustonProperties에 Winner 정보를 저장한다.
			var props = new ExitGames.Client.Photon.Hashtable
			{
				{ "Winner", Winner }
			};

			//로컬 플레이어릐 CustomProperties를 서버에 업데이트 한다.
			PhotonNetwork.CurrentRoom.SetCustomProperties(props);

			StartCoroutine(EndGameC());
		}
	}

	IEnumerator EndGameC()
	{
		//일정 이상 대기 후
		yield return new WaitForSecondsRealtime(WaitSecond);

		//Masterclient는
		if (PhotonNetwork.IsMasterClient)
		{
			//결과 씬으로 이동한다.
			//masterclient가 이동하면 모든 클라이언트가 동기화되어서 함께 씬 이동이 된다.
			PhotonNetwork.LoadLevel("ResultScene");
		}
	}
}
