using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class RoomPrefab : MonoBehaviour
{
    //현재 해당 객체가 표시하고 있는 룸 정보를 저장한다.
    RoomInfo myRoomInfo;

    public Text roomNameText;
    public Text playerCounterText;
    private string roomName;

    //룸 정보를 받아서 초기화 하는 함수
    public void SetUp(RoomInfo info)
    {
        //룸 정보 초기화
        myRoomInfo = info;
        //룸 이름 초기화
        roomName = info.Name;
        //참가 인원수 초기화
        roomNameText.text = roomName;
        playerCounterText.text = $"참가 인원 : {info.PlayerCount} / {info.MaxPlayers}";
    }

    //방 참가 버튼을 누르면 호출될 함수
    public void OnClickJoin()
    {
        //패스워드를 입력하는 UI창을 띄운다.
        LobbyUIManager.Instance.OnPassWord(myRoomInfo);
    }
}
