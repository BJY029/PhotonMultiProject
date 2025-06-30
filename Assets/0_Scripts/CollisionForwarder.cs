using UnityEngine;
using Photon.Pun;

public class CollisionForwarder : MonoBehaviour
{
	//�ش� �ݶ��̴��� ���� ������ ��ũ��Ʈ�� ����
    public CapturePoint capturePoint;

	//������ �÷��̾ ������
	private void OnTriggerEnter(Collider other)
	{
		//PhotonView ����, ���� �÷��̾�κ��� view�� �޾Ƴ���.
		PhotonView view = other.GetComponent<PhotonView>();
		//���� �÷��̾ Runner�� ���
		if (other.CompareTag("Runner"))
		{
			//�ش� view�� ���ϴ� �÷��̾��� UI�� Ȱ��ȭ�Ѵ�.
			if(view != null && view.IsMine)
			{
				capturePoint.ShowLocalUI(true);
			}

			//���� ������ MasterClient�� �������� �����Ѵ�.
			if(!PhotonNetwork.IsMasterClient)
			{
				//MasterClient�� �ƴϸ�,
				//MasterClien���� ���� ���� �÷��̾ �������� RPC�� ���� �˸���.
				capturePoint.photonView.RPC("RPC_OnRunnerEnter", RpcTarget.MasterClient);
			}
			else
			{
				//MasterClient�̸�, �׳� ������ ������Ʈ �Ѵ�.
				capturePoint.OnChildTriggerEnter(other);
			}
		}
	}

	//������ �÷��̾ ������
	private void OnTriggerExit(Collider other)
	{
		//PhotonView ����. ���� �÷��̾�κ��� view�� �޾Ƴ���.
		PhotonView view = other.GetComponent<PhotonView>();

		//���� �÷��̾ Runner�̸�
		if (other.CompareTag("Runner"))
		{
			//���� �÷��̾��� UI�� ��Ȱ��ȭ �Ѵ�.
			if (view != null && view.IsMine)
			{
				capturePoint.ShowLocalUI(false);
			}

			if (!PhotonNetwork.IsMasterClient)
			{
				//MasterClient�� �ƴϸ�
				//MasterClient���� ������ �÷��̾ �������� RPC�� ���� �˸���.
				capturePoint.photonView.RPC("RPC_OnRunnerExit", RpcTarget.MasterClient);
				//Debug.Log("(Non-Master)�÷��̾� ���� ����");
			}
			else
			{
				//MasterClient�̸�, �׳� ������ ������Ʈ �Ѵ�.
				capturePoint.OnChildTriggerExit(other);
				//Debug.Log("(Master)�÷��̾� ���� ����");
			}
		}
	}
}
