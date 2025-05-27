using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerUI : MonoBehaviour
{
	public Text nickNameText;
	public Text readyStatusText;

	public void SetUp(Player player)
	{
		nickNameText.text = player.NickName;

		bool isReady = player.CustomProperties.ContainsKey("IsReady") &&
			(bool)player.CustomProperties["IsReady"];

		readyStatusText.text = isReady ? "Ready" : "Waiting";
		readyStatusText.color = isReady ? Color.green : Color.red;
	}

}
