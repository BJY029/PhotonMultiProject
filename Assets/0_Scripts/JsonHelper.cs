using UnityEngine;

//���� Ŭ������ ����(instance�� �������� �ʰ� ��𼭵� �ٷ� ��� ����)
public class JsonHelper
{
	//���׸� �Լ� ����
	//�ܺο��� �ҷ��� JSON �ؽ�Ʈ�� �Է����� �޴´�.
	//��ȯ ���� T Ÿ���� �迭�̴�.
	public static T[] FromJson<T>(string json)
	{
		//Unity�� JsonUtility�� []�� �����ϴ� ��Ʈ �迭 JSON�� ���� �Ľ�(������ȭ)���� ����
			//[{"x" : 0}, {"x" : 1}] -> ���� �߻�
		//���� �Ʒ� �ڵ带 ����Ͽ� �迭�� points��� Ű�� ���� JSON ��ü�� ��ȯ�Ѵ�.
			//{ "points": [ { "x": 0 }, { "x": 1 } ] }
		string wrappedJson = "{\"points\":" + json + "}";
		//���� JSON�� Unity�� �ν� ������ ���·� �Ľ��Ѵ�.
		//Wrapper<T> Ŭ���� ����(DummySpawnPoints Ŭ����)�� ���� �Ľ��� ��, �� �ȿ� �ִ� points �迭�� ������ ��ȯ�Ѵ�.
		//��, wrappedJson ���ڿ��� DummySpawnPoint Ŭ���� ���Ŀ� �°� ������ȭ �ϸ�
			//wrapper.points = new DummySpawnPoint[] {
			//new DummySpawnPoint { x = 0, y = 0, z = 0, rotY = 0 },
			//new DummySpawnPoint { x = 1, y = 0, z = 1, rotY = 90 }};
			//������ ���� ������ �ǰ�
		//���⼭ .points�� ���ؼ� �� �迭���� ��ȯ�Ѵ�.
		return JsonUtility.FromJson<Wrapper<T>>(wrappedJson).points;
	}


	//���������� JSON�� ���δ� ������ ���� JSON �Ľ� �� ���Ǵ� Ŭ����
	//T Ÿ���� �迭�� points(��, DummySpawnPoint Ŭ���� Ÿ�� �迭�� �̸��� points �� ����)
	[System.Serializable]
	private class Wrapper<T>
	{
		public T[] points;
	}
}

