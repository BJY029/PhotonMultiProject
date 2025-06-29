using System.Collections.Generic;
using UnityEngine;

public class CapturePointManager : MonoBehaviour
{
	//싱글턴
    public static CapturePointManager Instance;
	//거점들을 관리하기 위한 거점 리스트 선언
	private List<CapturePoint> points = new List<CapturePoint>();

	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	//각 거점을 리스트에 삽입하는 함수
	public void RegisterPoint(CapturePoint point)
	{
		if(!points.Contains(point)) points.Add(point);
	}

	//각 거점 정보를 이름을 통해 반환해주는 함수
	public CapturePoint GetPoint(string name)
	{
		return points.Find(p => p.pointName == name);
	}
}
