using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

	public GameObject camera;

	private void Start()
	{
		SpawnPlayer();
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

	//�÷��̾ �����ϴ� �Լ�
	void SpawnPlayer()
	{
		//������ ��ġ�� �÷��̾� ������ ����
		Vector3 spawnPosition = new Vector3(Random.Range(-85.0f, -90f), 2.0f, Random.Range(10.0f, 30.0f));
		PhotonNetwork.Instantiate("RobotKyle", spawnPosition, Quaternion.identity);
		//������ ī�޶� �ϳ��� �����ؾ� �ϸ�, Audio Listener�� �ϳ� �����ؾ� �ϱ� ������, �⺻ ī�޶� ������ ����
		//�÷��̾� �ϳ� �̻��� ���� ���, �⺻ ī�޶� ��Ȱ��ȭ ��Ų��.
		if (camera != null) camera.SetActive(false);
	}
}
