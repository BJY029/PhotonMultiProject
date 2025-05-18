using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
	public static PlayerSensor Instance;

	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	//�÷��̾��� ���� ���� ���� �ִ� Dummy ���� ����
	public HashSet<GameObject> detectableDummies = new HashSet<GameObject>();


	//������� ���� ���� ���� Dummy�� �ش� ����Ʈ�� ����
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Dummy") || other.CompareTag("Player"))
		{
			detectableDummies.Add(other.gameObject);
			//Debug.Log("���̰� �����Ǿ����ϴ�.");
		}
	}
	//������� ���� ������ ���� Dummy���� ����Ʈ���� ����
	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Dummy")||other.CompareTag("Player"))
		{
			detectableDummies.Remove(other.gameObject);
			//Debug.Log("���̰� �����Ǿ����ϴ�.");
		}
	}
	//� Dummy�� ���� ������� ���� ���� �ִ��� Ȯ���ϴ� �Լ�
	public bool IsDummyInRange(GameObject Dummy)
	{
		return detectableDummies.Contains(Dummy);
	}
}
