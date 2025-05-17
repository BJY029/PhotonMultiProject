using Photon.Pun;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
	//Menu 창
	[SerializeField] private GameObject Menu;

	private void Start()
	{
		//Menu 창이 처음에 크기가 0으로 초기화되어 있기 때문에, 크기를 1로 만든다.
		Menu.transform.localScale = Vector3.one;
		//그리고 Menu를 비활성화 한다.
		Menu.SetActive(false);
	}

	private void Update()
	{
		//ESC키가 눌리면
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//Menu 창을 토글한다.
			ToggleMenu();
		}
	}

	//Menu 창을 토글하는 함수
	public void ToggleMenu()
    {
		//Menu 창이 켜져있으면
		if (Menu.activeSelf)
		{
			//Menu를 비활성화 하고
			Menu.SetActive(false);
			//마우스 잠금 및 커서를 안보이게 처리한다.
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else//Menu 창이 꺼져있으면
		{
			//Menu를 활성화하고
			Menu.SetActive(true);
			//마우스 잠금 해제 및 커서를 보이게 처리한다.
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
    }

	//EXIT 버튼의 OnButtonClicke에 적용될 이벤트
    public void ExitButtonClicked()
    {
		//앱을 나간다.
		//추후에는 로비로 나가기로 변경한다.
		Application.Quit();
    }
}
