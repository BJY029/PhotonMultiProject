using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;


public class Result_UIManager : MonoBehaviourPun
{
    //게임 우승자를 표시해주는 UI
    public GameObject WinnerUI;
    //MasterClient에게 표시되는 대기룸으로 돌아가는 버튼
    public GameObject BackToRoomBtn;
    //일반 Client에게 표시되는 안내 텍스트
    public GameObject WaitingMasterClientText;

    
	private void Start()
	{
        //마우스 커서 락을 해제시키고
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

        //시간 속도 또한 정상화
        Time.timeScale = 1.0f;

        //우승자를 확인하는 함수 실행
		GetWinner();

        //만약 MaterClient라면
        if (PhotonNetwork.IsMasterClient)
        {
            //관련 버튼을 활성화 하고
            BackToRoomBtn.SetActive(true);
            //해당 버튼에 대기 룸으로 돌아갈 때 해야 하는 처리들을 연결시켜준다.
            BackToRoomBtn.GetComponent<Button>().onClick.AddListener(BackToRoom);
            WaitingMasterClientText.SetActive(false);
        }
        else //master client가 아니면 간단한 안내 텍스트를 활성화시킨다.
        {
			BackToRoomBtn.SetActive(false);
			WaitingMasterClientText.SetActive(true);
		}
	}

    //우승자를 표시하는 함수
	void GetWinner()
    {
        //다음과 같이 CustomProperties에서 우승자 정보를 받아와서 출력한다.
        object winner;
        if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Winner", out winner))
        {
            Text winnerText = WinnerUI.GetComponentInChildren<Text>();
            winnerText.text = "Winner is " + winner.ToString();
        }
    }

    //대기룸으로 돌아가기 위해 실행되는 함수
    void BackToRoom()
    {
        //대기 룸에 저장된 각 플레이어 정보들을 초기화시킨다.(모든 클라이언트에게 실행되어야 함)
        photonView.RPC(nameof(initPlayerProperties), RpcTarget.All);
        //그리고 대기 룸 자체에 저장된 정보를 초기화 시킨다.
        InitCustomProperties();

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
			PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.DestroyAll();
		}
        //그리고 대기 룸으로 이동한다.
        PhotonNetwork.LoadLevel("RoomScene");
    }

    //각 플레이어 정보를 초기화 하는 함수
    [PunRPC]
    void initPlayerProperties()
    {
        //역할 정보와 준비 정보를 null로 초기화 시키고 적용한다.
		var playerProps = new ExitGames.Client.Photon.Hashtable
		{
			{"Role", null},
			{"IsReady", false },
			{"IsMaster", false }
		};
		PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
	}

    //방 정보를 초기화 하는 함수
    void InitCustomProperties()
    {
        //우승자 정보를 초기화 하고 적용한다.
        var resetProps = new ExitGames.Client.Photon.Hashtable
        {
            {"Winner", null },
            {"StartTime", null }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(resetProps);
	}
}
