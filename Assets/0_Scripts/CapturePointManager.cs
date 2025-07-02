using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CapturePointManager : MonoBehaviourPun
{
	//싱글턴
	public static CapturePointManager Instance;
	//거점들을 관리하기 위한 거점 리스트 선언
	private List<CapturePoint> points = new List<CapturePoint>();
	//점령된 거점을 저장하는 리스트
	public List<CapturePoint> Captured = new List<CapturePoint>();

	private void Awake()
	{
		if (Instance == null) Instance = this;
	}

	//각 거점을 리스트에 삽입하는 함수
	public void RegisterPoint(CapturePoint point)
	{
		if (!points.Contains(point)) points.Add(point);
	}

	//각 거점 정보를 이름을 통해 반환해주는 함수
	public CapturePoint GetPoint(string name)
	{
		return points.Find(p => p.pointName == name);
	}

	//MasterClient만 접근 가능하다.
	//점령된 거점을 리스트에 저장한다.
	public void UpdateCapturedPoint(CapturePoint point)
	{
		if (!PhotonNetwork.IsMasterClient) return;

		if (!Captured.Contains(point)) Captured.Add(point);
	}

	//점령된 거점 개수 조건을 만족하는지 확인
	public void CheckAllCaptured()
	{
		if (!PhotonNetwork.IsMasterClient) return;

		if (Captured.Count >= 3) //임시로 3개의 거점으로 설정, 3개가 만족되면
		{
			//모든 거점들을 비활성화한다.
			foreach (CapturePoint capturePoint in points)
			{
				//RPC를 활용해서 모든 플레이어의 거점 오브젝트를 비활성화한다.
				photonView.RPC("DeactiveSite", RpcTarget.All, capturePoint.pointName);
			}

			//PhotonNetwork에서 PlayerList를 받아온다.
			foreach (Player player in PhotonNetwork.PlayerList)
			{
				//해당 Player의 CustomProperties에 Role 키 값이 있는지 확인한다.
				if (player.CustomProperties.ContainsKey("Role"))
				{
					//있으면, 해당 키에 저장된 값(역할 값)을 가져온다.
					string role = (string)player.CustomProperties["Role"];
					if (role == "Runner") //Runner이면
					{
						//표시할 텍스트
						string str = "Destroy the generator and kill the Seeker.";
						//해당 텍스트 색상 값을 Vector2 2개로 표현한다.
						Vector2 colorVec1 = new Vector2(0f, 1f); // r=0, g=1
						Vector2 colorVec2 = new Vector2(0f, 1f); // b=0, a=1
						//RPC를 통해 띄울 UI 값을 각 플레이어에게 전달한다.
						Game_UIManager.instance.photonView.RPC("AlertAllPointCapturedF", player, str, colorVec1, colorVec2);
					}
					else if (role == "Seeker") //Seeker이면
					{
						//표시할 텍스트
						string str = "Once all the generators are destroyed, you are fired. Kill all the Runners.";
						//해당 텍스트 색상 값은 Vector2 2개로 표현한다.
						Vector2 colorVec1 = new Vector2(1f, 0f); // r=1, g=0
						Vector2 colorVec2 = new Vector2(0f, 1f); // b=0, a=1
						//RPC를 통해 띄울 UI 값을 각 플레이어에게 전달한다.
						Game_UIManager.instance.photonView.RPC("AlertAllPointCapturedF", player, str, colorVec1, colorVec2);
					}
				}
			}
		}
	}

	//각 Site를 비활성화 하는 Rpc 함수
	[PunRPC]
	void DeactiveSite(string pointName)
	{
		//거점 이름을 통해 거점 오브젝트를 받아온다.
		CapturePoint point = GetPoint(pointName);
		if (point != null)
		{
			//해당 오브젝트를 비활성화한다.우선 C
			point.gameObject.SetActive(false);
		}
	}
}