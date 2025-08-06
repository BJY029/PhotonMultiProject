using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class ItemManager : MonoBehaviourPun
{
	public static ItemManager Instance;

	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	//아이템 스폰 위치를 관리하는 리스트
	public List<Transform> spawnPoints = new List<Transform>();
	//스폰되는 아이템 프리팹들
	public List<GameObject> ItemPrefabs = new List<GameObject>();
	//현재 활성화 된 아이템들을 위치를 기반으로 관리하는 리스트
	public Dictionary<float, GameObject> activeItems = new Dictionary<float, GameObject>();

	//아이템이 새로 스폰될 때까지 적용되는 딜레이
	[SerializeField]
	private float Delay = 15f;

	private void Start()
	{
		//MasterClient만 실행
		if (!PhotonNetwork.IsMasterClient) return;

		//각각의 스폰 위치에 랜덤 아이템을 스폰시킨다.
		foreach(Transform item in spawnPoints)
		{
			SpawnRandomItem(item);
		}
	}

	//랜덤 아이템을 지정된 위치에 스폰하는 함수
	//MasterClient만 수행해야 한다.
	void SpawnRandomItem(Transform spawnPoint)
	{
		//랜덤 값 적용 및 아이템 선택
		int index = Random.Range(0, ItemPrefabs.Count);
		string prefabName = ItemPrefabs[index].name;

		//선택된 아이템을 해당되는 위치에 Photon으로 생성
		GameObject item =  PhotonNetwork.Instantiate(prefabName, spawnPoint.position, Quaternion.identity);
		//해당 아이템을 활성화 딕셔너리에 삽입
		activeItems[spawnPoint.transform.position.x] = item;
	}

	//아이템이 플레이어에게 먹히면 호출되는 함수
	[PunRPC]
	void RPC_PickUpItem(Vector3 position)
	{
		//MasterClient만 수행한다.
		if(!PhotonNetwork.IsMasterClient) return;

		//아이템을 먹은 위치로부터 가장 가까운 스폰 위치를 받아온다.
		Transform spawnPoint = FindClosestSpawnPoint(position);
		//해당 스폰 위치를 기반으로 활성화 된 아이템을 제거하고, 코루틴을 실행한다.
		if(spawnPoint != null && activeItems.ContainsKey(spawnPoint.transform.position.x))
		{
			GameObject item = activeItems[spawnPoint.transform.position.x];
			PhotonNetwork.Destroy(item);
			activeItems.Remove(spawnPoint.transform.position.x);
			StartCoroutine(RespawnAfterDelay(spawnPoint));
		}
	}

	public void PlayItemAudio(Vector3 pos)
	{
		GameObject prefab = Resources.Load<GameObject>("ItemAudio");
		GameObject go = Instantiate(prefab, pos, Quaternion.identity);

		AudioSource audioSource = go.GetComponent<AudioSource>();
		audioSource.clip = AudioManager.instance.getRandomItemClip();
		audioSource.Play();

		Destroy(go, audioSource.clip.length);
	}

	//현재 위치 기반으로 가장 가까운 스폰 위치를 찾아서 반환해 주는 함수
	Transform FindClosestSpawnPoint(Vector3 position)
	{
		//초기화
		Transform closest = null;
		float minDist = Mathf.Infinity;

		//각 스폰 위치를 돌면서 가장 가까운 거리의 transfrom을 찾아서 반환한다.
		foreach (Transform point in spawnPoints)
		{
			float dist = Vector3.Distance(point.position, position);
			if (dist < minDist)
			{
				minDist = dist;
				closest = point;
			}
		}

		return closest;
	}

	//일정 시간 이후에 아이템을 스폰 시키는 코루틴
	IEnumerator RespawnAfterDelay(Transform spawnPosition)
	{
		yield return new WaitForSeconds(Delay);
		SpawnRandomItem(spawnPosition);
	}
}
