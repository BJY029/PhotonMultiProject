using UnityEngine;
using System.Collections;

public class LocalBullet : MonoBehaviour
{
	//총알 속도
	public float bulletSpeed = 70f;
	//파괴 속도
	public float destroyTime = 1f;
	//해당 총알 정보
	private Vector3 direction;
	private float maxDistance;
	private Vector3 startPos;

	public void Init(Vector3 dir, float range, string hitkey)
	{
		//생성된 총알의 정보를 초기화 하고
		direction = dir.normalized;
		maxDistance = range;
		startPos = transform.position;
		StopAllCoroutines();
		//총알 코루틴을 실행한다.
		StartCoroutine(MoveAndDestroy(hitkey));
	}

	IEnumerator MoveAndDestroy(string hitkey)
	{
		//목표 지점에 다를 때까지, 목표 지점까지 움직인다.
		while (Vector3.Distance(startPos, transform.position) < maxDistance)
		{
			transform.position += direction * bulletSpeed * Time.deltaTime;
			yield return null;
		}

		//목표 지점에 닿으면, 충돌 이펙트를 생성하고
		GameObject localHit = PoolManager.instance.GetFromPool(hitkey, gameObject.transform.position, Quaternion.identity);
		//해당 이펙트를 파괴하는 코루틴을 재생한다.
		StartCoroutine(DestoryHit(localHit));

		//그리고 총알 이펙트도 파괴한다.(pool을 사용하므로 setactive false)
		gameObject.SetActive(false);
	}

	//충돌 이펙트 관리 코루틴
	IEnumerator DestoryHit(GameObject localHit)
	{
		//일정 시간 지난 후 파괴
		yield return new WaitForSeconds(destroyTime);
		localHit.SetActive(false);
	}
}
