using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class RoomPrefab : MonoBehaviour
{
    //���� �ش� ��ü�� ǥ���ϰ� �ִ� �� ������ �����Ѵ�.
    RoomInfo myRoomInfo;

    public Text roomNameText;
    public Text playerCounterText;
    private string roomName;

    //�� ������ �޾Ƽ� �ʱ�ȭ �ϴ� �Լ�
    public void SetUp(RoomInfo info)
    {
        //�� ���� �ʱ�ȭ
        myRoomInfo = info;
        //�� �̸� �ʱ�ȭ
        roomName = info.Name;
        //���� �ο��� �ʱ�ȭ
        roomNameText.text = roomName;
        playerCounterText.text = $"���� �ο� : {info.PlayerCount} / {info.MaxPlayers}";
    }

    //�� ���� ��ư�� ������ ȣ��� �Լ�
    public void OnClickJoin()
    {
        //�н����带 �Է��ϴ� UIâ�� ����.
        LobbyUIManager.Instance.OnPassWord(myRoomInfo);
    }
}
