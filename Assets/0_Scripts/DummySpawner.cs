using Photon.Pun;
using UnityEngine;

//JSON 파일에서 Dummy의 위치(x, y, z)와 회전(rotY)를 읽어서
//게임 내 위치로 바꾸기 위한 데이터 구조 정의
//[System.Serializable]를 통해 해당 클래스가 직렬화 가능하다는 것을 Unity에 알림(Attribute)
//Unity는 다음과 같이 [System.Serializable]로 선언된 클래스만 직렬화가 가능하다.
	//즉 JsonUtility로 JSON <-> 객체 변환이 가능해진다.
[System.Serializable]
public class DummySpawnPoint
{
    public float x, y, z, roty;
}


public class DummySpawner : MonoBehaviour
{
	//해당 스크립트를 싱글턴화
	public static DummySpawner instance;

	private void Awake()
	{
		if (instance == null) instance = this;
	}


	//JSON을 통해서 spawn 위치를 불러와서 더미들을 생성하는 함수
	public void LoadAndSpawnDummies()
	{
		//Resources 폴더에서 dummy_spawn_points.json 파일을 불러온다.
		TextAsset jsonFile = Resources.Load<TextAsset>("dummy_spawn_points");

		if(jsonFile == null)
		{
			Debug.LogError("Dummy Spawn Json 파일을 찾을 수 없습니다.");
			return;
		}

		//JSON 문자열을 DummySpawnPoint 배열로 역직렬화한다.
		//이때 따로 정의한 JsonHelper를 사용해서 역직렬화를 한다.
		DummySpawnPoint[] spawnPoints = JsonHelper.FromJson<DummySpawnPoint>(jsonFile.text);

		//역직렬화 된 위치 배열을 돌며 각각에 Dummy들을 생성한다.
		foreach(var point in spawnPoints)
		{
			Vector3 pos = new Vector3(point.x, point.y, point.z);
			Quaternion rot = Quaternion.Euler(0, point.roty, 0);
			//모든 클라이언트에 동기화 되는 Dummy 생성
			PhotonNetwork.Instantiate("DummyKyle", pos, rot);
		}
	}
}
