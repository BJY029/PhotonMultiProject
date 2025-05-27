using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

//�÷��̾� �������� ��Ÿ���� prefab�� ����Ǵ� ��ũ��Ʈ
public class RoomPlayerUI : MonoBehaviour
{
	//�÷��̾��� �г����� ��Ÿ���� Text ���
	public Text nickNameText;
	//�غ� ���¸� ��Ÿ���� Text ���
	public Text readyStatusText;

	//�ش� prefab ��Ҹ� �ʱ�ȭ �ϴ� �Լ�
	public void SetUp(Player player)
	{
		//���޹��� �÷��̾� ������ �°� �̸� ����
		nickNameText.text = player.NickName;

		//Player�� �غ� ���¸� �޾ƿ´�.
		//Player�� �ؽ����̺� IsReadyŰ�� �����ϰ�, �ش� Ű�� ����� ���� true�̸� true ���� ����ȴ�.
		bool isReady = player.CustomProperties.ContainsKey("IsReady") &&
			(bool)player.CustomProperties["IsReady"];

		//Player�� �غ� ���¿� ���� �� ���� �ؽ�Ʈ�� �ʱ�ȭ�Ѵ�.
		readyStatusText.text = isReady ? "Ready" : "Waiting";
		readyStatusText.color = isReady ? Color.green : Color.red;
	}

}
