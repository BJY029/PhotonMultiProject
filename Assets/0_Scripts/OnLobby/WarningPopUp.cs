using UnityEngine;
using UnityEngine.UI;

public class WarningPopUp : MonoBehaviour
{
	//싱글턴으로 제작
	public static WarningPopUp Instance;

	public Text WarningText;
	Animator PopUpAnimator;

	private void Awake()
	{
		if(Instance == null) Instance = this;

		//싱글턴이기 때문에 active 된 상태이며, 현재 0으로 설정된 상태
		//따라서 1로 초기화
		this.transform.localScale = Vector3.one;
		//비활성화
		this.gameObject.SetActive(false);
		PopUpAnimator = GetComponent<Animator>();
	}


	//해당 함수는 LobbyUIManager에서 RoomCreate 조건에 맞지 않으면 호출된다.
	public void PopUpWarning(string temp)
	{
		//오브젝트 활성화
		this.gameObject.SetActive(true);
		WarningText.text = temp;
		//애니메이션을 재생한다.
		PopUpAnimator.Play("PopUpAnim");
	}

	//애니메이션은 자동으로 오브젝트를 닫는 애니메이션으로 연결되고
	//아래 이벤트는 닫는 애니메이션의 마지막에 적용되는 오브젝트 비활성화 함수다.
	public void Deactive() => this.gameObject.SetActive(false);
}
