using UnityEngine;

public class LogPusher : MonoBehaviour
{
	//밀어내는 힘
    public float pushForce = 3f;
	//밀어내는 방향
    public Vector3 pushDirection = Vector3.left;

	//트리거와 접촉하고 있는 모든 collider other에 대해 프레임당 한 번 씩 호출되는 함수
	private void OnTriggerStay(Collider other)
	{
		//해당 플레이어의 CharacterController를 가져오고
		var controller = other.GetComponent<CharacterController>();
		if (controller != null &&  (other.CompareTag("Runner") || other.CompareTag("Seeker"))) {
			//밀어내는 힘을 가한다.
			controller.Move(pushDirection.normalized * pushForce * Time.deltaTime);
		}
	}
}
