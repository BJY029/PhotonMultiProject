using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

//플레이어 정보들을 나타내는 prefab에 적용되는 스크립트
public class RoomPlayerUI : MonoBehaviour
{
	//플레이어의 닉네임을 나타내는 Text 요소
	public Text nickNameText;
	//준비 상태를 나타내는 Text 요소
	public Text readyStatusText;

	//해당 prefab 요소를 초기화 하는 함수
	public void SetUp(Player player)
	{
		//전달받은 플레이어 정보에 맞게 이름 설정
		nickNameText.text = player.NickName;

		//Player의 준비 상태를 받아온다.
		//Player의 해시테이블에 IsReady키가 존재하고, 해당 키에 저장된 값이 true이면 true 값이 저장된다.
		bool isReady = player.CustomProperties.ContainsKey("IsReady") &&
			(bool)player.CustomProperties["IsReady"];

		//Player의 준비 상태에 따라 각 상태 텍스트를 초기화한다.
		readyStatusText.text = isReady ? "Ready" : "Waiting";
		readyStatusText.color = isReady ? Color.green : Color.red;
	}

}
