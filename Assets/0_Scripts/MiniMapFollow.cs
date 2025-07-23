using UnityEngine;

public class MiniMapFollow : MonoBehaviour
{
	//싱글턴
	public static MiniMapFollow instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//따라갈 플레이어
	public Transform player;

	private void LateUpdate()
	{
		//따라갈 플레이어가 null이 아니면
		if (player != null)
		{
			//해당 플레이어를 고정된 y축을 가지고 따라가도록 설정한다.
			Vector3 newPos = player.position;
			newPos.y = transform.position.y;
			transform.position = newPos;
		}
	}
}
