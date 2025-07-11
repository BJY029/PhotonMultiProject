using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Bullet : MonoBehaviourPun
{
	//총알 날아가는 속도
	public float bulletSpeed = 70f;
	//총알 부딪혔을 때 재생될 파티클
	public GameObject impactEffectPrefab;

	//RPC, All 호출
	[PunRPC]
	void RPC_MoveBullet(Vector3 targetPos)
	{
		//총알을 움직이는 코루틴 호출
		StartCoroutine(MoveBullet(targetPos));
	}

	//총알을 목표 지점까지 움직이는 코루틴
	IEnumerator MoveBullet(Vector3 targetPos)
	{
		//총알이 목표 지점까지 도달하기 전까지
		while(Vector3.Distance(transform.position, targetPos) > 0.1f)
		{
			//해당 방향으로 전진
			transform.position = Vector3.MoveTowards(transform.position, targetPos, bulletSpeed * Time.deltaTime);
			yield return null;
		}

		//총알이 목표 지점에 도달하면, 임팩트 파티클을 생성하고 파괴한다.
		//해당 코루틴은 모든 클라이언트에서 재생되는 코드이므로, 해당 파티클 생성 및 파괴는 RPC 호출 하지 않아도 된다.
		if(impactEffectPrefab != null)
		{
			GameObject impact = Instantiate(impactEffectPrefab, targetPos, Quaternion.identity);
			Destroy(impact, 1f);
		}

		if (PhotonNetwork.IsMasterClient)
		{
			//총알 임펙트를 네트워크 상에서 파괴한다.
			PhotonNetwork.Destroy(gameObject);
		}
	}
}
