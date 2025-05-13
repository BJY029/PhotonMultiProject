using Photon.Pun;
using UnityEngine;

//JSON ���Ͽ��� Dummy�� ��ġ(x, y, z)�� ȸ��(rotY)�� �о
//���� �� ��ġ�� �ٲٱ� ���� ������ ���� ����
//[System.Serializable]�� ���� �ش� Ŭ������ ����ȭ �����ϴٴ� ���� Unity�� �˸�(Attribute)
//Unity�� ������ ���� [System.Serializable]�� ����� Ŭ������ ����ȭ�� �����ϴ�.
	//�� JsonUtility�� JSON <-> ��ü ��ȯ�� ����������.
[System.Serializable]
public class DummySpawnPoint
{
    public float x, y, z, roty;
}


public class DummySpawner : MonoBehaviour
{
	//�ش� ��ũ��Ʈ�� �̱���ȭ
	public static DummySpawner instance;

	private void Awake()
	{
		if (instance == null) instance = this;
	}


	//JSON�� ���ؼ� spawn ��ġ�� �ҷ��ͼ� ���̵��� �����ϴ� �Լ�
	public void LoadAndSpawnDummies()
	{
		//Resources �������� dummy_spawn_points.json ������ �ҷ��´�.
		TextAsset jsonFile = Resources.Load<TextAsset>("dummy_spawn_points");

		if(jsonFile == null)
		{
			Debug.LogError("Dummy Spawn Json ������ ã�� �� �����ϴ�.");
			return;
		}

		//JSON ���ڿ��� DummySpawnPoint �迭�� ������ȭ�Ѵ�.
		//�̶� ���� ������ JsonHelper�� ����ؼ� ������ȭ�� �Ѵ�.
		DummySpawnPoint[] spawnPoints = JsonHelper.FromJson<DummySpawnPoint>(jsonFile.text);

		//������ȭ �� ��ġ �迭�� ���� ������ Dummy���� �����Ѵ�.
		foreach(var point in spawnPoints)
		{
			Vector3 pos = new Vector3(point.x, point.y, point.z);
			Quaternion rot = Quaternion.Euler(0, point.roty, 0);
			//��� Ŭ���̾�Ʈ�� ����ȭ �Ǵ� Dummy ����
			PhotonNetwork.Instantiate("DummyKyle", pos, rot);
		}
	}
}
