using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CapturePointManager : MonoBehaviourPun
{
	//�̱���
	public static CapturePointManager Instance;
	//�������� �����ϱ� ���� ���� ����Ʈ ����
	private List<CapturePoint> points = new List<CapturePoint>();
	//���ɵ� ������ �����ϴ� ����Ʈ
	public List<CapturePoint> Captured = new List<CapturePoint>();

	private void Awake()
	{
		if (Instance == null) Instance = this;
	}

	//�� ������ ����Ʈ�� �����ϴ� �Լ�
	public void RegisterPoint(CapturePoint point)
	{
		if (!points.Contains(point)) points.Add(point);
	}

	//�� ���� ������ �̸��� ���� ��ȯ���ִ� �Լ�
	public CapturePoint GetPoint(string name)
	{
		return points.Find(p => p.pointName == name);
	}

	//MasterClient�� ���� �����ϴ�.
	//���ɵ� ������ ����Ʈ�� �����Ѵ�.
	public void UpdateCapturedPoint(CapturePoint point)
	{
		if (!PhotonNetwork.IsMasterClient) return;

		if (!Captured.Contains(point)) Captured.Add(point);
	}

	//���ɵ� ���� ���� ������ �����ϴ��� Ȯ��
	public void CheckAllCaptured()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		if (Captured.Count >= 3) //�ӽ÷� 3���� �������� ����, 3���� �����Ǹ�
		{
			//��� �������� ��Ȱ��ȭ�Ѵ�.
			foreach (CapturePoint capturePoint in points)
			{
				//RPC�� Ȱ���ؼ� ��� �÷��̾��� ���� ������Ʈ�� ��Ȱ��ȭ�Ѵ�.
				photonView.RPC("DeactiveSite", RpcTarget.All, capturePoint.pointName);
			}

			//PhotonNetwork���� PlayerList�� �޾ƿ´�.
			foreach (Player player in PhotonNetwork.PlayerList)
			{
				//�ش� Player�� CustomProperties�� Role Ű ���� �ִ��� Ȯ���Ѵ�.
				if (player.CustomProperties.ContainsKey("Role"))
				{
					//������, �ش� Ű�� ����� ��(���� ��)�� �����´�.
					string role = (string)player.CustomProperties["Role"];
					if (role == "Runner") //Runner�̸�
					{
						//ǥ���� �ؽ�Ʈ
						string str = "Destroy the generator and kill the Seeker.";
						//�ش� �ؽ�Ʈ ���� ���� Vector2 2���� ǥ���Ѵ�.
						Vector2 colorVec1 = new Vector2(0f, 1f); // r=0, g=1
						Vector2 colorVec2 = new Vector2(0f, 1f); // b=0, a=1
						//RPC�� ���� ��� UI ���� �� �÷��̾�� �����Ѵ�.
						Game_UIManager.instance.photonView.RPC("AlertAllPointCapturedF", player, str, colorVec1, colorVec2);
					}
					else if (role == "Seeker") //Seeker�̸�
					{
						//ǥ���� �ؽ�Ʈ
						string str = "Once all the generators are destroyed, you are fired. Kill all the Runners.";
						//�ش� �ؽ�Ʈ ���� ���� Vector2 2���� ǥ���Ѵ�.
						Vector2 colorVec1 = new Vector2(1f, 0f); // r=1, g=0
						Vector2 colorVec2 = new Vector2(0f, 1f); // b=0, a=1
						//RPC�� ���� ��� UI ���� �� �÷��̾�� �����Ѵ�.
						Game_UIManager.instance.photonView.RPC("AlertAllPointCapturedF", player, str, colorVec1, colorVec2);
					}
				}
			}
		}
	}

	//�� Site�� ��Ȱ��ȭ �ϴ� Rpc �Լ�
	[PunRPC]
	void DeactiveSite(string pointName)
	{
		//���� �̸��� ���� ���� ������Ʈ�� �޾ƿ´�.
		CapturePoint point = GetPoint(pointName);
		if (point != null)
		{
			//�ش� ������Ʈ�� ��Ȱ��ȭ�Ѵ�.�켱 C
			point.gameObject.SetActive(false);
		}
	}
}