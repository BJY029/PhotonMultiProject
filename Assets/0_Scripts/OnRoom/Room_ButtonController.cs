using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Room_ButtonController : MonoBehaviour
{
	//Ready 버튼이 눌리면 호출되는 함수
	public void ToogleReady()
    {
        //플레이어의 준비 상태를 나타내는 변수
        bool isReady = 
            //CustomProperties에 "IsReady" 키가 존재하지 않거나
            !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsReady") ||
            //"IsReady"키가 존재하지만, 값이 false인 경우 ready를 true로 만든다.
            !(bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];

        //CustomProperties을 변경하기 위해서는 Hashtable로 키-값 쌍을 만들어야 한다.
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            {"IsReady", isReady }
        };

        //로컬 플레이어의 CustomProperties를 서버에 업데이트 한다.
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }


    public void StartGame()
    {
        //MasterClient만 역할을 부여한다.
        if (!PhotonNetwork.IsMasterClient) return;

        //플레이어 리스트를 받아온다.
        var players = PhotonNetwork.PlayerList.ToList();
        //해당 플레이어들을 랜덤으로 섞는다.
        Shuffle(players);

        //첫 번째에 위치한 플레이어를 seeker(술래)로 선정한다.
        Player seeker = players[0];

        //플레이어 리스트를 돌면서
        foreach (var player in players)
        {
            //Seeker에 해당되는 플레이어는 Seeker, 아닌 나머지는 Runner로 역할을 Hashtable에 저장한다.
            string role = (player == seeker) ? "Seeker" : "Runner";

            var props = new ExitGames.Client.Photon.Hashtable
            {
                { "Role", role }
            };

            //로컬 플레이어릐 CustomProperties를 서버에 업데이트 한다.
            player.SetCustomProperties(props);
        }

        //게임 씬으로 이동한다.
        PhotonNetwork.LoadLevel("GameScene");
    }

    //Fisher-Yates Shuffle 알고리즘
    public void Shuffle(IList list)
    {
        //랜덤 객체 생성
        System.Random rand = new System.Random();
        //리스트의 요소 개수를 파악
        int n = list.Count;

        //마지막 하나가 남을 때까지
        while(n > 1)
        {
            //랜덤한 위치와 임의의 위치를 swap 한다.
            n--;
            int k = rand.Next(n + 1); //0 ~ n
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
