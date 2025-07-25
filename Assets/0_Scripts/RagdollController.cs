using Photon.Pun;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
	//플레이어 애니메이터
    private Animator animator;
	//레그돌용 강체들
    private Rigidbody[] ragdollBodies;

	//초기화
	private void Start()
	{
		animator = GetComponent<Animator>();
		//자식들로부터 각 강체들을 얻어온다.
		ragdollBodies = GetComponentsInChildren<Rigidbody>();
		//해당 강체들을 비활성화시킨다.
		SetRagdoll(false);
	}

	//인자에 따라 레그돌 활성화 및 비활성화를 진행하는 함수
	public void SetRagdoll(bool isActive)
	{
		//레그돌 비활성화시
		if (!isActive)
		{
			//애니메이터는 그대로 true로 유지
			animator.enabled = true;
		}
		else//레그돌 활성화시
		{
			//애니메이터를 현재 위치에서 업데이트 정지
			animator.Update(0);
			//그리고 비활성화
			animator.enabled = false;
		}

		//각 강체들을 설정
		foreach(Rigidbody rb in ragdollBodies)
		{
			//레그돌 활성화 여부에 따라 각 부위의 강체를 설정한다.
			rb.isKinematic = !isActive;
			rb.detectCollisions = isActive;
		}
	}

	//죽을 때 호출될 함수
	public void OnDeath()
	{
		SetRagdoll(true);
	}
}
