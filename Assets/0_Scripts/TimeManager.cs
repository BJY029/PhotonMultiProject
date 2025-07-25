using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Collections;
using System;
using UnityEngine.UI;

public class TimeManager : MonoBehaviourPun
{
	//싱글턴
	public static TimeManager instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//제한 시간 10분
	private float timeLimit = 600f;
	//게임 시작 시간
    private double startTime = 0f;
	//타이머 재생 플래그
	private bool startTimerFlag = false;

	//관련 UI
	public GameObject Timer;
	public Text time;

	//게임을 시작하면 타이머를 제한 시간에 맞게 초기화
	private void Start()
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(timeLimit);
		time.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

		//플래그를 false로 해서 타이머 재생되지 않도록 함
		startTimerFlag = false;
	}

	//타이머 재생 전 설정 진행
	[PunRPC]
	void SetTimer()
	{
		//MasterClient만 진행
		if (PhotonNetwork.IsMasterClient)
		{
			//PhotonNetwork상의 시간을 기준으로 시작 시간 설정
			startTime = PhotonNetwork.Time;
			//해당 방의 커스텀 프로퍼티에 시작 시간 저장
			var props = new ExitGames.Client.Photon.Hashtable { { "StartTime", startTime } };
			PhotonNetwork.CurrentRoom.SetCustomProperties(props);

			//모든 클라이언트가 타이머를 재생시키기 위해 RPC 호출
			photonView.RPC(nameof(StartAllTimer), RpcTarget.All);
		}
	}

	//모든 클라이언트의 타이머를 설정하고 재생시키는 RPC 함수
	[PunRPC]
	void StartAllTimer()
	{
		//시작 시간을 커스텀 프로퍼티로부터 받아온다.
		if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("StartTime"))
		{
			startTime = (double)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"];
		}
		//타이머 재생이 가능하도록 설정
		startTimerFlag = true;
	}

	private void Update()
	{
		//타이머 재생 플래그
		if (!startTimerFlag) return;

		if(startTime > 0)
		{
			//현재 시간 기준 시작 시간으로부터 흐른 시간 계산
			double elapsedTime = PhotonNetwork.Time - startTime;
			//해당 시간을 제한시간에서 빼서 현재 남은 시간 계산
			double remainingTime = timeLimit - elapsedTime;

			//시간 단위로 변환 및 출력
			TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
			time.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

			// 60초 이하일 때 경고 색상 변경
			if (remainingTime <= 60 && time.color != Color.red)
			{
				time.color = Color.red;
			}

			//제한 시간 초과시, Seeker를 승리로 판단
			if (remainingTime <= 0)
			{
				GameResultManager.instance.photonView.RPC("EndGame", RpcTarget.All, "Seeker");
				startTimerFlag = false;
			}
		}
	}

}
