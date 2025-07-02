using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Game_UIManager : MonoBehaviourPun
{
	//싱글턴으로 제작
	public static Game_UIManager instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//거점 상태 정보를 알려주는 UI
	public GameObject SiteStatusUI;
	//각 거점 상태 정보를 저장하는 UI 오브젝트의 이름과 오브젝트를 딕셔너리로 저장
	public Dictionary<string, GameObject> SiteStatusDic = new Dictionary<string, GameObject>();
	//거점이 점령되면 활성화 되는 UI
	public GameObject CapturedAlertUI;
	public bool AlertUIFlag;


	//내 역할을 알려주는 pannel을 없애는 애니메이션 재생을 위한 선언
	public Animator PanelAnim;
	//역할을 알려주기 위한 UI들
	public GameObject Panel;
    public Text Role;
	public Text Desc;
	public Text Timer;

	//각종 flag들
    public bool isTyping;
	public bool isIniting;

	//각종 값들
    [SerializeField]
    private float typingSpeed;
	[SerializeField]
	private float waitingTime;
	[SerializeField]
	private float seekerWaitingTime;

	private void Start()
	{
		Panel.transform.localScale = Vector3.one;
		CapturedAlertUI.transform.localScale = Vector3.one;
		CapturedAlertUI.SetActive(false);

		//해당 UI 오브젝트의 자식에 달려있는 오브젝트들을 array로 저장
		Transform[] childTransforms = SiteStatusUI.GetComponentsInChildren<Transform>();
		//해당 array를 참고하여 딕셔너리 초기화
		foreach (Transform t in childTransforms)
		{
			SiteStatusDic[t.gameObject.name] = t.gameObject;
		}
	}

	[PunRPC]//RPC로 호출하기 위해 선언된 함수
	public void AlertAllPointCapturedF(string str, Vector2 colorVec1, Vector2 colorVec2)
	{
		//Vector2 값을 합쳐서 색상 값으로 변환 후
		Color TextColor = new Color(colorVec1.x, colorVec1.y, colorVec2.x, colorVec2.y);
		//코루틴을 수행한다.
		StartCoroutine(Game_UIManager.instance.AlertAllPointCaptured(str, TextColor));
	}

	//특정 거점이 점령되었다는 UI가 종료된 후에, 해당 UI를 띄우도록 하기 위해 다음과 같이 딜레이를 준다.
	public IEnumerator AlertAllPointCaptured(string str, Color color)
	{
		yield return new WaitForSeconds(0.5f);
		yield return new WaitUntil(() => !AlertUIFlag);
		CapturedAlertActivated(str, color);
	}

	//거점 점령 UI를 활성화해주는 함수
	public void CapturedAlertActivated(string name)
	{
		AlertUIFlag = true;
		//전달받은 거점 이름을 UI에 포함시킨다.
		Text Alert = CapturedAlertUI.GetComponent<Text>();
		Alert.text = name + " has been captured";
		//UI 활성화 하고
		CapturedAlertUI.SetActive(true);
		//애니메이션을 재생하여 UI가 표시되도록 한다.
		Animator AlertAnim = CapturedAlertUI.GetComponent<Animator>();
		AlertAnim.Play("CaptureAlertPopUp");

		//해당 UI는 일정 이상이 지나면 이벤트로 인해 자동적으로 비활성화된다.
	}

	//override
	public void  CapturedAlertActivated(string str, Color coler)
	{
		AlertUIFlag = true;
		//전달받은 거점 이름을 UI에 포함시킨다.
		Text Alert = CapturedAlertUI.GetComponent<Text>();
		Alert.color = coler; 
		Alert.text = str;
		//UI 활성화 하고
		CapturedAlertUI.SetActive(true);
		//애니메이션을 재생하여 UI가 표시되도록 한다.
		Animator AlertAnim = CapturedAlertUI.GetComponent<Animator>();
		AlertAnim.Play("CaptureAlertPopUp");
	}


	//Runner UI 설정을 담당하는 코루틴
	public IEnumerator InitRunnerUI()
	{
		isIniting = true;

		Role.text = "";
		Desc.text = "";
		Timer.text = "";

		//Runner임을 알리는 텍스트를 타이핑 형식으로 작성해나가는 함수 호출
		//인자로 사용된 색상 값은 hex 값을 string 형식으로 넘겨주며, 자동 변환을 해주는 함수를 사용함
		StartCoroutine(TypeSentence("Runner", ColorToHex(Color.green)));
		//위 타이핑이 끝날때까지 대기 후
		yield return new WaitUntil(() => !isTyping);
		//해당 역할에 대한 간단한 설명을 해주는 문자열을 타이핑하도록 함수 호출
		//해당 함수는 오버로드로 구현 됨
		StartCoroutine(TypeSentence("Take all the strongholds and escape without the seeker's knowledge!"));

		//모두 작성이 끝나면
		yield return new WaitUntil(() => !isTyping);

		//패널을 닫는 애니메이션을 재생하고
		PanelAnim.Play("RoleClose");
		//코루틴이 종료되었음을 알린다.
		isIniting = false;
	}

	//Seeker UI 설정을 담당하는 코루틴
	public IEnumerator InitSeekerUI()
	{
		isIniting = true;

		Role.text = "";
		Desc.text = "";
		Timer.text = "";

		//Seeker임을 알리는 텍스트를 타이핑 형식으로 작성해나가는 함수 호출
		//인자로 사용된 색상 값은 hex 값을 string 형식으로 넘겨주며, 자동 변환을 해주는 함수를 사용함
		StartCoroutine(TypeSentence("Seeker", ColorToHex(Color.red)));
		//위 타이핑이 끝날 때까지 대기 한다.
		yield return new WaitUntil(() => !isTyping);
		//Seeker 역할에 대한 간단한 설명을 타이핑을 통해 보여주고
		StartCoroutine(TypeSentence("Catch all the runners pretending to be bots and keep them away from the base!"));
		//타이핑 작성이 끝나면
		yield return new WaitUntil(() => !isTyping);

		//Seeker는 별도의 대기 시간을 부여한다.(30초)
		float remaining = seekerWaitingTime;
		//아래 코드를 통해 타이머를 재생한다.
		while(remaining >= 0)
		{
			if(remaining <= 10) Timer.color = Color.red;
			Timer.text = $"{remaining:F0}";
			yield return new WaitForSeconds(1f);
			remaining -= 1f;
		}
		//타이머가 끝나면, panel을 닫는다.
		PanelAnim.Play("RoleClose");
		//코루틴이 끝났음을 알린다.
		isIniting = false;
	}

	//타이핑을 담당하는 함수
	//인자로 Seeker 혹은 Runner 문자열을 받고, 해당 문자열을 출력할 색상 값을 Hex 문자열로 받는다.
	private IEnumerator TypeSentence(string myRole, string colorHex)
    {
        isTyping = true;
		//기본 문자열
        string str = "You are ";
		//색상 값을 입히기 위한 문자열들
		string colorStartTag = $"<color={colorHex}>";
		string colorEndTag = "</color>";

		Role.text = "";

		//기본 문자열을 차례대로 타이핑한다.
		foreach (char c in str.ToCharArray())
		{
			Role.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}

		//역할을 출력하기 위한 별도의 문자열 선언
		string roleTyped = "";

		foreach (char c in myRole.ToCharArray())
		{
			//우선 역할 글자의 한 글자를 해당 문자열에 저장 후
			roleTyped += c;
			//사용자 UI에 다음과 같은 연산으로 출력한다.
			//이렇게 하지 않을 시, 색상값을 입히기 위한 앞뒤 명령줄이 출력되버린다.
			Role.text = str + colorStartTag + roleTyped + colorEndTag;

			yield return new WaitForSeconds(typingSpeed);
		}

		//타이핑이 끝났음을 알린다.
		isTyping = false;
	}

	//overload 로 구현된 함수
	//해당 함수는 그냥 전달받은 문자열을 타이핑해서 출력한다.
	private IEnumerator TypeSentence(string text)
	{
		isTyping = true;
		Desc.text = "";

		foreach(char c in text.ToCharArray())
		{
			Desc.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}

		isTyping = false;
	}

	//Site 점령 여부를 UI에 표시하는 함수
	public void SetSiteComplete(string SiteName)
	{
		//매개변수로 받은 점령된 거점 이름을 Dictionary에서 검색한다.
		//동일한 이름을 가진 요소가 존재하면, 해당 요소의 값(gameObject)를 반환한다.
		if(SiteStatusDic.TryGetValue(SiteName, out var site))
		{
			//해당 오브젝트의 Image 요소를 받아온다.
			Image s = site.GetComponent<Image>();
			
			//Image가 null이 아니라면
            if(s != null)
			{
				//해당 색상은 그대로
				Color c = s.color;
				//알파값을 50%로 표시한다.
				c.a = 0.5f;
				//색상 초기화
				s.color = c;
			}
			else //예외 처리
			{
				Debug.LogWarning($"{SiteName}에 Image 컴포넌트가 없습니다.");
			}
        }
		else //예외 처리
		{
			Debug.LogWarning($"{SiteName}이 SiteStatusDic에 없습니다.");
		}
	}

	//인자로 받은 색상을 hex 값으로 변환해서 문자열로 반환하는 함수
	private string ColorToHex(Color color)
	{
		Color32 color32 = color;
		return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}";
	}
}
