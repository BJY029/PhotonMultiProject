using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using WebSocketSharp;

public class LobbyUIManager : MonoBehaviour
{
	//싱글턴화
	public static LobbyUIManager Instance;
	//LobbyManager는 싱글턴으로 만들지 않는다.
	public LobbyManager LobbyManager;
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	public GameObject CreateRoomFrame;
	public InputField RoomName;
	public InputField Password;


	private void Start()
	{
		//싱글턴이 정상적으로 적용되기 위해서 active 한 상태로 게임이 시작되므로
		//방해되지 않게 현재 localScale을 0으로 설정한 상태
		//따라서 localScale을 다시 1로 설정해준다.
		CreateRoomFrame.transform.localScale = Vector3.one;
		//그리고 비활성화한다.
		CreateRoomFrame.gameObject.SetActive(false);
	}

	//Create 버튼이 눌린경우, 다음 함수가 실행된다.
	public void OnClickedCreateRoom()
	{
		if (!CreateRoomFrame.activeSelf)
		{
			CreateRoomFrame.SetActive(true);
		}
	}

	//x 버튼이 눌린 경우 다음 함수가 실행된다.
	public void OnExitCreateRoom()
	{
		if (CreateRoomFrame.activeSelf)
		{
			CreateRoomFrame.SetActive(false);
		}
	}

	//방 생성 창에서 정보를 입력하고, Create 버튼을 누르면 호출되는 함수
	public void OnClickedCreateBtn()
	{
		//모든 요소가 입력되지 않은 상태라면, Warning 메시지를 생성한다.
		if (RoomName.text.IsNullOrEmpty() || Password.text.IsNullOrEmpty())
		{
			WarningPopUp.Instance.PopUpWarning("모든 요소를 입력하세요!");
			return;
		}

		LobbyManager.CreateRoomWithPassword(RoomName.text, Password.text);
	}
}
