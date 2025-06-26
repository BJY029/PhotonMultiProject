using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
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
}
