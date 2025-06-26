using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Linq;
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


    public void StartGame()
    {
        //MasterClient�� ������ �ο��Ѵ�.
        if (!PhotonNetwork.IsMasterClient) return;

        //�÷��̾� ����Ʈ�� �޾ƿ´�.
        var players = PhotonNetwork.PlayerList.ToList();
        //�ش� �÷��̾���� �������� ���´�.
        Shuffle(players);

        //ù ��°�� ��ġ�� �÷��̾ seeker(����)�� �����Ѵ�.
        Player seeker = players[0];

        //�÷��̾� ����Ʈ�� ���鼭
        foreach (var player in players)
        {
            //Seeker�� �ش�Ǵ� �÷��̾�� Seeker, �ƴ� �������� Runner�� ������ Hashtable�� �����Ѵ�.
            string role = (player == seeker) ? "Seeker" : "Runner";

            var props = new ExitGames.Client.Photon.Hashtable
            {
                { "Role", role }
            };

            //���� �÷��̾�l CustomProperties�� ������ ������Ʈ �Ѵ�.
            player.SetCustomProperties(props);
        }

        //���� ������ �̵��Ѵ�.
        PhotonNetwork.LoadLevel("GameScene");
    }

    //Fisher-Yates Shuffle �˰���
    public void Shuffle(IList list)
    {
        //���� ��ü ����
        System.Random rand = new System.Random();
        //����Ʈ�� ��� ������ �ľ�
        int n = list.Count;

        //������ �ϳ��� ���� ������
        while(n > 1)
        {
            //������ ��ġ�� ������ ��ġ�� swap �Ѵ�.
            n--;
            int k = rand.Next(n + 1); //0 ~ n
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
