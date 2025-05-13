using UnityEngine;

//정적 클래스로 선언(instance를 생성하지 않고 어디서든 바로 사용 가능)
public class JsonHelper
{
	//제네릭 함수 정의
	//외부에서 불러온 JSON 텍스트를 입력으로 받는다.
	//반환 값은 T 타입의 배열이다.
	public static T[] FromJson<T>(string json)
	{
		//Unity의 JsonUtility는 []로 시작하는 루트 배열 JSON을 직접 파싱(역직렬화)하지 못함
			//[{"x" : 0}, {"x" : 1}] -> 오류 발생
		//따라서 아래 코드를 사용하여 배열을 points라는 키로 감싼 JSON 객체로 변환한다.
			//{ "points": [ { "x": 0 }, { "x": 1 } ] }
		string wrappedJson = "{\"points\":" + json + "}";
		//감싼 JSON을 Unity가 인식 가능한 형태로 파싱한다.
		//Wrapper<T> 클래스 구조(DummySpawnPoints 클래스)에 맞춰 파싱한 후, 그 안에 있는 points 배열만 꺼내서 반환한다.
		//즉, wrappedJson 문자열을 DummySpawnPoint 클래스 형식에 맞게 역직렬화 하면
			//wrapper.points = new DummySpawnPoint[] {
			//new DummySpawnPoint { x = 0, y = 0, z = 0, rotY = 0 },
			//new DummySpawnPoint { x = 1, y = 0, z = 1, rotY = 90 }};
			//다음과 같은 형식이 되고
		//여기서 .points를 통해서 각 배열들을 반환한다.
		return JsonUtility.FromJson<Wrapper<T>>(wrappedJson).points;
	}


	//내부적으로 JSON을 감싸는 구조로 실제 JSON 파싱 시 사용되는 클래스
	//T 타입의 배열인 points(즉, DummySpawnPoint 클래스 타입 배열의 이름을 points 로 정의)
	[System.Serializable]
	private class Wrapper<T>
	{
		public T[] points;
	}
}

