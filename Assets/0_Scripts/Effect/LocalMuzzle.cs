using Photon.Pun;
using UnityEngine;
using System.Collections;

public class LocalMuzzle : MonoBehaviour
{
	//파티클 파괴 시간
	public float DistoryTime = 1.0f;

	//총 발사 이펙트를 관리할 함수
	public void MuzzleInit()
	{
		StartCoroutine(DestoryMuzzle());
	}

	IEnumerator DestoryMuzzle()
	{
		//일정 시간 대기 후
		yield return new WaitForSeconds(DistoryTime);
		//해당 파티클을 파괴한다.(pool 이므로 active false)
		gameObject.SetActive(false);
	}
}
