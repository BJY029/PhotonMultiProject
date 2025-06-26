using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Room_ButtonController : MonoBehaviour
{
	//Ready ��ư�� ������ ȣ��Ǵ� �Լ�
	public void ToogleReady()
    {
        //�÷��̾��� �غ� ���¸� ��Ÿ���� ����
        bool isReady = 
            //CustomProperties�� "IsReady" Ű�� �������� �ʰų�
            !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsReady") ||
            //"IsReady"Ű�� ����������, ���� false�� ��� ready�� true�� �����.
            !(bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];

        //CustomProperties�� �����ϱ� ���ؼ��� Hashtable�� Ű-�� ���� ������ �Ѵ�.
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            {"IsReady", isReady }
        };

        //���� �÷��̾��� CustomProperties�� ������ ������Ʈ �Ѵ�.
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
