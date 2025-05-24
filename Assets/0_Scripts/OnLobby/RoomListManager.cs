using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomListManager : MonoBehaviourPunCallbacks
{
    //�� ������ ��� prefab�� ���� �� ��ġ
    public Transform content;
    //�� ������ ��� prefab
    public GameObject roomPrefab;

    //���� ������ ����� �����ϰ� �����ϴ� ��ųʸ�
    private Dictionary<string, GameObject> roomItems = new Dictionary<string, GameObject>();

    //�ش� �Լ��� ���� ��쿡 ȣ��ȴ�.
        //�κ� �� ����(PhotonNetwork.JoinLobby())
        //�ٸ� ������ ���� ����ų� ������ ��
        //�濡 �� ���� ���� ����� ��
        //�濡 �����ų�(IsOpen = false) ��������(IsVisible = false)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //�� ������ �� �������� ���ƺ���
        foreach(RoomInfo info in roomList)
        {
            //���� �ش� ������ ����Ʈ�κ��� ���ŵ� ���
            if(info.RemovedFromList)
            {
                //���� �ش� �� ������ ��ųʸ��� ����Ǿ� ������
                if(roomItems.ContainsKey(info.Name))
                {
                    //�ش� ������ ��� �ִ� ��ü�� �ı��ϰ�
                    Destroy(roomItems[info.Name]);
                    //��ųʸ������� �����Ѵ�.
                    roomItems.Remove(info.Name);
                }
            }
            else//�ش� ������ ����Ʈ �ȿ� ������
            {
                //���� �̹� ��ųʸ� �ȿ� �ش� �� ������ �����ϴ� ���
                if (roomItems.ContainsKey(info.Name))
                {
                    //�ش� �� ������ ��� �ִ� ��ü�� ������ ������Ʈ�Ѵ�.
                    roomItems[info.Name].GetComponent<RoomPrefab>().SetUp(info);
                }
                else//���� �����Ǿ� ��ųʸ��� ���� ���
                {
                    //���ο� �� ������ ������ prefab�� content ��ġ�� �����Ѵ�.
                    GameObject newItem = Instantiate(roomPrefab, content);
                    //�׸��� �ش� ��ü ������ �ʱ�ȭ�Ѵ�.
                    newItem.GetComponent<RoomPrefab>().SetUp(info);
                    //��ųʸ����� �ش� ���� �����Ѵ�.
                    roomItems.Add(info.Name, newItem);
                }
            }
        }
    }
}
