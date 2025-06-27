using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

	private void Start()
	{

	}

	//���� ������ ���
	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("������ ������ ���� ������ �����Ǿ����ϴ� : " + cause);
	}


	//MasterClient�� ����Ǹ� ȣ��Ǵ� �Լ�
	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("MasterClient�� ����Ǿ����ϴ�.");

		//MasterClient�� �����
		if (PhotonNetwork.IsMasterClient)
		{
			//�� ���� ����.
			Application.Quit();
		}
	}
}
