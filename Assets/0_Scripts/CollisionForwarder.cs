using UnityEngine;
using Photon.Pun;

public class CollisionForwarder : MonoBehaviour
{
	//해당 콜라이더가 속한 거점의 스크립트를 참조
    public CapturePoint capturePoint;

	//거점에 플레이어가 들어오면
	private void OnTriggerEnter(Collider other)
	{
		//PhotonView 선언, 들어온 플레이어로부터 view를 받아낸다.
		PhotonView view = other.GetComponent<PhotonView>();
		//들어온 플레이어가 Runner일 경우
		if (other.CompareTag("Runner"))
		{
			//해당 view에 속하는 플레이어의 UI를 활성화한다.
			if(view != null && view.IsMine)
			{
				capturePoint.ShowLocalUI(true);
			}

			//거점 정보는 MasterClient가 메인으로 관리한다.
			if(!PhotonNetwork.IsMasterClient)
			{
				//MasterClient가 아니면,
				//MasterClien에게 거점 내에 플레이어가 들어왔음을 RPC를 통해 알린다.
				capturePoint.photonView.RPC("RPC_OnRunnerEnter", RpcTarget.MasterClient);
			}
			else
			{
				//MasterClient이면, 그냥 정보를 업데이트 한다.
				capturePoint.OnChildTriggerEnter(other);
			}
		}
	}

	//거점에 플레이어가 나가면
	private void OnTriggerExit(Collider other)
	{
		//PhotonView 선언. 나간 플레이어로부터 view를 받아낸다.
		PhotonView view = other.GetComponent<PhotonView>();

		//나간 플레이어가 Runner이면
		if (other.CompareTag("Runner"))
		{
			//나간 플레이어의 UI를 비활성화 한다.
			if (view != null && view.IsMine)
			{
				capturePoint.ShowLocalUI(false);
			}

			if (!PhotonNetwork.IsMasterClient)
			{
				//MasterClient가 아니면
				//MasterClient에게 거점에 플레이어가 떠났음을 RPC를 통해 알린다.
				capturePoint.photonView.RPC("RPC_OnRunnerExit", RpcTarget.MasterClient);
				//Debug.Log("(Non-Master)플레이어 거점 나감");
			}
			else
			{
				//MasterClient이면, 그냥 정보를 업데이트 한다.
				capturePoint.OnChildTriggerExit(other);
				//Debug.Log("(Master)플레이어 거점 나감");
			}
		}
	}
}
