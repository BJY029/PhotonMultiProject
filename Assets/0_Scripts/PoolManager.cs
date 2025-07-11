using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
	//싱글턴
    public static PoolManager instance;

	//각 프리팹 pool을 관리할 dictionary
    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

	private void Awake()
	{
		if(instance == null) { instance = this; }
	}

	//pool을 추가하는 함수
	public void CreatePool(string key, GameObject prefab, int count)
	{
		//해당 프리팹 풀이 존재하면 생성 안함
		if (poolDict.ContainsKey(key)) return;

		//해당 프리팹 풀, 큐를 생성
		Queue<GameObject> queue = new Queue<GameObject>();

		//큐에 프리팹들 생성 및 저장
		for(int i = 0; i < count; i++)
		{
			GameObject obj = Instantiate(prefab);
			obj.SetActive(false);
			queue.Enqueue(obj);
		}
		//딕셔너리에 해당 풀 저장
		poolDict.Add(key, queue);
	}

	//pool에서 오브젝트를 꺼내는 함수
	public GameObject GetFromPool(string key, Vector3 pos, Quaternion rot)
	{
		//해당하는 프리팹 풀이 없으면 실행 안함
		if (!poolDict.ContainsKey(key))
		{
			Debug.LogWarning("Pool not found " + key);
			return null;
		}

		//해당하는 프리팹 풀에서 하나의 오브젝트를 꺼냄
		GameObject obj = poolDict[key].Dequeue();
		//해당 오브젝트를 활성화하고, 위치 및 방향을 설정함
		obj.SetActive(true);
		obj.transform.position = pos;
		obj.transform.rotation = rot;
		//다시 해당 오브젝트를 풀에 넣음 
		poolDict[key].Enqueue(obj);

		return obj;

		//따라서 해당 오브젝트를 파괴하려고 하면, 단순히 SetActive(false)하면 됨
	}
}
