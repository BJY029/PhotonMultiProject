using UnityEngine;
using Photon.Pun;

public class SwitchLabtop : MonoBehaviourPun
{
	//해당 오브젝트의 MeshRenderer
    private MeshRenderer m_MeshRenderer;
	//해당 오브젝트의 box collider
	private BoxCollider m_BoxCollider;
	//상호작용시 적용될 머테리얼
    public Material turnoffMat;

	public void Start()
	{
		//초기화
		m_MeshRenderer = GetComponent<MeshRenderer>();
		m_BoxCollider = GetComponent<BoxCollider>();
	}

	//RPC로 실행시켜서 모든 플레이어에게 적용되도록 한다.
	[PunRPC]
	public void ChangeToTurnoffMat()
	{
		//해당 오브젝트의 머테리얼을 변경하고
		m_MeshRenderer.material = turnoffMat;
		//Boxcollider를 꺼서 상호작용이 불가하도록 한다.
		m_BoxCollider.enabled = false;
	}
}
