using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
	public static PlayerSensor Instance;

	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	//플레이어의 일정 범위 내에 있는 Dummy 들을 저장
	public HashSet<GameObject> detectableDummies = new HashSet<GameObject>();


	//사용자의 범위 내에 들어온 Dummy를 해당 리스트에 저장
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Dummy") || other.CompareTag("Player"))
		{
			detectableDummies.Add(other.gameObject);
			//Debug.Log("더미가 감지되었습니다.");
		}
	}
	//사용자의 범위 밖으로 나간 Dummy들을 리스트에서 제거
	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Dummy")||other.CompareTag("Player"))
		{
			detectableDummies.Remove(other.gameObject);
			//Debug.Log("더미가 삭제되었습니다.");
		}
	}
	//어떤 Dummy가 현재 사용자의 범위 내에 있는지 확인하는 함수
	public bool IsDummyInRange(GameObject Dummy)
	{
		return detectableDummies.Contains(Dummy);
	}
}
