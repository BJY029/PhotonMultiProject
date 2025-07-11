using UnityEngine;

public class CameraCollision : MonoBehaviour
{
	public static CameraCollision instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//Player의 위치 조정
	public Transform player;
	//기본 카메라 거리
	public float distance = 5f;
	//확대 카메라 거리
	public float zoomDistance = 3.5f;
	//카메라 거리 조정 속도
	public float smoothSpeed = 10f;
	//현재 카메라 거리
	private float currentDistance;
	//총 발사 플래그
	public bool shoot;
	//벽 레이어
	public LayerMask collisionLayer;
	//확대 여부 flag
	public bool zoom;

	private void Start()
	{
		zoom = false;
	}

	private void LateUpdate()
	{
		currentDistance = zoom ? zoomDistance : distance;
		if (shoot) currentDistance *= 1.5f;
		//기본 카메라 위치 설정
		Vector3 desiredCameraPos = player.position - player.forward * currentDistance + Vector3.up * 2f;

		//RayCasy 정의
		RaycastHit hit;
		//Ray 발사 방향 : 캐릭터 기준 카메라 방향
		Vector3 rayDirection = (desiredCameraPos - player.position).normalized;
		//Ray 거리 : 우선 기본 카메라 거리로 초기화
		float rayDistance = currentDistance;
		//캐릭터 위치에서 Ray 방향으로 Ray 광선 발사, 최대 거리는 기본 거리인 distance, 충돌 레이어는 collisionLayer로 한정
		if (Physics.Raycast(player.position, rayDirection, out hit, currentDistance, collisionLayer))
		{
			//충돌체가 존재하면, 해당 충돌치 거리만큼 Ray 거리를 초기화
			//1을 빼는 이유는, 카메라가 벽에 딱 붙지 않도록 방지하기 위함
			rayDistance = hit.distance - 1f;
		}

		//최종 카메라 거리 계산
		//충돌체가 없다면 기존과 같은 위치
		//충돌치게 있으면, 그에 맞게 계산된 RayDistance 만큼 카메라가 떨어져서 존재
		Vector3 finalCameraPos = player.position - player.forward * rayDistance + Vector3.up * 2f;
		//최종 카메라 위치 변경
		//기존 카메라에서 새롭게 갱신된 카메라 위치를 부드럽게 전환하기 위해 Lerp 사용
		transform.position = Vector3.Lerp(transform.position, finalCameraPos, Time.deltaTime * smoothSpeed);
	}
}
