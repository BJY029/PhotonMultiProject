using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomListManager : MonoBehaviourPunCallbacks
{
    //방 정보를 담는 prefab이 생성 될 위치
    public Transform content;
    //방 정보를 담는 prefab
    public GameObject roomPrefab;

    //현재 생성된 방들을 저장하고 관리하는 딕셔너리
    private Dictionary<string, GameObject> roomItems = new Dictionary<string, GameObject>();

    //해당 함수는 다음 경우에 호출된다.
        //로비에 들어간 상태(PhotonNetwork.JoinLobby())
        //다른 유저가 방을 만들거나 나갔을 때
        //방에 들어간 유저 수가 변경될 때
        //방에 닫히거나(IsOpen = false) 숨겨질때(IsVisible = false)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //각 서버의 방 정보들을 돌아보며
        foreach(RoomInfo info in roomList)
        {
            //만약 해당 정보가 리스트로부터 제거된 경우
            if(info.RemovedFromList)
            {
                //만약 해당 방 정보가 딕셔너리에 저장되어 있으면
                if(roomItems.ContainsKey(info.Name))
                {
                    //해당 정보를 담고 있는 객체를 파괴하고
                    Destroy(roomItems[info.Name]);
                    //딕셔너리에서도 제거한다.
                    roomItems.Remove(info.Name);
                }
            }
            else//해당 정보가 리스트 안에 있으면
            {
                //만약 이미 딕셔너리 안에 해당 방 정보가 존재하는 경우
                if (roomItems.ContainsKey(info.Name))
                {
                    //해당 방 정보를 담고 있는 객체의 정보를 업데이트한다.
                    roomItems[info.Name].GetComponent<RoomPrefab>().SetUp(info);
                }
                else//새로 생성되어 딕셔너리에 없는 경우
                {
                    //새로운 방 정보를 저장할 prefab을 content 위치에 생성한다.
                    GameObject newItem = Instantiate(roomPrefab, content);
                    //그리고 해당 객체 정보를 초기화한다.
                    newItem.GetComponent<RoomPrefab>().SetUp(info);
                    //딕셔너리에도 해당 방을 삽입한다.
                    roomItems.Add(info.Name, newItem);
                }
            }
        }
    }
}
