using System.Collections.Generic;
using UnityEngine;

public class CapturePointManager : MonoBehaviour
{
	//�̱���
    public static CapturePointManager Instance;
	//�������� �����ϱ� ���� ���� ����Ʈ ����
	private List<CapturePoint> points = new List<CapturePoint>();

	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	//�� ������ ����Ʈ�� �����ϴ� �Լ�
	public void RegisterPoint(CapturePoint point)
	{
		if(!points.Contains(point)) points.Add(point);
	}

	//�� ���� ������ �̸��� ���� ��ȯ���ִ� �Լ�
	public CapturePoint GetPoint(string name)
	{
		return points.Find(p => p.pointName == name);
	}
}
